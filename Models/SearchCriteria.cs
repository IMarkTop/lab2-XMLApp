namespace XMLApp.Models;

/// <summary>
/// Критерії фільтрації та пошуку публікацій у каталозі бібліотеки.
/// </summary>
public class SearchCriteria
{
    public string? Author { get; set; }
    public string? Category { get; set; }
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public string? Keyword { get; set; }

    /// <summary>
    /// Очищення всіх критеріїв пошуку.
    /// </summary>
    public void Clear()
    {
        Author = null;
        Category = null;
        Faculty = null;
        Department = null;
        Keyword = null;
    }

    /// <summary>
    /// Перевіряє, чи не встановлено жодного фільтра.
    /// </summary>
    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Author) &&
               string.IsNullOrWhiteSpace(Category) &&
               string.IsNullOrWhiteSpace(Faculty) &&
               string.IsNullOrWhiteSpace(Department) &&
               string.IsNullOrWhiteSpace(Keyword);
    }
}
