namespace XMLApp.Models;

/// <summary>
/// Представляє публікацію в кафедральній бібліотеці (книга, монографія, посібник тощо).
/// </summary>
public class Publication
{
    public string Id { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Annotation { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"[{Id}] {Author} - \"{Title}\" ({Category}, {Faculty}, {Year})";
    }
}
