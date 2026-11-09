using Avalonia.Controls;
using Avalonia.Interactivity;

namespace XMLApp.Views;

/// <summary>
/// Просте модальне вікно повідомлень для інтерфейсу.
/// </summary>
public partial class MessageBoxWindow : Window
{
    public MessageBoxWindow()
    {
        InitializeComponent();
    }

    public MessageBoxWindow(string title, string message) : this()
    {
        Title = title;
        TitleText.Text = title;
        MessageText.Text = message;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
