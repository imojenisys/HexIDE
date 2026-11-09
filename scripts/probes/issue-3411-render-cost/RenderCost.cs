// #3411: what does it cost to draw a designer's control in another process and hand back a picture?
//
//   dotnet run -c Release -- measure [iterations]     runs the table, starting this program again as the child
//   dotnet run -c Release -- serve                    the child: reads requests on stdin, writes pictures on stdout
//
// Each row draws a control the way ControlRenderer does: build it, attach it, lay it out, render it to a bitmap.
//   draw        that, in this process. Today's cost.
//   raw copy    draw, then copy the pixels out.
//   png         draw, then encode a PNG.
//   child raw   ask the child to draw and send raw pixels, and make a bitmap of them here.
//   child png   the same with a PNG, decoded here.
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Themes.Simple;
using HexIDE.Runtime.Components;
using HexIDE.Runtime.Components.InBox;

internal sealed class ProbeApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new SimpleTheme());
        // As App.axaml does. Without the IDE's colours every system colour falls back to DeepPink, and a control is pink
        // on pink, which draws in next to no time and encodes to next to nothing.
        Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://HexIDE/"))
        {
            Source = new Uri("avares://HexIDE/Themes/Classic.axaml"),
        });
        foreach (var name in new[] { "VBTimer", "VBLabel", "Resources" })
            Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://HexIDE.Runtime/"))
            {
                Source = new Uri($"avares://HexIDE.Runtime/BuiltinControls/{name}.axaml"),
            });
        RequestedThemeVariant = ThemeVariant.Light;
    }
}

internal static class Program
{
    private static readonly (string Name, ComponentBaseClass Type)[] Classes =
    [
        ("CommandButton", CommandButtonComponentClass.Instance),
        ("TextBox", TextBoxComponentClass.Instance),
        ("Frame", FrameComponentClass.Instance),
        ("MonthView", MonthViewComponentClass.Instance),
    ];

    private static Decorator host = null!;

    private static int Main(string[] args)
    {
        AppBuilder.Configure<ProbeApp>().UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
        host = new Decorator();
        new Window { Width = 1000, Height = 800, Content = host }.Show();
        if (args.Length > 0 && args[0] == "serve")
            return Serve();
        if (args.Length > 1 && args[0] == "sample")
        {
            Directory.CreateDirectory(args[1]);
            for (var i = 0; i < Classes.Length; i++)
                foreach (var (w, h) in new[] { (81, 33), (640, 480) })
                {
                    using var bitmap = Draw(Classes[i].Type, w, h, 1, "Caption");
                    bitmap.Save(Path.Combine(args[1], $"{Classes[i].Name}-{w}.png"));
                }
            return 0;
        }
        return Measure(args.Length > 1 ? int.Parse(args[1]) : 200);
    }

    private static RenderTargetBitmap Draw(ComponentBaseClass type, int width, int height, double scale, string caption)
    {
        var instance = new ComponentInstance(type, "Control1");
        instance.SetProperty(VBProperties.WidthProperty, width * 15.0);
        instance.SetProperty(VBProperties.HeightProperty, height * 15.0);
        if (type.PropertiesByName.ContainsKey("Caption")) instance.SetProperty(VBProperties.CaptionProperty, caption);
        if (type.PropertiesByName.ContainsKey("Text")) instance.SetProperty(VBProperties.TextProperty, caption);
        var visual = type.InstantiateForDesigner(instance);
        var target = new RenderTargetBitmap(new PixelSize((int)(width * scale), (int)(height * scale)), new Vector(96 * scale, 96 * scale));
        host.Child = visual;
        visual.Width = width;
        visual.Height = height;
        visual.Measure(new Size(width, height));
        visual.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();
        target.Render(visual);
        host.Child = null;
        return target;
    }

    private static byte[] Raw(RenderTargetBitmap bitmap)
    {
        var size = bitmap.PixelSize;
        var stride = size.Width * 4;
        var bytes = new byte[stride * size.Height];
        var pin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { bitmap.CopyPixels(new PixelRect(size), pin.AddrOfPinnedObject(), bytes.Length, stride); }
        finally { pin.Free(); }
        return bytes;
    }

    private static byte[] Png(RenderTargetBitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream);
        return stream.ToArray();
    }

    private static int Serve()
    {
        using var input = new BinaryReader(Console.OpenStandardInput());
        using var output = new BinaryWriter(Console.OpenStandardOutput());
        while (true)
        {
            int index;
            try { index = input.ReadInt32(); }
            catch (EndOfStreamException) { return 0; }
            var width = input.ReadInt32();
            var height = input.ReadInt32();
            var scale = input.ReadDouble();
            var png = input.ReadBoolean();
            var caption = input.ReadString();
            using var bitmap = Draw(Classes[index].Type, width, height, scale, caption);
            var bytes = png ? Png(bitmap) : Raw(bitmap);
            output.Write(bitmap.PixelSize.Width);
            output.Write(bitmap.PixelSize.Height);
            output.Write(bytes.Length);
            output.Write(bytes);
            output.Flush();
        }
    }

    private sealed class Child : IDisposable
    {
        private readonly Process process;
        private readonly BinaryWriter requests;
        private readonly BinaryReader replies;

        public Child()
        {
            process = Process.Start(new ProcessStartInfo(Environment.ProcessPath!, "serve")
            {
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false,
            })!;
            process.ErrorDataReceived += (_, _) => { };
            process.BeginErrorReadLine();
            requests = new BinaryWriter(process.StandardInput.BaseStream);
            replies = new BinaryReader(process.StandardOutput.BaseStream);
        }

        public (int Width, int Height, byte[] Bytes) Ask(int index, int width, int height, double scale, bool png, string caption)
        {
            requests.Write(index);
            requests.Write(width);
            requests.Write(height);
            requests.Write(scale);
            requests.Write(png);
            requests.Write(caption);
            requests.Flush();
            var w = replies.ReadInt32();
            var h = replies.ReadInt32();
            var bytes = replies.ReadBytes(replies.ReadInt32());
            return (w, h, bytes);
        }

        public void Dispose()
        {
            process.StandardInput.Close();
            if (!process.WaitForExit(5000)) process.Kill(true);
            process.Dispose();
        }
    }

    private static Bitmap FromRaw(int width, int height, byte[] bytes)
    {
        var pin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Premul, pin.AddrOfPinnedObject(), new PixelSize(width, height), new Vector(96, 96), width * 4); }
        finally { pin.Free(); }
    }

    private static (double Median, double P95) Time(int iterations, Action<int> work)
    {
        for (var i = 0; i < 30; i++) work(i);
        var times = new double[iterations];
        for (var i = 0; i < iterations; i++)
        {
            var clock = Stopwatch.StartNew();
            work(i);
            times[i] = clock.Elapsed.TotalMilliseconds;
        }
        Array.Sort(times);
        return (times[iterations / 2], times[(int)(iterations * 0.95)]);
    }

    private static int Measure(int iterations)
    {
        var start = Stopwatch.StartNew();
        using var child = new Child();
        child.Ask(0, 81, 33, 1, false, "first");
        Console.WriteLine($"child's start to its first picture: {start.Elapsed.TotalMilliseconds:F0} ms");
        Console.WriteLine($"iterations: {iterations} after 30 to warm up. Milliseconds, median / p95.");
        Console.WriteLine("| Class | DIP | Scale | draw | + raw copy | + png | child, raw | child, png | raw bytes | png bytes |");
        Console.WriteLine("|---|---|---|---|---|---|---|---|---|---|");
        foreach (var (width, height) in new[] { (81, 33), (640, 480) })
        foreach (var scale in new[] { 1.0, 1.5 })
        for (var index = 0; index < Classes.Length; index++)
        {
            var (name, type) = Classes[index];
            var which = index;
            string F((double Median, double P95) t) => $"{t.Median:F2} / {t.P95:F2}";
            try
            {
                var draw = Time(iterations, i => { using var b = Draw(type, width, height, scale, "Caption " + i); });
                var raw = Time(iterations, i => { using var b = Draw(type, width, height, scale, "Caption " + i); Raw(b); });
                var png = Time(iterations, i => { using var b = Draw(type, width, height, scale, "Caption " + i); Png(b); });
                var rawBytes = 0;
                var childRaw = Time(iterations, i =>
                {
                    var (w, h, bytes) = child.Ask(which, width, height, scale, false, "Caption " + i);
                    rawBytes = bytes.Length;
                    using var picture = FromRaw(w, h, bytes);
                });
                var pngBytes = 0;
                var childPng = Time(iterations, i =>
                {
                    var (_, _, bytes) = child.Ask(which, width, height, scale, true, "Caption " + i);
                    pngBytes = bytes.Length;
                    using var stream = new MemoryStream(bytes);
                    using var picture = new Bitmap(stream);
                });
                Console.WriteLine($"| {name} | {width}x{height} | {scale:0.#} | {F(draw)} | {F(raw)} | {F(png)} | {F(childRaw)} | {F(childPng)} | {rawBytes:N0} | {pngBytes:N0} |");
            }
            catch (Exception error) when (error is NotImplementedException or InvalidOperationException or EndOfStreamException)
            {
                Console.WriteLine($"| {name} | {width}x{height} | {scale:0.#} | failed: {error.GetType().Name}: {error.Message} |");
                return 1;
            }
        }
        return 0;
    }
}
