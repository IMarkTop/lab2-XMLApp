using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using XMLApp.Models;

namespace XMLApp.Exporters;

/// <summary>
/// Серіалізатор відфільтрованих даних у формат HTML.
/// </summary>
public class HtmlFileSaver : IFileSaver
{
    public void Save(Stream targetStream, List<Publication> data)
    {
        // Не закриваємо базовий потік (leaveOpen: true), щоб творець міг продовжити роботу
        using var writer = new StreamWriter(targetStream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);

        writer.WriteLine("<!DOCTYPE html>");
        writer.WriteLine("<html lang=\"uk\">");
        writer.WriteLine("<head>");
        writer.WriteLine("  <meta charset=\"utf-8\" />");
        writer.WriteLine("  <title>Збережені результати пошуку</title>");
        writer.WriteLine("  <style>");
        writer.WriteLine("    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; padding: 25px; background: #f8fafc; color: #2d3748; }");
        writer.WriteLine("    .card { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.08); max-width: 1000px; margin: auto; }");
        writer.WriteLine("    h2 { color: #1e3a8a; border-bottom: 2px solid #e2e8f0; padding-bottom: 8px; }");
        writer.WriteLine("    table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
        writer.WriteLine("    th { background: #3b82f6; color: white; text-align: left; padding: 10px; font-size: 14px; }");
        writer.WriteLine("    td { padding: 10px; border-bottom: 1px solid #e2e8f0; font-size: 13.5px; vertical-align: top; }");
        writer.WriteLine("    tr:nth-child(even) { background: #f8fafc; }");
        writer.WriteLine("    .badge { display: inline-block; background: #e0e7ff; color: #3730a3; padding: 2px 8px; border-radius: 4px; font-size: 12px; }");
        writer.WriteLine("  </style>");
        writer.WriteLine("</head>");
        writer.WriteLine("<body>");
        writer.WriteLine("  <div class=\"card\">");
        writer.WriteLine($"    <h2>Збережені результати пошуку ({data.Count} записів)</h2>");
        writer.WriteLine($"    <p style=\"color: #64748b; font-size: 13px;\">Дата експорту: {System.DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>");
        writer.WriteLine("    <table>");
        writer.WriteLine("      <thead>");
        writer.WriteLine("        <tr>");
        writer.WriteLine("          <th>ID</th>");
        writer.WriteLine("          <th>Назва та анотація</th>");
        writer.WriteLine("          <th>Автор</th>");
        writer.WriteLine("          <th>Категорія</th>");
        writer.WriteLine("          <th>Факультет / Кафедра</th>");
        writer.WriteLine("          <th>Рік</th>");
        writer.WriteLine("        </tr>");
        writer.WriteLine("      </thead>");
        writer.WriteLine("      <tbody>");

        foreach (var pub in data)
        {
            writer.WriteLine("        <tr>");
            writer.WriteLine($"          <td><strong>{HttpUtility.HtmlEncode(pub.Id)}</strong></td>");
            writer.WriteLine($"          <td><strong>{HttpUtility.HtmlEncode(pub.Title)}</strong><br/><small style=\"color: #64748b;\">{HttpUtility.HtmlEncode(pub.Annotation)}</small></td>");
            writer.WriteLine($"          <td>{HttpUtility.HtmlEncode(pub.Author)}</td>");
            writer.WriteLine($"          <td><span class=\"badge\">{HttpUtility.HtmlEncode(pub.Category)}</span></td>");
            writer.WriteLine($"          <td>{HttpUtility.HtmlEncode(pub.Faculty)} {(string.IsNullOrEmpty(pub.Department) ? "" : $"({HttpUtility.HtmlEncode(pub.Department)})")}</td>");
            writer.WriteLine($"          <td>{HttpUtility.HtmlEncode(pub.Year)}</td>");
            writer.WriteLine("        </tr>");
        }

        writer.WriteLine("      </tbody>");
        writer.WriteLine("    </table>");
        writer.WriteLine("  </div>");
        writer.WriteLine("</body>");
        writer.WriteLine("</html>");
        writer.Flush();
    }
}
