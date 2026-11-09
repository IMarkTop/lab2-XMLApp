using System;

namespace XMLApp.Models;

/// <summary>
/// Інформація про файл на Google Drive для діалогу перегляду.
/// </summary>
public class GoogleDriveFileInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? Size { get; set; }
    public DateTime? ModifiedTime { get; set; }

    public string SizeFormatted
    {
        get
        {
            if (!Size.HasValue) return "—";
            if (Size.Value < 1024) return $"{Size.Value} B";
            if (Size.Value < 1024 * 1024) return $"{Size.Value / 1024.0:F1} KB";
            return $"{Size.Value / (1024.0 * 1024.0):F1} MB";
        }
    }

    public string ModifiedFormatted => ModifiedTime.HasValue
        ? ModifiedTime.Value.ToString("dd.MM.yyyy HH:mm")
        : "—";

    public override string ToString() => $"{Name} ({Id})";
}
