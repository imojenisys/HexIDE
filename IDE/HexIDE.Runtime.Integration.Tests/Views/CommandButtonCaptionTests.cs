using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Classic.Avalonia.Theme;
using HexIDE.Integration.Tests.Views;
using HexIDE.Runtime.BuiltinControls;

namespace HexIDE.Runtime.Integration.Tests.Views;

// VB6 SP6 measurements quoted in #4551's final brief (oracle PR #5031), also covering #4586.
public class CommandButtonCaptionTests
{
    private static string Form(int width, int height, bool disabled, string caption = "aZ_g") => $$"""
        VERSION 5.00
        Begin VB.Form Form1
           ClientWidth = 6000
           ClientHeight = 4500
           Begin VB.CommandButton Command1
              Caption = "{{caption}}"
              Width = {{width * 15}}
              Height = {{height * 15}}
              Enabled = {{(disabled ? 0 : -1)}}
           End
        End
        """;

    [AvaloniaTheory]
    [InlineData(1.0, 20)]
    [InlineData(1.5, 20)]
    [InlineData(1.0, 15)]
    [InlineData(1.5, 15)]
    public void Caption_keeps_its_natural_size_and_measured_origin(double scaling, int height)
    {
        var (canvas, window, _, _) = ContainerRuntimeHarness.Laid(Form(80, height, false));
        try
        {
            window.SetRenderScaling(scaling);
            Dispatcher.UIThread.RunJobs();
            var button = (VBCommandButton)ContainerRuntimeHarness.Child(canvas, "Command1");
            var text = button.GetVisualDescendants().OfType<TextBlock>().Single(t => t.IsVisible);
            text.Bounds.Size.Should().Be(text.DesiredSize);
            var origin = text.TranslatePoint(default, button)!.Value;
            var tolerance = scaling == 1 ? 0 : 1 / scaling;
            origin.X.Should().BeApproximately(Math.Truncate((79 - text.DesiredSize.Width) / 2), tolerance);
            origin.Y.Should().BeApproximately(Math.Truncate((height - 1 - text.DesiredSize.Height) / 2), tolerance);
            button.Padding.Should().Be(new Thickness(0));
            var clipHost = text.GetVisualAncestors().First(v => v.Clip != null);
            var clip = clipHost.Clip!.Bounds;
            clip.X.Should().Be(2);
            clip.Y.Should().Be(2);
            clip.Width.Should().Be(76);
            clip.Height.Should().BeApproximately(height - 4, tolerance);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(1.0)]
    [InlineData(1.5)]
    public void Disabled_caption_is_embossed_on_an_opaque_unchanged_face(double scaling)
    {
        var (canvas, window, _, _) = ContainerRuntimeHarness.Laid(Form(100, 33, true, "&Disabled"));
        try
        {
            window.SetRenderScaling(scaling);
            Dispatcher.UIThread.RunJobs();
            var button = (VBCommandButton)ContainerRuntimeHarness.Child(canvas, "Command1");
            button.Opacity.Should().Be(1);
            var edge = button.GetVisualDescendants().OfType<ClassicBorderDecorator>().Single();
            var caption = button.GetVisualDescendants().OfType<ContentPresenter>().Single(p => p.Name == "PART_ContentPresenter");
            var highlight = button.GetVisualDescendants().OfType<ContentPresenter>().Single(p => p.Name == "CaptionHighlight");
            caption.Foreground.Should().Be(edge.BorderDarkBrush);
            highlight.Foreground.Should().Be(edge.BorderLightLightBrush);
            highlight.IsVisible.Should().BeTrue();
            var tolerance = scaling == 1 ? 0 : 1 / scaling;
            var delta = highlight.TranslatePoint(default, button)!.Value - caption.TranslatePoint(default, button)!.Value;
            delta.X.Should().BeApproximately(1, tolerance);
            delta.Y.Should().BeApproximately(1, tolerance);
            var primaryText = caption.GetVisualDescendants().OfType<TextBlock>().Single();
            var copyText = highlight.GetVisualDescendants().OfType<TextBlock>().Single();
            copyText.GetType().Should().Be(primaryText.GetType());
            copyText.Text.Should().Be(primaryText.Text);
            copyText.FontFamily.Should().Be(primaryText.FontFamily);
            ((AccessText)copyText).AccessKey.Should().Be(((AccessText)primaryText).AccessKey);
            var clipHost = caption.GetVisualParent()!;
            highlight.GetVisualParent().Should().BeSameAs(clipHost);
            clipHost.Clip!.Bounds.Should().Be(new Rect(2, 2, button.Bounds.Width - 4, button.Bounds.Height - 4));
            var disabledOrigin = caption.TranslatePoint(default, button);
            var background = button.Background;
            button.IsEnabled = true;
            Dispatcher.UIThread.RunJobs();
            button.Background.Should().Be(background);
            highlight.IsVisible.Should().BeFalse();
            caption.TranslatePoint(default, button).Should().Be(disabledOrigin);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(1.0)]
    [InlineData(1.5)]
    public void Disabled_background_matches_enabled_background(double scaling)
    {
        var (canvas, window, _, _) = ContainerRuntimeHarness.Laid(Form(100, 33, false));
        try
        {
            window.SetRenderScaling(scaling);
            Dispatcher.UIThread.RunJobs();
            var button = (VBCommandButton)ContainerRuntimeHarness.Child(canvas, "Command1");
            var background = button.Background;
            ((ISolidColorBrush)background!).Color.A.Should().Be(255);
            button.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();
            button.Background.Should().Be(background);
        }
        finally { window.Close(); }
    }
}
