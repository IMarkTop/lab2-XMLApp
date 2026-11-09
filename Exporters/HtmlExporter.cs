using XMLApp.Services;

namespace XMLApp.Exporters;

/// <summary>
/// Експортер у формат HTML.
/// Реалізує фабричний метод CreateSaver(), повертаючи HtmlFileSaver.
/// </summary>
public class HtmlExporter : BaseExporter
{
    protected override string MimeType => "text/html";

    public HtmlExporter(IGoogleDriveService driveService) : base(driveService)
    {
    }

    /// <summary>
    /// Фабричний метод для створення зберігача формату HTML.
    /// </summary>
    public override IFileSaver CreateSaver()
    {
        return new HtmlFileSaver();
    }
}
