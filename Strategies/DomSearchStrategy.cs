using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XMLApp.Models;

namespace XMLApp.Strategies;

/// <summary>
/// Реалізація стратегії парсингу на базі DOM API.
/// Завантажує все дерево документа в пам'ять та навігується вузлами.
/// </summary>
public class DomSearchStrategy : IXmlSearchStrategy
{
    public List<Publication> Search(string xmlPath, SearchCriteria criteria)
    {
        var results = new List<Publication>();

        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            return results;
        }

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);

        var nodes = xmlDoc.SelectNodes("//Publication");
        if (nodes == null)
        {
            return results;
        }

        foreach (XmlNode node in nodes)
        {
            if (node.Attributes == null)
            {
                continue;
            }

            var pub = new Publication
            {
                Id = node.Attributes["id"]?.Value ?? string.Empty,
                Author = node.Attributes["author"]?.Value ?? string.Empty,
                Category = node.Attributes["category"]?.Value ?? string.Empty,
                Faculty = node.Attributes["faculty"]?.Value ?? string.Empty,
                Department = node.Attributes["department"]?.Value ?? string.Empty,
                Year = node.Attributes["year"]?.Value ?? string.Empty,
                Title = node.SelectSingleNode("Title")?.InnerText?.Trim() ?? string.Empty,
                Annotation = node.SelectSingleNode("Annotation")?.InnerText?.Trim() ?? string.Empty
            };

            if (Matches(pub, criteria))
            {
                results.Add(pub);
            }
        }

        return results;
    }

    private static bool Matches(Publication pub, SearchCriteria? criteria)
    {
        if (criteria == null || criteria.IsEmpty())
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Author) &&
            !string.Equals(pub.Author, criteria.Author, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Category) &&
            !string.Equals(pub.Category, criteria.Category, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Faculty) &&
            !string.Equals(pub.Faculty, criteria.Faculty, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Department) &&
            !string.Equals(pub.Department, criteria.Department, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var kw = criteria.Keyword.Trim();
            bool inTitle = pub.Title.Contains(kw, StringComparison.OrdinalIgnoreCase);
            bool inAnnotation = pub.Annotation.Contains(kw, StringComparison.OrdinalIgnoreCase);

            if (!inTitle && !inAnnotation)
            {
                return false;
            }
        }

        return true;
    }
}
