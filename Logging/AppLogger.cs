using System;
using System.IO;
using System.Text;

namespace XMLApp.Logging;

/// <summary>
/// Потокобезпечний логер аудиту на основі патерна Singleton.
/// Фіксує операції фільтрації, трансформації та збереження у файл audit.log.
/// </summary>
public sealed class AppLogger
{
    private static readonly Lazy<AppLogger> _instance = new(() => new AppLogger());
    private readonly object _lock = new();
    private string _logFilePath;

    /// <summary>
    /// Єдиний екземпляр класу AppLogger.
    /// </summary>
    public static AppLogger Instance => _instance.Value;

    /// <summary>
    /// Шлях до файлу журналу аудиту.
    /// </summary>
    public string LogFilePath
    {
        get => _logFilePath;
        set
        {
            lock (_lock)
            {
                _logFilePath = value;
            }
        }
    }

    /// <summary>
    /// Закритий конструктор для заборони створення об'єктів ззовні.
    /// </summary>
    private AppLogger()
    {
        _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audit.log");
    }

    /// <summary>
    /// Записує подію до журналу згідно з визначеним форматом:
    /// "дд.ММ.рррр гг:хх:сс <Рівень>. <Повідомлення>"
    /// </summary>
    /// <param name="level">Рівень події (Filtering, Transformation, Saving)</param>
    /// <param name="message">Опис події та параметри</param>
    public void Log(EventLevel level, string message)
    {
        var timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        var levelTitle = GetLevelTitle(level);
        var entry = $"{timestamp} {levelTitle}. {message}";

        lock (_lock)
        {
            try
            {
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(_logFilePath, entry + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Запобігаємо падінню застосунку у разі тимчасової помилки запису у файл
                System.Diagnostics.Debug.WriteLine($"[AppLogger ERROR]: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Читає весь вміст файлу аудиту.
    /// </summary>
    public string ReadAllLogs()
    {
        lock (_lock)
        {
            if (!File.Exists(_logFilePath))
            {
                return string.Empty;
            }

            return File.ReadAllText(_logFilePath, Encoding.UTF8);
        }
    }

    /// <summary>
    /// Очищує файл аудиту.
    /// </summary>
    public void ClearLog()
    {
        lock (_lock)
        {
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }
    }

    private static string GetLevelTitle(EventLevel level) => level switch
    {
        EventLevel.Filtering => "Фільтрація",
        EventLevel.Transformation => "Трансформація",
        EventLevel.Saving => "Збереження",
        _ => level.ToString()
    };
}
