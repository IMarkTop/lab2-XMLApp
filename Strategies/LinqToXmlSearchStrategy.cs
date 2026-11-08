using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using XMLApp.Models;

namespace XMLApp.Strategies;

/// <summary>
/// Реалізація стратегії парсингу на базі LINQ to XML.
/// Використовує декларативні LINQ-запити для вибірки та фільтрації елементів.
/// </summary>
public class LinqToXmlSearchStrategy : IXmlSearchStrategy
{
    public List<Publication> Search(string xmlPath, SearchCriteria criteria)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            return new List<Publication>();
        }

        var xDoc = XDocument.Load(xmlPath);

        var query = xDoc.Descendants("Publication")
            .Select(p => new Publication
            {
                Id = (string?)p.Attribute("id") ?? string.Empty,
                Author = (string?)p.Attribute("author") ?? string.Empty,
                Category = (string?)p.Attribute("category") ?? string.Empty,
                Faculty = (string?)p.Attribute("faculty") ?? string.Empty,
                Department = (string?)p.Attribute("department") ?? string.Empty,
                Year = (string?)p.Attribute("year") ?? string.Empty,
                Title = ((string?)p.Element("Title"))?.Trim() ?? string.Empty,
                Annotation = ((string?)p.Element("Annotation"))?.Trim() ?? string.Empty
            });

        if (criteria != null && !criteria.IsEmpty())
        {
            if (!string.IsNullOrWhiteSpace(criteria.Author))
            {
                query = query.Where(p => string.Equals(p.Author, criteria.Author, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Category))
            {
                query = query.Where(p => string.Equals(p.Category, criteria.Category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Faculty))
            {
                query = query.Where(p => string.Equals(p.Faculty, criteria.Faculty, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Department))
            {
                query = query.Where(p => string.Equals(p.Department, criteria.Department, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var kw = criteria.Keyword.Trim();
                query = query.Where(p =>
                    p.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    p.Annotation.Contains(kw, StringComparison.OrdinalIgnoreCase));
            }
        }

        return query.ToList();
    }
}
