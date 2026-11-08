using System.Collections.Generic;
using XMLApp.Models;

namespace XMLApp.Strategies;

/// <summary>
/// Інтерфейс патерна Strategy для алгоритмів парсингу та пошуку в XML-документах.
/// </summary>
public interface IXmlSearchStrategy
{
    /// <summary>
    /// Виконує пошук публікацій за заданими критеріями у вказаному XML-файлі.
    /// </summary>
    /// <param name="xmlPath">Шлях до XML-файлу</param>
    /// <param name="criteria">Критерії фільтрації</param>
    /// <returns>Список знайдених публікацій</returns>
    List<Publication> Search(string xmlPath, SearchCriteria criteria);
}
