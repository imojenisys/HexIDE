using Avalonia.Headless.XUnit;
using HexIDE.Events;
using HexIDE.IDE;
using HexIDE.Localization;
using HexIDE.Runtime.Components;
using HexIDE.Runtime.ProjectElements;
using HexIDE.Runtime.Serialization;
using HexIDE.VisualDesigner;
using NSubstitute;

namespace HexIDE.Integration.Tests.Views;

/// <summary>
/// A committed change in the designer announces itself once, with the model already flushed
/// (hexide-io/HexIDE#273 task 3.3).
/// </summary>
/// <remarks>
/// The header the code window shows in front of a form's code is rendered from
/// <c>FormDefinition.Components</c>, and the designer's working collections run ahead of that list until
/// something flushes them. So the two halves of this are inseparable: an announcement without the flush
/// renders the form as it was one control ago, and a flush without the announcement leaves the window
/// showing a header for a form that no longer exists.
/// </remarks>
public class DesignerCommitTests
{
    private static readonly ProjectDefinition Project = new(VBProjectType.EXE, "P");

    private sealed class NullSink : IDeserializeErrorSink
    {
        public static readonly NullSink Instance = new();
        public void LogError(string _) { }
    }

    private static FormDefinition Deserialize(string source) =>
        new FormDeserializer().Deserialize(Project, source, NullSink.Instance)!;

    private const string OneLabel = """
        VERSION 5.00
        Begin VB.Form Form1
           Begin VB.Label Label1
              Caption         =   "Hi"
           End
        End
        Attribute VB_Name = "Form1"
        """;

    /// <summary>
    /// A command that does nothing on either side.
    /// </summary>
    /// <remarks>
    /// The subject here is the stack, not any command: a real one (LockControlsCommand) saves the form from
    /// its Undo, which would drag the whole project service into a test about when a notification is raised.
    /// </remarks>
    private sealed class NoopCommand : IDesignerCommand
    {
        public string Description => "noop";
        public void Execute(FormEditViewModel vm) { }
        public void Undo(FormEditViewModel vm) { }
    }

    private static FormEditViewModel NewDesignerVm(IEventBus bus) => new(
        null!, bus, null!, null!, null!, null!, Substitute.For<ILocalizationService>());

    [AvaloniaFact]
    public void ACommitAnnouncesItselfWithTheModelAlreadyFlushed()
    {
        // The ordering IS the test. Both facts are read inside the handler, because checking them after the
        // publish would pass just as happily if the flush ran second -- and second is useless: whatever
        // re-renders the form does so while handling this event.
        var bus = new EventBus();
        var form = Deserialize(OneLabel);
        var vm = NewDesignerVm(bus).Initialize(form);

        var spawned = new ComponentInstance(CommandButtonComponentClass.Instance, "Command1");
        var spawnedVm = new ComponentInstanceViewModel(vm, spawned);
        vm.AllComponents.Add(spawnedVm);
        vm.Components.Add(spawnedVm);

        var announcements = 0;
        var modelHadItWhenAnnounced = false;
        bus.Subscribe<FormLayoutChangedEvent>(e =>
        {
            announcements++;
            modelHadItWhenAnnounced = e.Form.Components.Contains(spawned);
        });

        vm.UndoStack.Push(new NoopCommand());

        announcements.Should().Be(1);
        modelHadItWhenAnnounced.Should().BeTrue(
            "a render taken on this event would otherwise miss the control just added");
    }

    [AvaloniaFact]
    public void UndoAndRedoAreCommitsToo()
    {
        // They change the model exactly as the original gesture did, and nothing else would announce it.
        var bus = new EventBus();
        var vm = NewDesignerVm(bus).Initialize(Deserialize(OneLabel));
        vm.UndoStack.Push(new NoopCommand());

        var announcements = 0;
        bus.Subscribe<FormLayoutChangedEvent>(_ => announcements++);

        vm.UndoStack.Undo();
        vm.UndoStack.Redo();

        announcements.Should().Be(2);
    }

    [AvaloniaFact]
    public void ClearingTheUndoStackIsNotACommit()
    {
        // Clear runs when the designer is rebuilt from a freshly-reloaded model. Treating it as a commit
        // would re-render the form and replace the text just read from disk with a reproduction of it --
        // which is precisely what the invariant forbids outside a save.
        var bus = new EventBus();
        var vm = NewDesignerVm(bus).Initialize(Deserialize(OneLabel));
        vm.UndoStack.Push(new NoopCommand());

        var announcements = 0;
        bus.Subscribe<FormLayoutChangedEvent>(_ => announcements++);

        vm.UndoStack.Clear();

        announcements.Should().Be(0);
    }

    [AvaloniaFact]
    public void ADragAnnouncesOnceWhenItEndsRatherThanPerPointerMove()
    {
        // Every pointer move writes the model through a two-way Canvas.Left binding. Announcing per write
        // would mean a full re-render of the designer text and a whole-document didChange to every attached
        // language server for each pixel of a drag; the undo stack's existing mid-drag discard is what makes
        // once-per-gesture free.
        var bus = new EventBus();
        var form = Deserialize(OneLabel);
        var vm = NewDesignerVm(bus).Initialize(form);
        var target = vm.Components[0];

        var announcements = 0;
        bus.Subscribe<FormLayoutChangedEvent>(_ => announcements++);

        vm.BeginDrag([target]);
        for (var x = 1; x <= 20; x++)
        {
            target.Left = x * 10;
            vm.UndoStack.Push(new NoopCommand());   // as a gesture mid-drag would
        }
        announcements.Should().Be(0, "nothing is committed until the gesture ends");

        vm.EndDrag();

        announcements.Should().Be(1);
    }

    [AvaloniaFact]
    public void RenamingTheFormBecomesObservableOnTheCommit()
    {
        // The half of task 2.4a deferred to 3.3. FormDefinition.Name is derived from the root component's
        // (Name) property and has no setter, so it only raises PropertyChanged when UpdateComponents runs --
        // which is what the flush does. Before this, renaming a form in the property grid changed nothing
        // the tab title or the language layer could see until a save happened to flush.
        var bus = new EventBus();
        var form = Deserialize(OneLabel);
        var vm = NewDesignerVm(bus).Initialize(form);

        var renames = 0;
        form.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(FormDefinition.Name)) renames++; };

        vm.Form!.Instance.SetProperty(VBProperties.NameProperty, "Renamed");
        renames.Should().Be(0, "a property written straight onto the root component announces nothing");

        vm.UndoStack.Push(new NoopCommand());

        renames.Should().Be(1);
        form.Name.Should().Be("Renamed");
    }
}
