using System.IO;
using System.Threading.Tasks;

namespace XMLApp.Services;

/// <summary>
/// Інтерфейс для взаємодії зі сховищем Google Drive API v3.
/// Забезпечує зчитування вхідних файлів та збереження результатів у хмару.
/// </summary>
public interface IGoogleDriveService
{
    /// <summary>
    /// Завантажує файл з Google Drive за його ідентифікатором у локальний файл.
    /// </summary>
    /// <param name="fileId">Ідентифікатор файлу в Google Drive</param>
    /// <param name="destLocalPath">Локальний шлях для збереження</param>
    Task DownloadFileAsync(string fileId, string destLocalPath);

    /// <summary>
    /// Вивантажує потік даних у Google Drive із зазначеним MIME-типом.
    /// </summary>
    /// <param name="fileName">Назва файлу у хмарі</param>
    /// <param name="stream">Потік даних (Stream)</param>
    /// <param name="mimeType">MIME-тип (наприклад, text/xml або text/html)</param>
    /// <returns>Ідентифікатор створеного файлу у хмарі</returns>
    Task<string> UploadStreamAsync(string fileName, Stream stream, string mimeType);
}
