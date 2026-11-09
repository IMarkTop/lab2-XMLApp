using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using XMLApp.Models;

namespace XMLApp.Exporters;

/// <summary>
/// Серіалізатор відфільтрованих даних у формат XML.
/// </summary>
public class XmlFileSaver : IFileSaver
{
    public void Save(Stream targetStream, List<Publication> data)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        using var writer = XmlWriter.Create(targetStream, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("Library");
        writer.WriteAttributeString("exported", System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
        writer.WriteAttributeString("count", data.Count.ToString());

        foreach (var pub in data)
        {
            writer.WriteStartElement("Publication");
            writer.WriteAttributeString("id", pub.Id);

            if (!string.IsNullOrEmpty(pub.Author))
            {
                writer.WriteAttributeString("author", pub.Author);
            }

            if (!string.IsNullOrEmpty(pub.Category))
            {
                writer.WriteAttributeString("category", pub.Category);
            }

            if (!string.IsNullOrEmpty(pub.Faculty))
            {
                writer.WriteAttributeString("faculty", pub.Faculty);
            }

            if (!string.IsNullOrEmpty(pub.Department))
            {
                writer.WriteAttributeString("department", pub.Department);
            }

            if (!string.IsNullOrEmpty(pub.Year))
            {
                writer.WriteAttributeString("year", pub.Year);
            }

            if (!string.IsNullOrEmpty(pub.Title))
            {
                writer.WriteElementString("Title", pub.Title);
            }

            if (!string.IsNullOrEmpty(pub.Annotation))
            {
                writer.WriteElementString("Annotation", pub.Annotation);
            }

            writer.WriteEndElement(); // </Publication>
        }

        writer.WriteEndElement(); // </Library>
        writer.WriteEndDocument();
        writer.Flush();
    }
}
