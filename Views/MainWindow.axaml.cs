using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using XMLApp.ViewModels;

namespace XMLApp.Views;

/// <summary>
/// Представлення головного вікна.
/// </summary>
public partial class MainWindow : Window
{
    private bool _canClose = false;

    public MainWindow()
    {
        InitializeComponent();
        Closing += MainWindow_Closing;
        DataContextChanged += MainWindow_DataContextChanged;
    }

    private void MainWindow_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // Реєстрація візуальних діалогів для ViewModel
            vm.RequestOpenFileDialog = OpenFilePickerAsync;
            vm.RequestSaveFileDialog = SaveFilePickerAsync;
            vm.RequestMessageBox = ShowMessageBoxAsync;
            vm.RequestBrowseDriveDialog = BrowseGoogleDriveAsync;
        }
    }

    private async Task<XMLApp.Models.GoogleDriveFileInfo?> BrowseGoogleDriveAsync()
    {
        var browser = new GoogleDriveBrowserWindow();
        return await browser.ShowDialog<XMLApp.Models.GoogleDriveFileInfo?>(this);
    }

    private async Task<string?> OpenFilePickerAsync(string title, string filter)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        if (files != null && files.Count > 0)
        {
            return files[0].Path.LocalPath;
        }

        return null;
    }

    private async Task<string?> SaveFilePickerAsync(string title, string filter)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title
        });

        return file?.Path.LocalPath;
    }

    private async Task ShowMessageBoxAsync(string title, string message)
    {
        var msgBox = new MessageBoxWindow(title, message);
        await msgBox.ShowDialog(this);
    }

    private async void ChooseXslButton_Click(object? sender, RoutedEventArgs e)
    {
        var path = await OpenFilePickerAsync("Оберіть файл стилів XSLT", "XSL файли (*.xsl)|*.xsl");
        if (!string.IsNullOrEmpty(path) && DataContext is MainViewModel vm)
        {
            vm.XslPath = path;
        }
    }

    /// <summary>
    /// Модальне підтвердження виходу з програми
    /// «Чи дійсно ви хочете завершити роботу з програмою?» з кнопками «Так» та «Ні».
    /// </summary>
    private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (_canClose)
        {
            return;
        }

        e.Cancel = true;

        var dialog = new ConfirmExitWindow();
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed)
        {
            _canClose = true;
            Close();
        }
    }
}
