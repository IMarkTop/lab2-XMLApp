using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XMLApp.Exporters;
using XMLApp.Logging;
using XMLApp.Models;
using XMLApp.Services;
using XMLApp.Strategies;

namespace XMLApp.ViewModels;

/// <summary>
/// Головна ViewModel програми (архітектурний патерн MVVM).
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private IXmlSearchStrategy _searchStrategy;
    private readonly SaxSearchStrategy _saxStrategy = new();
    private readonly DomSearchStrategy _domStrategy = new();
    private readonly LinqToXmlSearchStrategy _linqStrategy = new();

    private readonly XmlTransformer _xmlTransformer;
    private readonly IGoogleDriveService _driveService;
    private readonly XmlExporter _xmlExporter;
    private readonly HtmlExporter _htmlExporter;

    // Делегати для взаємодії з UI
    public Func<string, string, Task<string?>>? RequestOpenFileDialog { get; set; }
    public Func<string, string, Task<string?>>? RequestSaveFileDialog { get; set; }
    public Func<string, string, Task>? RequestMessageBox { get; set; }
    public Func<Task<GoogleDriveFileInfo?>>? RequestBrowseDriveDialog { get; set; }

    #region Властивості стану (Observable Properties)

    [ObservableProperty]
    private string _xmlPath = string.Empty;

    [ObservableProperty]
    private string _xslPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Готово до роботи";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _selectedStrategyIndex = 0; // 0: SAX, 1: DOM, 2: LINQ

    [ObservableProperty]
    private int _selectedExportFormat = 0; // 0: XML, 1: HTML

    [ObservableProperty]
    private int _selectedExportDestination = 0; // 0: Локальний диск, 1: Google Drive

    [ObservableProperty]
    private string _driveFileId = string.Empty;

    // Властивості фільтрації
    [ObservableProperty]
    private string? _selectedAuthor;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private string? _selectedFaculty;

    [ObservableProperty]
    private string? _selectedDepartment;

    [ObservableProperty]
    private string? _keyword;

    public SearchCriteria Criteria { get; } = new();

    // Динамічні списки для випадаючих списків (ComboBox)
    public ObservableCollection<string> AvailableAuthors { get; } = new();
    public ObservableCollection<string> AvailableCategories { get; } = new();
    public ObservableCollection<string> AvailableFaculties { get; } = new();
    public ObservableCollection<string> AvailableDepartments { get; } = new();

    // Результати пошуку
    public ObservableCollection<Publication> SearchResults { get; } = new();

    #endregion

    public MainViewModel()
    {
        _searchStrategy = _saxStrategy;
        _xmlTransformer = new XmlTransformer();
        _driveService = new GoogleDriveService();
        _xmlExporter = new XmlExporter(_driveService);
        _htmlExporter = new HtmlExporter(_driveService);

        // Спроба знайти дефолтні тестові файли у каталозі Data/
        InitializeDefaultPaths();
    }
    
    public MainViewModel(
        IXmlSearchStrategy? initialStrategy,
        XmlTransformer? xmlTransformer,
        IGoogleDriveService? driveService)
    {
        _searchStrategy = initialStrategy ?? _saxStrategy;
        _xmlTransformer = xmlTransformer ?? new XmlTransformer();
        _driveService = driveService ?? new GoogleDriveService();
        _xmlExporter = new XmlExporter(_driveService);
        _htmlExporter = new HtmlExporter(_driveService);

        InitializeDefaultPaths();
    }

    private void InitializeDefaultPaths()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var projectDataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");

        var candidateXml = Path.Combine(projectDataDir, "library.xml");
        var candidateXsl = Path.Combine(projectDataDir, "library.xsl");

        if (File.Exists(candidateXml))
        {
            XmlPath = candidateXml;
            PopulateMetadata(candidateXml);
        }

        if (File.Exists(candidateXsl))
        {
            XslPath = candidateXsl;
        }
    }

    partial void OnSelectedStrategyIndexChanged(int value)
    {
        _searchStrategy = value switch
        {
            1 => _domStrategy,
            2 => _linqStrategy,
            _ => _saxStrategy
        };
        StatusMessage = $"Обрано стратегію: {GetStrategyName()}";
    }

    private string GetStrategyName() => SelectedStrategyIndex switch
    {
        0 => "SAX (XmlReader)",
        1 => "DOM (XmlDocument)",
        2 => "LINQ to XML (XDocument)",
        _ => "SAX (XmlReader)"
    };

    #region Синхронізація вибраних фільтрів із SearchCriteria

    partial void OnSelectedAuthorChanged(string? value) => Criteria.Author = value;
    partial void OnSelectedCategoryChanged(string? value) => Criteria.Category = value;
    partial void OnSelectedFacultyChanged(string? value) => Criteria.Faculty = value;
    partial void OnSelectedDepartmentChanged(string? value) => Criteria.Department = value;
    partial void OnKeywordChanged(string? value) => Criteria.Keyword = value;

    #endregion

    #region Команди (MVVM RelayCommands)

    /// <summary>
    /// Завантаження XML-файлу з локального диску з динамічним наповненням ComboBox.
    /// </summary>
    [RelayCommand]
    public async Task LoadLocalAsync()
    {
        string? selectedFile = null;
        if (RequestOpenFileDialog != null)
        {
            selectedFile = await RequestOpenFileDialog("Оберіть файл бібліотеки XML", "XML файли (*.xml)|*.xml");
        }

        if (!string.IsNullOrEmpty(selectedFile))
        {
            XmlPath = selectedFile;
            PopulateMetadata(selectedFile);
            Search();
        }
    }

    /// <summary>
    /// Завантаження XML-файлу з Google Drive через вікно огляду або за ID.
    /// </summary>
    [RelayCommand]
    public async Task LoadFromDriveAsync()
    {
        string? targetFileId = DriveFileId?.Trim();
        string fileName = "catalog.xml";

        if (RequestBrowseDriveDialog != null)
        {
            var chosen = await RequestBrowseDriveDialog();
            if (chosen == null)
            {
                return; // Користувач скасував вибір
            }

            targetFileId = chosen.Id;
            fileName = chosen.Name;
            DriveFileId = chosen.Id;
        }

        if (string.IsNullOrWhiteSpace(targetFileId))
        {
            if (RequestMessageBox != null)
                await RequestMessageBox("Google Drive", "Будь ласка, оберіть файл або вкажіть його ID.");
            return;
        }

        IsBusy = true;
        StatusMessage = $"Завантаження '{fileName}' з Google Drive...";
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"gdrive_{fileName}");
            await _driveService.DownloadFileAsync(targetFileId, tempPath);

            XmlPath = tempPath;
            PopulateMetadata(tempPath);
            Search();

            StatusMessage = $"Файл '{fileName}' успішно завантажено з Google Drive.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка завантаження з хмари: {ex.Message}";
            if (RequestMessageBox != null)
                await RequestMessageBox("Помилка Google Drive", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Динамічний аналіз структури документа та наповнення ComboBox наявними атрибутами.
    /// </summary>
    public void PopulateMetadata(string path)
    {
        if (!File.Exists(path)) return;

        var metadata = XmlMetadataExtractor.ExtractMetadata(path);

        AvailableAuthors.Clear();
        foreach (var a in metadata.Authors) AvailableAuthors.Add(a);

        AvailableCategories.Clear();
        foreach (var c in metadata.Categories) AvailableCategories.Add(c);

        AvailableFaculties.Clear();
        foreach (var f in metadata.Faculties) AvailableFaculties.Add(f);

        AvailableDepartments.Clear();
        foreach (var d in metadata.Departments) AvailableDepartments.Add(d);

        StatusMessage = $"Завантажено каталог: {Path.GetFileName(path)} (Авторів: {AvailableAuthors.Count}, Категорій: {AvailableCategories.Count})";
    }

    /// <summary>
    /// Виконання пошуку згідно з обраною стратегією та фіксація у логері події Filtering.
    /// </summary>
    [RelayCommand]
    public void Search()
    {
        if (string.IsNullOrWhiteSpace(XmlPath) || !File.Exists(XmlPath))
        {
            StatusMessage = "XML-файл не вказано або він не існує.";
            return;
        }

        IsBusy = true;
        try
        {
            var results = _searchStrategy.Search(XmlPath, Criteria);

            SearchResults.Clear();
            foreach (var item in results)
            {
                SearchResults.Add(item);
            }

            // Журналювання події рівня Filtering
            var logMessage = $"Стратегія: {GetStrategyName()}. " +
                             $"Параметри: «Author» – {Criteria.Author ?? "—"}, «Category» – {Criteria.Category ?? "—"}, " +
                             $"«Faculty» – {Criteria.Faculty ?? "—"}, «Department» – {Criteria.Department ?? "—"}, " +
                             $"«Keyword» – {Criteria.Keyword ?? "—"}. Знайдено: {results.Count}";

            AppLogger.Instance.Log(EventLevel.Filtering, logMessage);

            StatusMessage = $"Пошук завершено ({GetStrategyName()}). Знайдено: {results.Count} записів.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка пошуку: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Очищення фільтрів та результатів пошуку (кнопка Clear).
    /// </summary>
    [RelayCommand]
    public void Clear()
    {
        SelectedAuthor = null;
        SelectedCategory = null;
        SelectedFaculty = null;
        SelectedDepartment = null;
        Keyword = null;

        Criteria.Clear();
        SearchResults.Clear();

        StatusMessage = "Фільтри та результати очищено.";
    }

    /// <summary>
    /// XSLT-трансформація XML у стилізований HTML з фіксацією події Transformation у логері.
    /// </summary>
    [RelayCommand]
    public async Task TransformHtmlAsync()
    {
        if (string.IsNullOrWhiteSpace(XmlPath) || !File.Exists(XmlPath))
        {
            StatusMessage = "Помилка: XML-файл не знайдено.";
            return;
        }

        if (string.IsNullOrWhiteSpace(XslPath) || !File.Exists(XslPath))
        {
            if (RequestOpenFileDialog != null)
            {
                XslPath = await RequestOpenFileDialog("Оберіть файл стилів XSLT", "XSL файли (*.xsl;*.xslt)|*.xsl;*.xslt") ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(XslPath) || !File.Exists(XslPath))
            {
                StatusMessage = "Помилка: Файл XSL-стилів не знайдено.";
                return;
            }
        }

        string? outPath = null;
        if (RequestSaveFileDialog != null)
        {
            outPath = await RequestSaveFileDialog("Зберегти згенерований HTML-звіт", "HTML файли (*.html)|*.html");
        }

        if (string.IsNullOrEmpty(outPath))
        {
            outPath = Path.Combine(Path.GetDirectoryName(XmlPath) ?? AppDomain.CurrentDomain.BaseDirectory, "catalog_transformed.html");
        }

        IsBusy = true;
        StatusMessage = "Виконується XSLT-трансформація...";
        try
        {
            _xmlTransformer.TransformToHtml(XmlPath, XslPath, outPath);

            // Журналювання події рівня Transformation
            AppLogger.Instance.Log(EventLevel.Transformation, $"Шаблон: {Path.GetFileName(XslPath)}, Результуючий файл: {outPath}");

            StatusMessage = $"XSLT-трансформацію успішно завершено: {Path.GetFileName(outPath)}";
            if (RequestMessageBox != null)
                await RequestMessageBox("XSLT Трансформація", $"Звіт успішно створено:\n{outPath}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка XSLT-трансформації: {ex.Message}";
            if (RequestMessageBox != null)
                await RequestMessageBox("Помилка XSLT", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Збереження відфільтрованих даних на локальний диск.
    /// </summary>
    [RelayCommand]
    public async Task SaveLocalAsync()
    {
        if (SearchResults.Count == 0)
        {
            if (RequestMessageBox != null)
                await RequestMessageBox("Збереження", "Список результатів порожній. Спочатку виконайте пошук.");
            return;
        }

        bool isXml = SelectedExportFormat == 0;
        var filter = isXml ? "XML файли (*.xml)|*.xml" : "HTML файли (*.html)|*.html";
        var defaultExt = isXml ? "filtered_results.xml" : "filtered_results.html";

        string? outPath = null;
        if (RequestSaveFileDialog != null)
        {
            outPath = await RequestSaveFileDialog("Зберегти відфільтровані результати", filter);
        }

        if (string.IsNullOrEmpty(outPath))
        {
            outPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultExt);
        }

        IsBusy = true;
        try
        {
            BaseExporter exporter = isXml ? _xmlExporter : _htmlExporter;
            exporter.SaveToLocalFile(outPath, SearchResults.ToList());

            // Журналювання події рівня Saving
            AppLogger.Instance.Log(EventLevel.Saving, $"Формат: {(isXml ? "XML" : "HTML")}, Місце: Локальний диск, Файл: {outPath}");

            StatusMessage = $"Результати збережено на локальний диск: {Path.GetFileName(outPath)}";
            if (RequestMessageBox != null)
                await RequestMessageBox("Збереження успішне", $"Файл збережено:\n{outPath}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка збереження: {ex.Message}";
            if (RequestMessageBox != null)
                await RequestMessageBox("Помилка збереження", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Збереження відфільтрованих даних у хмарне сховище Google Drive.
    /// </summary>
    [RelayCommand]
    public async Task SaveToDriveAsync()
    {
        if (SearchResults.Count == 0)
        {
            if (RequestMessageBox != null)
                await RequestMessageBox("Google Drive", "Список результатів порожній. Спочатку виконайте пошук.");
            return;
        }

        bool isXml = SelectedExportFormat == 0;
        var fileName = isXml ? $"library_export_{DateTime.Now:yyyyMMddHHmmss}.xml" : $"library_export_{DateTime.Now:yyyyMMddHHmmss}.html";

        IsBusy = true;
        StatusMessage = "Збереження на Google Drive...";
        try
        {
            BaseExporter exporter = isXml ? _xmlExporter : _htmlExporter;
            var fileId = await exporter.SaveToGoogleDriveAsync(fileName, SearchResults.ToList());

            // Журналювання події рівня Saving
            AppLogger.Instance.Log(EventLevel.Saving, $"Формат: {(isXml ? "XML" : "HTML")}, Місце: Google Drive, ID: {fileId}");

            StatusMessage = $"Успішно експортовано на Google Drive (ID: {fileId})";
            if (RequestMessageBox != null)
                await RequestMessageBox("Google Drive", $"Файл успішно завантажено на Google Drive!\nID: {fileId}\nНазва: {fileName}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка Google Drive: {ex.Message}";
            if (RequestMessageBox != null)
                await RequestMessageBox("Помилка Google Drive", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion
}
