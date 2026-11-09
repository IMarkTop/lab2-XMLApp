using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using XMLApp.Models;
using XMLApp.Services;

namespace XMLApp.Views;

/// <summary>
/// Вікно перегляду та вибору файлів на Google Drive.
/// </summary>
public partial class GoogleDriveBrowserWindow : Window
{
    private readonly IGoogleDriveService _driveService;
    private List<GoogleDriveFileInfo> _allFiles = new();

    public GoogleDriveFileInfo? SelectedFile { get; private set; }

    public GoogleDriveBrowserWindow()
    {
        InitializeComponent();
        _driveService = new GoogleDriveService();
        Loaded += GoogleDriveBrowserWindow_Loaded;
    }

    public GoogleDriveBrowserWindow(IGoogleDriveService driveService)
    {
        InitializeComponent();
        _driveService = driveService;
        Loaded += GoogleDriveBrowserWindow_Loaded;
    }

    private async void GoogleDriveBrowserWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        await LoadFilesAsync();
    }

    private async System.Threading.Tasks.Task LoadFilesAsync()
    {
        StatusText.Text = "Завантаження списку файлів з Google Drive...";
        try
        {
            _allFiles = await _driveService.ListFilesAsync();
            ApplyFilter();
            StatusText.Text = $"Знайдено файлів: {_allFiles.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Помилка: {ex.Message}";
            var msg = new MessageBoxWindow("Помилка Google Drive", ex.Message);
            await msg.ShowDialog(this);
        }
    }

    private void ApplyFilter()
    {
        var filter = SearchBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(filter))
        {
            FilesGrid.ItemsSource = _allFiles;
        }
        else
        {
            FilesGrid.ItemsSource = _allFiles
                .Where(f => f.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private void SearchBox_KeyUp(object? sender, KeyEventArgs e)
    {
        ApplyFilter();
    }

    private async void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        await LoadFilesAsync();
    }

    private void FilesGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        ConfirmSelection();
    }

    private void SelectButton_Click(object? sender, RoutedEventArgs e)
    {
        ConfirmSelection();
    }

    private void ConfirmSelection()
    {
        if (FilesGrid.SelectedItem is GoogleDriveFileInfo file)
        {
            SelectedFile = file;
            Close(file);
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}
