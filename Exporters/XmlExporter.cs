using XMLApp.Services;

namespace XMLApp.Exporters;

/// <summary>
/// Експортер у формат XML.
/// Реалізує фабричний метод CreateSaver(), повертаючи XmlFileSaver.
/// </summary>
public class XmlExporter : BaseExporter
{
    protected override string MimeType => "application/xml";

    public XmlExporter(IGoogleDriveService driveService) : base(driveService)
    {
    }

    /// <summary>
    /// Фабричний метод для створення зберігача формату XML.
    /// </summary>
    public override IFileSaver CreateSaver()
    {
        return new XmlFileSaver();
    }
}
