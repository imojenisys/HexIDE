using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using HexIDE.IDE;
using HexIDE.Localization;

namespace HexIDE.Integration.Tests.Views;

public class ProgramThemeTests
{
    [AvaloniaTheory]
    [InlineData("Color")]
    [InlineData("Font")]
    [InlineData("Print")]
    public async Task CommonDialog_windows_opened_by_the_program_stay_light(string kind)
    {
        var app = Application.Current!;
        var previous = app.RequestedThemeVariant;
        var lifetimeField = typeof(Application).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(field => field.FieldType == typeof(IApplicationLifetime));
        var lifetime = lifetimeField.GetValue(app);
        var owner = new Window();
        var desktop = Substitute.For<IClassicDesktopStyleApplicationLifetime>();
        desktop.MainWindow.Returns(owner);
        desktop.Windows.Returns(new[] { owner });
        try
        {
            app.RequestedThemeVariant = ThemeVariant.Dark;
            lifetimeField.SetValue(app, desktop);
            owner.Show();
            var localization = new LocalizationService();
            var host = new ProgramHost(new WindowManager(localization), localization);
            Task pending = kind switch
            {
                "Color" => host.ColorDialog(0),
                "Font" => host.FontDialog(new VbFontChoice("", 10, false, false)),
                _ => host.PrintDialog(),
            };
            var shown = owner.OwnedWindows.Single();
            try { shown.ActualThemeVariant.Should().Be(ThemeVariant.Light); }
            finally { shown.Close(); }
            await pending;
        }
        finally
        {
            foreach (var window in owner.OwnedWindows.ToArray()) window.Close();
            owner.Close();
            lifetimeField.SetValue(app, lifetime);
            app.RequestedThemeVariant = previous;
        }
    }

    [AvaloniaTheory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task ProgramDialogs_stay_light_while_IDE_message_boxes_follow_the_application(bool input, bool program)
    {
        var app = Application.Current!;
        var previous = app.RequestedThemeVariant;
        var lifetimeField = typeof(Application).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(field => field.FieldType == typeof(IApplicationLifetime));
        var lifetime = lifetimeField.GetValue(app);
        var owner = new Window();
        var desktop = Substitute.For<IClassicDesktopStyleApplicationLifetime>();
        desktop.MainWindow.Returns(owner);
        desktop.Windows.Returns(new[] { owner });
        try
        {
            app.RequestedThemeVariant = ThemeVariant.Dark;
            lifetimeField.SetValue(app, desktop);
            owner.Show();
            var manager = new WindowManager(new LocalizationService());
            Task pending = input ? manager.InputBox("probe", "caption", "", isProgramDialog: program)
                : manager.MessageBox("probe", "caption", MessageBoxButtons.Ok, MessageBoxIcon.None, owner, isProgramDialog: program);
            var shown = owner.OwnedWindows.Single();
            try
            {
                owner.ActualThemeVariant.Should().Be(ThemeVariant.Dark);
                shown.ActualThemeVariant.Should().Be(program ? ThemeVariant.Light : ThemeVariant.Dark);
            }
            finally { shown.Close(); }
            await pending;
        }
        finally
        {
            foreach (var window in owner.OwnedWindows.ToArray()) window.Close();
            owner.Close();
            lifetimeField.SetValue(app, lifetime);
            app.RequestedThemeVariant = previous;
        }
    }
}
