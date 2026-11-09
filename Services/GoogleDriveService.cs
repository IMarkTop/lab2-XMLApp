using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using XMLApp.Models;

namespace XMLApp.Services;

/// <summary>
/// Сервіс для роботи з Google Drive API v3.
/// </summary>
public class GoogleDriveService : IGoogleDriveService
{
    private DriveService? _driveService;
    private readonly string _credentialsPath;
    private readonly string _appName = "LibraryApp";

    public GoogleDriveService(string? credentialsPath = null, DriveService? driveService = null)
    {
        // Шукаємо credentials.json спочатку у BaseDirectory, потім у поточному каталозі
        if (!string.IsNullOrEmpty(credentialsPath))
        {
            _credentialsPath = credentialsPath;
        }
        else
        {
            var baseDirCredentials = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");
            var currentDirCredentials = Path.Combine(Directory.GetCurrentDirectory(), "credentials.json");

            _credentialsPath = File.Exists(baseDirCredentials)
                ? baseDirCredentials
                : (File.Exists(currentDirCredentials) ? currentDirCredentials : baseDirCredentials);
        }

        _driveService = driveService;
    }

    /// <summary>
    /// Лінива ініціалізація клієнта Google Drive API.
    /// Розпізнає тип credentials (OAuth 2.0 installed/web або Service Account).
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
                $"Файл авторизації Google Drive не знайдено за шляхом: {_credentialsPath}.\n\n" +
                "Для роботи з Google Drive завантажте credentials.json з Google Cloud Console (OAuth 2.0 Client ID або Service Account) " +
                "та покладіть його у каталог програми.",
                _credentialsPath);
        }

        string jsonContent = await File.ReadAllTextAsync(_credentialsPath);
        IConfigurableHttpClientInitializer initializer;

        // Перевіряємо, чи це OAuth 2.0 Client ID (містить секцію "installed" або "web")
        if (jsonContent.Contains("\"installed\"") || jsonContent.Contains("\"web\""))
        {
            using var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read);
            var secrets = (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets;
            var tokenDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".gdrive_tokens");

            // Запуск стандартного OAuth 2.0 авторизатора (відкриває браузер для надання дозволу)
            initializer = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                new[] { DriveService.Scope.Drive },
                "user",
                CancellationToken.None,
                new FileDataStore(tokenDir, true));
        }
        else
        {
            // Якщо це Service Account
            using var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read);
            initializer = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.Scope.Drive);
        }

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = initializer,
            ApplicationName = _appName
        });

        return _driveService;
    }
    
    public async Task<List<GoogleDriveFileInfo>> ListFilesAsync()
    {
        var service = await GetDriveServiceAsync();
        var listRequest = service.Files.List();
        listRequest.PageSize = 50;
        listRequest.Fields = "files(id, name, size, modifiedTime, mimeType)";
        listRequest.Q = "trashed = false and (mimeType = 'text/xml' or mimeType = 'application/xml' or name contains '.xml')";

        var response = await listRequest.ExecuteAsync();
        var result = new List<GoogleDriveFileInfo>();

        if (response.Files != null)
        {
            foreach (var file in response.Files)
            {
                result.Add(new GoogleDriveFileInfo
                {
                    Id = file.Id ?? string.Empty,
                    Name = file.Name ?? "Без назви",
                    Size = file.Size,
                    ModifiedTime = file.ModifiedTimeDateTimeOffset?.DateTime
                });
            }
        }

        return result;
    }
    
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
