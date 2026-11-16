using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using HexIDE.Themes;
using HexIDE.Tools;
using HexIDE.VisualDesigner.Views;

namespace HexIDE.Integration.Tests.Views;

public class DarkThemeRenderTests : WindowTestFixture
{
    [AvaloniaTheory]
    [InlineData("Abyss", "#000C18")]
    [InlineData("Dark", "#252526")]
    public async Task MainWindowPanesUseThePacksBackground(string id, string expected)
    {
        var previous = Static.StartupProjectHook;
        Static.StartupProjectHook = (_, _) => Task.CompletedTask;
        var service = new ThemeService();
        await using var owned = new RealComposition();
        var dictionaries = Application.Current!.Resources.MergedDictionaries;
        var originalDictionaries = dictionaries.ToArray();
        owned.Composition.LocalizationService.Apply("en");
        var root = owned.Composition.Root;
        root.ResetWindowLayout();
        var window = new MainWindow { DataContext = root, Width = 1200, Height = 800 };
        var locator = new ViewLocator();
        Application.Current!.DataTemplates.Add(locator);
        try
        {
            service.Apply(id);
            window.Show();
            using var frame = window.CaptureRenderedFrame();
            frame.Should().NotBeNull();
            var evidence = Environment.GetEnvironmentVariable("HEXIDE_THEME_EVIDENCE");
            if (evidence is not null)
            {
                Directory.CreateDirectory(evidence);
                frame!.Save(Path.Combine(evidence, id + ".png"), PngBitmapEncoderOptions.Default);
            }
            var readings = new List<string>();
            using var assertions = new AwesomeAssertions.Execution.AssertionScope();
            foreach (var name in new[] { nameof(ProjectToolView), nameof(PropertiesToolView), nameof(ToolBoxToolView) })
            {
                var pane = window.GetVisualDescendants().OfType<Control>()
                    .Single(c => c.GetType().Name == name && c.IsEffectivelyVisible);
                var origin = pane.TranslatePoint(default, window)!.Value;
                var region = new PixelRect((int)origin.X + 4, (int)origin.Y + 4,
                    Math.Max(1, (int)pane.Bounds.Width - 8), Math.Max(1, (int)pane.Bounds.Height - 8));
                var colour = MostCommon(frame!, region);
                readings.Add($"{id} {name}: {colour}");
                Console.WriteLine(readings[^1]);
                colour.Should().Be(Color.Parse(expected), $"{id}'s {name} should use its navy/grey palette");
            }
            foreach (var key in ColorKeyMapping.Table.Keys.Where(key => key.StartsWith("Theme", StringComparison.Ordinal)
                         || key.StartsWith("Dock", StringComparison.Ordinal)))
            {
                var colour = ((ISolidColorBrush)window.FindResource(window.ActualThemeVariant, ColorKeyMapping.Table[key].Key)!).Color;
                readings.Add($"{id} {key}: {colour}");
                Console.WriteLine(readings[^1]);
            }
            if (evidence is not null) File.WriteAllLines(Path.Combine(evidence, id + ".txt"), readings);
        }
        finally
        {
            window.Close();
            Application.Current!.DataTemplates.Remove(locator);
            service.Apply("Classic");
            foreach (var dictionary in dictionaries.Except(originalDictionaries).ToArray()) dictionaries.Remove(dictionary);
            Static.StartupProjectHook = previous;
        }
    }

    private static Color MostCommon(Bitmap bitmap, PixelRect region)
    {
        var pixels = new int[region.Width * region.Height];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try { bitmap.CopyPixels(region, handle.AddrOfPinnedObject(), pixels.Length * 4, region.Width * 4); }
        finally { handle.Free(); }
        var pixel = unchecked((uint)pixels.GroupBy(p => p).MaxBy(g => g.Count())!.Key);
        return bitmap.Format == Avalonia.Platform.PixelFormat.Rgba8888
            ? Color.FromArgb((byte)(pixel >> 24), (byte)pixel, (byte)(pixel >> 8), (byte)(pixel >> 16))
            : Color.FromArgb((byte)(pixel >> 24), (byte)(pixel >> 16), (byte)(pixel >> 8), (byte)pixel);
    }
}
