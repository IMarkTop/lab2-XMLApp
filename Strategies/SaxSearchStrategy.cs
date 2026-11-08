using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XMLApp.Models;

namespace XMLApp.Strategies;

/// <summary>
/// Реалізація стратегії парсингу на базі SAX API.
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

        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true
        };

        using var reader = XmlReader.Create(xmlPath, settings);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "Publication")
            {
                var pub = new Publication
                {
                    Id = reader.GetAttribute("id") ?? string.Empty,
                    Author = reader.GetAttribute("author") ?? string.Empty,
                    Category = reader.GetAttribute("category") ?? string.Empty,
                    Faculty = reader.GetAttribute("faculty") ?? string.Empty,
                    Department = reader.GetAttribute("department") ?? string.Empty,
                    Year = reader.GetAttribute("year") ?? string.Empty
                };

                // Зчитуємо дочірні елементи вузла <Publication>
                using (var subtree = reader.ReadSubtree())
                {
                    while (subtree.Read())
                    {
                        if (subtree.NodeType == XmlNodeType.Element)
                        {
                            if (subtree.Name == "Title")
                            {
                                pub.Title = subtree.ReadElementContentAsString().Trim();
                            }
                            else if (subtree.Name == "Annotation")
                            {
                                pub.Annotation = subtree.ReadElementContentAsString().Trim();
                            }
                        }
                    }
                }

                if (Matches(pub, criteria))
                {
                    results.Add(pub);
                }
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
