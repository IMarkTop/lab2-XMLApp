# Кафедральна бібліотека

Кросплатформний десктопний застосунок для аналізу, багатопараметричного пошуку, XSLT-трансформації XML-каталогів публікацій кафедральної бібліотеки та синхронізації зі сховищами на локальному диску і Google Drive.

---

## 🛠 Технологічний стек

* **Платформа:** .NET 10 (C# 13)
* **Графічний фреймворк:** [Avalonia UI](https://avaloniaui.net/) (кросплатформна підтримка Linux, Windows, macOS)
* **MVVM-інструментарій:** `CommunityToolkit.Mvvm`
* **Хмарна інтеграція:** Google Drive API v3 (`Google.Apis.Drive.v3`)
* **XML-процесинг:** SAX (`System.Xml.XmlReader`), DOM (`System.Xml.XmlDocument`), LINQ to XML (`System.Xml.Linq.XDocument`), XSLT (`System.Xml.Xsl.XslCompiledTransform`)

---

## 🏛 Архітектура та патерни проєктування

### 1. Патерн «Стратегія» (Strategy Pattern)
Забезпечує три взаємозамінні алгоритми парсингу та пошуку в XML з однаковим інтерфейсом:
* **Інтерфейс:** `IXmlSearchStrategy` з методом `Search(string xmlPath, SearchCriteria criteria)`.
* **Реалізації:**
  1. `SaxSearchStrategy` — низькорівневий потоковий розбір через `XmlReader` без завантаження всього DOM-дерева в оперативну пам'ять (висока швидкодія на великих обсягах даних).
  2. `DomSearchStrategy` — розбір деревоподібної структури документа через `XmlDocument` та вузли DOM.
  3. `LinqToXmlSearchStrategy` — декларативний підхід засобами `XDocument` та LINQ-запитів (`Where`, `Select`).

### 2. Патерн «Фабричний метод» (Factory Method Pattern)
Організує збереження відфільтрованої вибірки у формати XML та HTML на відкритий потік (локальний `FileStream` або `MemoryStream` для передачі в Google Drive):
* **Абстрактний творець:** `BaseExporter` — інкапсулює код роботи з потоками та оголошує фабричний метод `CreateSaver()`.
* **Продукти (`IFileSaver`):**
  * `XmlFileSaver` — серіалізація списку публікацій у валідний XML-документ;
  * `HtmlFileSaver` — генерація адаптивної стилізованої HTML-таблиці.
* **Конкретні творці:**
  * `XmlExporter` (повертає `XmlFileSaver`);
  * `HtmlExporter` (повертає `HtmlFileSaver`).

### 3. Патерн «Одинак» (Singleton Pattern)
Централізована потокобезпечна підсистема аудиту (`AppLogger`):
* Закритий приватний конструктор та лінива потокобезпечна ініціалізація через `Lazy<AppLogger>`.
* Фіксація операцій у локальний файл `audit.log` у строго заданому форматі (`дд.ММ.рррр гг:хх:сс <Рівень>. <Опис>`).
* **3 рівні важливості подій (`EventLevel`):**
  * `Filtering` — параметри фільтрації, обрана стратегія та кількість знайдених записів;
  * `Transformation` — запуск XSLT-трансформації та шлях до вихідного HTML-файлу;
  * `Saving` — експорт вибірки (формат XML/HTML, локальний диск / Google Drive).

### 4. Архітектурний патерн MVVM (Model-View-ViewModel)
* **Model:** сутності `Publication`, `SearchCriteria`, `GoogleDriveFileInfo`.
* **ViewModel:** `MainViewModel` (CommunityToolkit.Mvvm) — властивості стану, списки `AvailableAuthors`, `AvailableCategories` тощо, команди `[RelayCommand]`.
* **View:** `MainWindow.axaml`, діалог підтвердження виходу `ConfirmExitWindow.axaml`, браузер хмари `GoogleDriveBrowserWindow.axaml`, сповіщення `MessageBoxWindow.axaml`.

---

## 📁 Структура проєкту

```text
├── Data/
│   ├── library.xml             # Вхідний каталог із різною кількістю атрибутів
│   └── library.xsl             # Таблиця стилів XSLT для перетворення в HTML
├── Models/
│   ├── Publication.cs          # Сутність публікації
│   ├── SearchCriteria.cs       # Параметри фільтрації
│   └── GoogleDriveFileInfo.cs  # Модель файлу Google Drive
├── Strategies/
│   ├── IXmlSearchStrategy.cs   # Інтерфейс патерна Strategy
│   ├── SaxSearchStrategy.cs    # Реалізація SAX (XmlReader)
│   ├── DomSearchStrategy.cs    # Реалізація DOM (XmlDocument)
│   ├── LinqToXmlSearchStrategy.cs # Реалізація LINQ to XML
│   └── XmlMetadataExtractor.cs # Динамічне вилучення атрибутів для ComboBox
├── Exporters/
│   ├── IFileSaver.cs           # Інтерфейс продукту Factory Method
│   ├── XmlFileSaver.cs         # XML-серіалізатор
│   ├── HtmlFileSaver.cs        # HTML-серіалізатор
│   ├── BaseExporter.cs         # Базовий творець Factory Method
│   ├── XmlExporter.cs          # Творець XML
│   └── HtmlExporter.cs         # Творець HTML
├── Services/
│   ├── XmlTransformer.cs       # XSLT-трансформація (XslCompiledTransform)
│   ├── IGoogleDriveService.cs  # Контракт хмарного сервісу
│   └── GoogleDriveService.cs   # Реалізація Google Drive API v3 (OAuth 2.0 / Service Account)
├── Logging/
│   ├── EventLevel.cs           # Рівні аудиту (Filtering, Transformation, Saving)
│   └── AppLogger.cs            # Singleton логер (audit.log)
├── ViewModels/
│   ├── ViewModelBase.cs        # Базовий клас ViewModel
│   └── MainViewModel.cs        # Головна бізнес-модель інтерфейсу
├── Views/
│   ├── MainWindow.axaml        # Головне вікно застосунку
│   ├── MainWindow.axaml.cs     # Чистий Code-Behind (без бізнес-коду)
│   ├── ConfirmExitWindow.axaml # Модальний діалог підтвердження виходу
│   ├── GoogleDriveBrowserWindow.axaml # Вікно перегляду файлів у хмарі
│   └── MessageBoxWindow.axaml  # Модальні повідомлення
└── XMLApp.csproj               # Конфігурація проєкту та NuGet-залежності
```

---

## 📄 Вимоги до даних (XML)

1. Файл library.xml має багаторівневу структуру: корінь `<Library>` $\rightarrow$ вузли `<Publication>` $\rightarrow$ дочірні теги `<Title>` та `<Annotation>`.
2. Головні вузли `<Publication>` можуть мати **неоднакову кількість атрибутів** у різних записах (4, 5, 6 та 7 атрибутів: `id`, `author`, `category`, `faculty`, `department`, `year`, `edition`, `isbn`, `language`).

---

## 🚀 Встановлення та запуск

### Системні вимоги:
* [.NET 10 SDK](https://dotnet.microsoft.com/download) (або .NET 8+)
* Linux / macOS / Windows

### Збірка:
```bash
dotnet build
```

### Запуск програми:
```bash
dotnet run
```

---

## ☁ Налаштування Google Drive API

Застосунок підтримує роботу з Google Drive API v3 через **OAuth 2.0 Client ID** або **Service Account**:
1. Створіть проєкт у [Google Cloud Console](https://console.cloud.google.com/) та увімкніть **Google Drive API**.
2. У розділі **Credentials** створіть **OAuth 2.0 Client ID** (тип *Desktop app*) або **Service Account**.
3. Завантажте JSON-файл ключа, перейменуйте його в `credentials.json` і покладіть поруч із виконуваним файлом програми або у корінь каталогу запуску.
4. При першому зверненні до хмари через OAuth 2.0 програма автоматично відкриє браузер для одноразового підтвердження входу під вашим обліковим записом Google.

---

