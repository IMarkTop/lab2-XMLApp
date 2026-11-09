using Avalonia.Controls;
using Avalonia.Interactivity;

namespace XMLApp.Views;

/// <summary>
/// Діалогове вікно підтвердження виходу з програми.
/// Містить виключно логіку повернення результату діалогу.
/// </summary>
public partial class ConfirmExitWindow : Window
{
    public ConfirmExitWindow()
    {
        InitializeComponent();
    }

    private void YesButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void NoButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
