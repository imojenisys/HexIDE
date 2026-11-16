using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Interactivity;
using HexIDE.Forms.ViewModels;

namespace HexIDE.Forms.Views;

public partial class FontDialog : UserControl
{
    public FontDialog()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            if (TopLevel.GetTopLevel(this) == null) return;
            FontNameBox.Focus();
            FontNameBox.SelectAll();
        }, DispatcherPriority.Loaded);
    }

    private void OkClick(object? sender, RoutedEventArgs e) =>
        (DataContext as FontDialogViewModel)?.Accept();

    private void CancelClick(object? sender, RoutedEventArgs e) =>
        (DataContext as FontDialogViewModel)?.Cancel();
}
