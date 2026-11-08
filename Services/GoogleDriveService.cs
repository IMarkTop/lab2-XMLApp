using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;

namespace XMLApp.Services;

/// <summary>
/// Сервіс для роботи з Google Drive API v3.
/// Підтримує завантаження XML/XSLT файлів з хмари та експорт результатів.
/// </summary>
public class GoogleDriveService : IGoogleDriveService
{
    private DriveService? _driveService;
    private readonly string _credentialsPath;
    private readonly string _appName = "LibraryApp";

    /// <summary>
    /// Створює екземпляр GoogleDriveService.
    /// За замовчуванням шукає credentials.json у каталозі застосунку.
    /// </summary>
    /// <param name="credentialsPath">Необов'язковий шлях до файлу облікових записів (credentials.json)</param>
    /// <param name="driveService">Необов'язковий вже налаштований екземпляр DriveService</param>
    public GoogleDriveService(string? credentialsPath = null, DriveService? driveService = null)
    {
        _credentialsPath = credentialsPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");
        _driveService = driveService;
    }

    /// <summary>
    /// Перевіряє або ліниво ініціалізує клієнт Google Drive API.
    /// </summary>
    private async Task<DriveService> GetDriveServiceAsync()
    {
        if (_driveService != null)
        {
            return _driveService;
        }

        if (!File.Exists(_credentialsPath))
        {
            throw new FileNotFoundException(
                $"Файл авторизації Google Drive не знайдено за шляхом: {_credentialsPath}. " +
                "Для підключення до Google Drive помістіть Service Account або OAuth2 credentials.json у каталог програми.",
                _credentialsPath);
        }

        GoogleCredential credential;
        using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.Scope.Drive);
        }

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _appName
        });

        return _driveService;
    }

    /// <inheritdoc />
    public async Task DownloadFileAsync(string fileId, string destLocalPath)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException("Ідентифікатор файлу не може бути порожнім.", nameof(fileId));
        }

        var service = await GetDriveServiceAsync();
        var request = service.Files.Get(fileId);

        var dir = Path.GetDirectoryName(destLocalPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var fileStream = new FileStream(destLocalPath, FileMode.Create, FileAccess.Write);
        var progress = await request.DownloadAsync(fileStream);

        if (progress.Status == DownloadStatus.Failed)
        {
            throw progress.Exception ?? new Exception("Помилка завантаження файлу з Google Drive.");
        }
    }

    /// <inheritdoc />
    public async Task<string> UploadStreamAsync(string fileName, Stream stream, string mimeType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Назва файлу не може бути порожньою.", nameof(fileName));
        }

        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        var service = await GetDriveServiceAsync();

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName
        };

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var request = service.Files.Create(fileMetadata, stream, mimeType);
        request.Fields = "id, name, webViewLink";

        var uploadProgress = await request.UploadAsync();

        if (uploadProgress.Status == UploadStatus.Failed)
        {
            throw uploadProgress.Exception ?? new Exception("Помилка відвантаження файлу на Google Drive.");
        }

        return request.ResponseBody?.Id ?? Guid.NewGuid().ToString();
    }
}
