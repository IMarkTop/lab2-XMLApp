using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XMLApp.Strategies;

/// <summary>
/// Допоміжний клас для динамічного аналізу структури XML-документа
/// та вилучення унікальних значень атрибутів для заповнення списків фільтрів.
/// </summary>
public static class XmlMetadataExtractor
{
    public class MetadataResult
    {
        public List<string> Authors { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<string> Faculties { get; set; } = new();
        public List<string> Departments { get; set; } = new();
    }

    public static MetadataResult ExtractMetadata(string xmlPath)
    {
        var result = new MetadataResult();

        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            return result;
        }

        try
        {
            var doc = XDocument.Load(xmlPath);
            var publications = doc.Descendants("Publication").ToList();

            result.Authors = publications
                .Select(p => (string?)p.Attribute("author"))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            result.Categories = publications
                .Select(p => (string?)p.Attribute("category"))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            result.Faculties = publications
                .Select(p => (string?)p.Attribute("faculty"))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            result.Departments = publications
                .Select(p => (string?)p.Attribute("department"))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[XmlMetadataExtractor ERROR]: {ex.Message}");
        }

        return result;
    }
}
