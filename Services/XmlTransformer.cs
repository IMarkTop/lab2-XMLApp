using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace XMLApp.Services;

/// <summary>
/// Сервіс для виконання XSLT-трансформацій XML-документів у формат HTML.
/// Використовує скомпільований оптимізований XSLT-процесор XslCompiledTransform.
/// </summary>
public class XmlTransformer
{
    /// <summary>
    /// Перетворює XML-документ за вказаним XSLT-шаблоном та записує результат у вихідний HTML-файл.
    /// </summary>
    /// <param name="xmlPath">Шлях до вхідного XML-файлу</param>
    /// <param name="xslPath">Шлях до файлу XSLT-шаблону</param>
    /// <param name="outputPath">Шлях до результуючого HTML-файлу</param>
    public void TransformToHtml(string xmlPath, string xslPath, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            throw new FileNotFoundException("Вхідний XML-файл не знайдено.", xmlPath);
        }

        if (string.IsNullOrWhiteSpace(xslPath) || !File.Exists(xslPath))
        {
            throw new FileNotFoundException("Файл XSLT-шаблону не знайдено.", xslPath);
        }

        var xsltSettings = new XsltSettings
        {
            EnableDocumentFunction = true
        };

        var transform = new XslCompiledTransform();
        transform.Load(xslPath, xsltSettings, new XmlUrlResolver());

        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        using var xmlReader = XmlReader.Create(xmlPath);
        using var writer = XmlWriter.Create(outputPath, transform.OutputSettings);
        transform.Transform(xmlReader, writer);
    }

    /// <summary>
    /// Перетворює XML-документ за вказаним XSLT-шаблоном та повертає HTML у вигляді рядка.
    /// </summary>
    /// <param name="xmlPath">Шлях до вхідного XML-файлу</param>
    /// <param name="xslPath">Шлях до файлу XSLT-шаблону</param>
    /// <returns>HTML-розмітка у вигляді рядка</returns>
    public string TransformToHtmlString(string xmlPath, string xslPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            throw new FileNotFoundException("Вхідний XML-файл не знайдено.", xmlPath);
        }

        if (string.IsNullOrWhiteSpace(xslPath) || !File.Exists(xslPath))
        {
            throw new FileNotFoundException("Файл XSLT-шаблону не знайдено.", xslPath);
        }

        var xsltSettings = new XsltSettings
        {
            EnableDocumentFunction = true
        };

        var transform = new XslCompiledTransform();
        transform.Load(xslPath, xsltSettings, new XmlUrlResolver());

        using var stringWriter = new StringWriter();
        using var xmlReader = XmlReader.Create(xmlPath);
        using var writer = XmlWriter.Create(stringWriter, transform.OutputSettings);
        
        transform.Transform(xmlReader, writer);
        return stringWriter.ToString();
    }
}
