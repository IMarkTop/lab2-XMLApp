using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XMLApp.Models;

namespace XMLApp.Strategies;

/// <summary>
/// Реалізація стратегії парсингу на базі SAX API (System.Xml.XmlReader).
/// Здійснює швидкий потоковий низькорівневий розбір без завантаження всього DOM-дерева в пам'ять.
/// </summary>
public class SaxSearchStrategy : IXmlSearchStrategy
{
    public List<Publication> Search(string xmlPath, SearchCriteria criteria)
    {
        var results = new List<Publication>();

        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            return results;
        }

        using var reader = XmlReader.Create(xmlPath);
        Publication? currentPub = null;
        string? currentTag = null;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Publication")
                {
                    currentPub = new Publication
                    {
                        Id = reader.GetAttribute("id") ?? string.Empty,
                        Author = reader.GetAttribute("author") ?? string.Empty,
                        Category = reader.GetAttribute("category") ?? string.Empty,
                        Faculty = reader.GetAttribute("faculty") ?? string.Empty,
                        Department = reader.GetAttribute("department") ?? string.Empty,
                        Year = reader.GetAttribute("year") ?? string.Empty
                    };
                }
                else if (currentPub != null)
                {
                    currentTag = reader.Name;
                }
            }
            else if (reader.NodeType == XmlNodeType.Text && currentPub != null)
            {
                if (currentTag == "Title")
                {
                    currentPub.Title = reader.Value.Trim();
                }
                else if (currentTag == "Annotation")
                {
                    currentPub.Annotation = reader.Value.Trim();
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                if (reader.Name == "Publication" && currentPub != null)
                {
                    if (Matches(currentPub, criteria))
                    {
                        results.Add(currentPub);
                    }
                    currentPub = null;
                }
                currentTag = null;
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
