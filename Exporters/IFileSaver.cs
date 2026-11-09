using System.Collections.Generic;
using System.IO;
using XMLApp.Models;

namespace XMLApp.Exporters;

/// <summary>
/// Інтерфейс продукту в патерні Factory Method.
/// Відповідає за серіалізацію списку публікацій у відкритий потік даних.
/// </summary>
public interface IFileSaver
{
    /// <summary>
    /// Зберігає дані публікацій у цільовий потік.
    /// </summary>
    /// <param name="targetStream">Відкритий потік для запису (FileStream або MemoryStream)</param>
    /// <param name="data">Список публікацій для збереження</param>
    void Save(Stream targetStream, List<Publication> data);
}
