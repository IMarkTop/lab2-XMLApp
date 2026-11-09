using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XMLApp.Models;
using XMLApp.Services;

namespace XMLApp.Exporters;

/// <summary>
/// Базовий клас творця у патерні Factory Method.
/// </summary>
public abstract class BaseExporter
{
    protected readonly IGoogleDriveService _driveService;

    /// <summary>
    /// MIME-тип контенту (наприклад, text/xml або text/html) для коректного збереження у хмарі.
    /// </summary>
    protected abstract string MimeType { get; }

    public BaseExporter(IGoogleDriveService driveService)
    {
        _driveService = driveService ?? throw new ArgumentNullException(nameof(driveService));
    }

    /// <summary>
    /// Фабричний метод: створює об'єкт збереження даних конкретного формату (XML, HTML).
    /// </summary>
    public abstract IFileSaver CreateSaver();

    /// <summary>
    /// Зберігає список публікацій у локальний файл за вказаним шляхом.
    /// </summary>
    /// <param name="filePath">Шлях до вихідного файлу</param>
    /// <param name="data">Дані для збереження</param>
    public void SaveToLocalFile(string filePath, List<Publication> data)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Шлях до файлу не може бути порожнім.", nameof(filePath));
        }

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        
        // Виклик фабричного методу
        var saver = CreateSaver();
        saver.Save(fileStream, data);
    }

    /// <summary>
    /// Зберігає список публікацій у хмарне сховище Google Drive.
    /// </summary>
    /// <param name="fileName">Назва файлу в хмарі</param>
    /// <param name="data">Дані для збереження</param>
    /// <returns>Ідентифікатор створеного файлу на Google Drive</returns>
    public async Task<string> SaveToGoogleDriveAsync(string fileName, List<Publication> data)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Назва файлу не може бути порожньою.", nameof(fileName));
        }

        // Підготовка відкритого потоку в пам'яті
        using var memoryStream = new MemoryStream();
        
        // Створення зберігача через фабричний метод та запис даних
        var saver = CreateSaver();
        saver.Save(memoryStream, data);

        // Перемотування потоку на початок для зчитування при передачі у Google Drive API
        memoryStream.Position = 0;

        return await _driveService.UploadStreamAsync(fileName, memoryStream, MimeType);
    }
}
