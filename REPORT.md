# Отчёт по практическому занятию ПЗ3

## 1. Номер и название практического занятия

**ПЗ3. «Проектирование и публикация .NET-библиотеки с CI/CD»**

МДК 02.02. Разработка, внедрение и адаптация программного обеспечения отраслевой направленности.

## 2. Цели занятия

Кратко: закрепить умения по компетенциям ОК 01, ОК 02, ОК 09 и ПК 2.3.

- **ОК 01** — выбирать способы решения задач профессиональной деятельности применительно к различным контекстам.
- **ОК 02** — использовать современные средства поиска, анализа и интерпретации информации и информационные технологии для выполнения задач профессиональной деятельности.
- **ОК 09** — пользоваться профессиональной документацией на государственном и иностранном языках.
- **ПК 2.3** — разрабатывать и сопровождать программное обеспечение, в том числе оформлять библиотеку как переиспользуемый NuGet-пакет.

В ходе работы получен опыт:

- проектирования решения из библиотеки и отдельного тестового проекта;
- написания модульных тестов xUnit и проверки граничных случаев;
- работы с Git: отдельные feature-ветки, осмысленные коммиты, слияние в `master`;
- настройки CI (restore, build, test, pack) и CD (публикация в GitHub Packages);
- документирования библиотеки и подготовки её к распространению.

## 3. Ход выполнения

### 3.1. Подготовка репозитория и структура проекта

Репозиторий инициализирован с веткой по умолчанию `master`:

```bash
git init -b master
dotnet new gitignore
```

В `.gitignore` дополнительно добавлен каталог `/artifacts/`, чтобы локальные NuGet-пакеты не попадали в репозиторий. Работа велась в отдельных ветках, после проверки каждая ветка сливалась в `master` командой `git merge --no-ff` (GitHub CLI не был авторизован, поэтому pull request на GitHub не создавались).

| Ветка | Назначение | Основной коммит |
|---|---|---|
| `feature/core` | решение, проект `Todo.Core`, базовые классы | `7569322` |
| `feature/tests` | проект `Todo.Core.Tests`, тесты xUnit | `c46e855` |
| `feature/json-persistence` | `Save`/`Load`, JSON-тесты | `e18b7f9` |
| `feature/ci` | workflow CI/CD и README | `e475f16`, `19c8cca` |

Структура проекта:

```text
D:\ПР3
├── .github
│   └── workflows
│       ├── ci.yml
│       └── publish-package.yml
├── Todo.Core
│   ├── Todo.Core.csproj
│   ├── TodoItem.cs
│   └── TodoList.cs
├── Todo.Core.Tests
│   ├── Todo.Core.Tests.csproj
│   ├── TodoItemTests.cs
│   ├── TodoListTests.cs
│   └── TodoListPersistenceTests.cs
├── .gitignore
├── README.md
├── REPORT.md
├── REPORT.docx
└── TodoSolution.sln
```

Каталоги `bin`, `obj` и `artifacts` создаются при сборке и исключены из Git.

### 3.2. Проект библиотеки Todo.Core

Файл `Todo.Core/Todo.Core.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <!-- Пакет собирается только явной командой dotnet pack. -->
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>

    <PackageId>Todo.Core</PackageId>
    <Version>0.1.0</Version>
    <Authors>Utos199</Authors>
    <Company>Practice</Company>
    <Description>Simple Todo core library (for practice)</Description>
    <PackageRequireLicenseAcceptance>false</PackageRequireLicenseAcceptance>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <RepositoryUrl>https://github.com/utos199/Todo.Core</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
  </PropertyGroup>

  <ItemGroup>
    <!-- README из корня репозитория включается в NuGet-пакет. -->
    <None Include="..\README.md" Pack="true" PackagePath="\" Link="README.md" />
  </ItemGroup>

</Project>
```

Метаданные пакета настроены в соответствии с заданием:

- `TargetFramework` — `net8.0`, включены `ImplicitUsings` и `Nullable`;
- `GeneratePackageOnBuild` — `false`, пакет создаётся только явной командой `dotnet pack`;
- `PackageId` — `Todo.Core`, `Version` — `0.1.0`;
- заполнены `Authors`, `Company`, `Description`, `RepositoryUrl`, `RepositoryType`;
- `PackageRequireLicenseAcceptance` — `false`;
- `PackageReadmeFile` — `README.md`, корневой README включается в пакет через элемент `None` с `Pack="true"`.

Файл `Todo.Core/TodoItem.cs`:

```csharp
using System.Text.Json.Serialization;

namespace Todo.Core;

/// <summary>
/// Элемент списка задач.
/// </summary>
public sealed class TodoItem
{
    /// <summary>
    /// Создаёт новую задачу с указанным названием.
    /// </summary>
    /// <param name="title">Название задачи.</param>
    /// <exception cref="ArgumentNullException">Название равно <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Название пустое или состоит только из пробелов.</exception>
    public TodoItem(string title)
        : this(Guid.NewGuid(), title, false)
    {
    }

    /// <summary>
    /// Создаёт задачу с заданными идентификатором и состоянием.
    /// Используется System.Text.Json при десериализации, чтобы сохранить исходный Id.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="title">Название задачи.</param>
    /// <param name="isDone">Признак выполнения задачи.</param>
    /// <exception cref="ArgumentException">Идентификатор пустой или название пустое.</exception>
    /// <exception cref="ArgumentNullException">Название равно <see langword="null"/>.</exception>
    [JsonConstructor]
    public TodoItem(Guid id, string title, bool isDone)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор задачи не может быть пустым.", nameof(id));
        }

        Id = id;
        Title = NormalizeTitle(title);
        IsDone = isDone;
    }

    /// <summary>
    /// Уникальный идентификатор задачи.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Название задачи без пробелов по краям.
    /// </summary>
    [JsonInclude]
    public string Title { get; private set; }

    /// <summary>
    /// Признак того, что задача выполнена.
    /// </summary>
    [JsonInclude]
    public bool IsDone { get; private set; }

    /// <summary>
    /// Отмечает задачу как выполненную.
    /// </summary>
    public void MarkDone()
    {
        IsDone = true;
    }

    /// <summary>
    /// Возвращает задачу в состояние «не выполнена».
    /// </summary>
    public void MarkUndone()
    {
        IsDone = false;
    }

    /// <summary>
    /// Изменяет название задачи.
    /// </summary>
    /// <param name="title">Новое название задачи.</param>
    /// <exception cref="ArgumentNullException">Название равно <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Название пустое или состоит только из пробелов.</exception>
    public void Rename(string title)
    {
        Title = NormalizeTitle(title);
    }

    /// <summary>
    /// Проверяет и нормализует название задачи.
    /// </summary>
    private static string NormalizeTitle(string title)
    {
        if (title is null)
        {
            throw new ArgumentNullException(nameof(title));
        }

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length == 0)
        {
            throw new ArgumentException("Название задачи не может быть пустым.", nameof(title));
        }

        return trimmedTitle;
    }
}
```

Файл `Todo.Core/TodoList.cs`:

```csharp
using System.Text.Json;

namespace Todo.Core;

/// <summary>
/// Коллекция задач с операциями добавления, удаления, поиска и сохранения в JSON.
/// </summary>
public sealed class TodoList
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly List<TodoItem> _items = new();

    /// <summary>
    /// Задачи в порядке добавления.
    /// </summary>
    public IReadOnlyList<TodoItem> Items => _items;

    /// <summary>
    /// Количество задач в списке.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Добавляет задачу с указанным названием.
    /// </summary>
    /// <param name="title">Название задачи.</param>
    /// <returns>Созданная задача.</returns>
    /// <exception cref="ArgumentNullException">Название равно <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Название пустое или состоит только из пробелов.</exception>
    public TodoItem Add(string title)
    {
        var item = new TodoItem(title);
        _items.Add(item);
        return item;
    }

    /// <summary>
    /// Удаляет задачу по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <returns><see langword="true"/>, если задача была удалена; иначе <see langword="false"/>.</returns>
    public bool Remove(Guid id)
    {
        var removedCount = _items.RemoveAll(item => item.Id == id);
        return removedCount > 0;
    }

    /// <summary>
    /// Ищет задачи, название которых содержит указанную подстроку (без учёта регистра).
    /// </summary>
    /// <param name="searchText">Подстрока для поиска.</param>
    /// <returns>Список найденных задач; пустой список, если совпадений нет.</returns>
    /// <exception cref="ArgumentNullException">Строка поиска равна <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Строка поиска пустая или состоит только из пробелов.</exception>
    public IReadOnlyList<TodoItem> Find(string searchText)
    {
        if (searchText is null)
        {
            throw new ArgumentNullException(nameof(searchText));
        }

        var query = searchText.Trim();
        if (query.Length == 0)
        {
            throw new ArgumentException("Строка поиска не может быть пустой.", nameof(searchText));
        }

        return _items
            .Where(item => item.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Сохраняет список задач в JSON-файл.
    /// </summary>
    /// <param name="path">Путь к файлу.</param>
    /// <exception cref="ArgumentException">Путь пустой или состоит только из пробелов.</exception>
    public void Save(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(path));
        }

        var json = JsonSerializer.Serialize(_items, JsonOptions);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Загружает задачи из JSON-файла, заменяя текущее содержимое списка.
    /// </summary>
    /// <param name="path">Путь к файлу.</param>
    /// <exception cref="ArgumentException">Путь пустой или состоит только из пробелов.</exception>
    /// <exception cref="FileNotFoundException">Файл по указанному пути не найден.</exception>
    public void Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Файл со списком задач не найден.", path);
        }

        var json = File.ReadAllText(path);
        var loadedItems = JsonSerializer.Deserialize<List<TodoItem>>(json, JsonOptions) ?? new List<TodoItem>();

        _items.Clear();
        _items.AddRange(loadedItems);
    }
}
```

Особенности реализации:

- `TodoItem` обрезает название по краям, выбрасывает `ArgumentNullException` для `null` и `ArgumentException` для пустой строки;
- `Id` создаётся один раз и доступен только для чтения;
- для десериализации добавлен публичный конструктор `TodoItem(Guid id, string title, bool isDone)` с атрибутом `[JsonConstructor]`, поэтому при загрузке `Id` не генерируется заново;
- свойства `Title` и `IsDone` имеют приватные сеттеры и помечены `[JsonInclude]`;
- `TodoList.Find` выполняет поиск без учёта регистра через `StringComparison.OrdinalIgnoreCase`;
- `Save`/`Load` используют `System.Text.Json`; `Load` сначала читает и десериализует файл, а затем заменяет содержимое `_items`, поэтому при ошибке чтения старый список не теряется;
- пустой путь проверяется через `string.IsNullOrWhiteSpace` и приводит к `ArgumentException`, отсутствующий файл — к `FileNotFoundException`.

### 3.3. Тесты

Файл `Todo.Core.Tests/Todo.Core.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.5.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Todo.Core\Todo.Core.csproj" />
  </ItemGroup>

</Project>
```

Файл `Todo.Core.Tests/TodoItemTests.cs`:

```csharp
using Todo.Core;

namespace Todo.Core.Tests;

/// <summary>
/// Тесты элемента списка задач.
/// </summary>
public sealed class TodoItemTests
{
    [Fact]
    public void Constructor_TrimsTitleAndCreatesActiveItem()
    {
        var item = new TodoItem("  Купить хлеб  ");

        Assert.Equal("Купить хлеб", item.Title);
        Assert.False(item.IsDone);
        Assert.NotEqual(Guid.Empty, item.Id);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenTitleIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TodoItem(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsArgumentException_WhenTitleIsEmpty(string title)
    {
        Assert.Throws<ArgumentException>(() => new TodoItem(title));
    }

    [Fact]
    public void MarkDone_SetsIsDoneTrue()
    {
        var item = new TodoItem("Подготовить отчёт");

        item.MarkDone();

        Assert.True(item.IsDone);
    }

    [Fact]
    public void MarkUndone_SetsIsDoneFalse()
    {
        var item = new TodoItem("Подготовить отчёт");
        item.MarkDone();

        item.MarkUndone();

        Assert.False(item.IsDone);
    }

    [Fact]
    public void Rename_ChangesAndTrimsTitle()
    {
        var item = new TodoItem("Старое название");

        item.Rename("  Новое название  ");

        Assert.Equal("Новое название", item.Title);
    }

    [Fact]
    public void Rename_ThrowsArgumentNullException_WhenTitleIsNull()
    {
        var item = new TodoItem("Задача");

        Assert.Throws<ArgumentNullException>(() => item.Rename(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_ThrowsArgumentException_WhenTitleIsEmpty(string title)
    {
        var item = new TodoItem("Задача");

        Assert.Throws<ArgumentException>(() => item.Rename(title));
    }
}
```

Файл `Todo.Core.Tests/TodoListTests.cs`:

```csharp
using Todo.Core;

namespace Todo.Core.Tests;

/// <summary>
/// Тесты коллекции задач.
/// </summary>
public sealed class TodoListTests
{
    [Fact]
    public void Add_IncreasesCount()
    {
        var list = new TodoList();

        list.Add("Первая задача");
        list.Add("Вторая задача");

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void Remove_ById_Works()
    {
        var list = new TodoList();
        var first = list.Add("Первая задача");
        var second = list.Add("Вторая задача");

        var removed = list.Remove(first.Id);

        Assert.True(removed);
        Assert.Equal(1, list.Count);
        Assert.DoesNotContain(first, list.Items);
        Assert.Contains(second, list.Items);
    }

    [Fact]
    public void Remove_UnknownId_ReturnsFalse()
    {
        var list = new TodoList();
        list.Add("Задача");

        var removed = list.Remove(Guid.NewGuid());

        Assert.False(removed);
        Assert.Equal(1, list.Count);
    }

    [Fact]
    public void Find_ReturnsMatches()
    {
        var list = new TodoList();
        list.Add("Купить хлеб");
        list.Add("Купить молоко");
        list.Add("Позвонить маме");

        var found = list.Find("купить");

        Assert.Equal(2, found.Count);
        Assert.Contains(found, item => item.Title == "Купить хлеб");
        Assert.Contains(found, item => item.Title == "Купить молоко");
    }

    [Fact]
    public void Find_ReturnsEmptyList_WhenNothingMatches()
    {
        var list = new TodoList();
        list.Add("Купить хлеб");

        var found = list.Find("отчёт");

        Assert.Empty(found);
    }

    [Fact]
    public void Find_ThrowsArgumentNullException_WhenSearchTextIsNull()
    {
        var list = new TodoList();

        Assert.Throws<ArgumentNullException>(() => list.Find(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Find_ThrowsArgumentException_WhenSearchTextIsEmpty(string searchText)
    {
        var list = new TodoList();

        Assert.Throws<ArgumentException>(() => list.Find(searchText));
    }

    [Fact]
    public void Items_PreservesInsertionOrder()
    {
        var list = new TodoList();
        var first = list.Add("Первая");
        var second = list.Add("Вторая");

        Assert.Equal(new[] { first, second }, list.Items);
    }
}
```

Файл `Todo.Core.Tests/TodoListPersistenceTests.cs`:

```csharp
using Todo.Core;

namespace Todo.Core.Tests;

/// <summary>
/// Тесты сохранения и загрузки списка задач в JSON.
/// </summary>
public sealed class TodoListPersistenceTests
{
    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesIdTitleAndIsDone()
    {
        var path = Path.GetTempFileName();
        try
        {
            var source = new TodoList();
            var doneItem = source.Add("  Купить хлеб  ");
            var activeItem = source.Add("Позвонить маме");
            doneItem.MarkDone();
            source.Save(path);

            var restored = new TodoList();
            restored.Add("Эта задача должна исчезнуть после загрузки");
            restored.Load(path);

            Assert.Equal(2, restored.Count);

            var restoredDoneItem = Assert.Single(restored.Items, item => item.Id == doneItem.Id);
            Assert.Equal("Купить хлеб", restoredDoneItem.Title);
            Assert.True(restoredDoneItem.IsDone);

            var restoredActiveItem = Assert.Single(restored.Items, item => item.Id == activeItem.Id);
            Assert.Equal("Позвонить маме", restoredActiveItem.Title);
            Assert.False(restoredActiveItem.IsDone);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SaveAndLoad_EmptyList_Works()
    {
        var path = Path.GetTempFileName();
        try
        {
            var source = new TodoList();
            source.Save(path);

            var restored = new TodoList();
            restored.Add("Задача, которая должна исчезнуть");
            restored.Load(path);

            Assert.Empty(restored.Items);
            Assert.Equal(0, restored.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        Assert.False(File.Exists(path));

        var list = new TodoList();

        Assert.Throws<FileNotFoundException>(() => list.Load(path));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Save_ThrowsArgumentException_WhenPathIsInvalid(string? path)
    {
        var list = new TodoList();

        Assert.Throws<ArgumentException>(() => list.Save(path!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Load_ThrowsArgumentException_WhenPathIsInvalid(string? path)
    {
        var list = new TodoList();

        Assert.Throws<ArgumentException>(() => list.Load(path!));
    }
}
```

Итого 28 тестовых случаев:

| Группа | Количество | Что проверяется |
|---|---:|---|
| `TodoItemTests` | 10 | trim, `Id`, `MarkDone`/`MarkUndone`, `Rename`, исключения для `null` и пустой строки |
| `TodoListTests` | 9 | `Add_IncreasesCount`, `Remove_ById_Works`, удаление несуществующего `Id`, `Find_ReturnsMatches`, пустой результат, исключения поиска, порядок элементов |
| `TodoListPersistenceTests` | 9 | round-trip с сохранением `Id`/`Title`/`IsDone`, пустой список, замена содержимого, отсутствующий файл, неверный путь для `Save` и `Load` |

Тесты используют временные файлы `Path.GetTempFileName()` и удаляют их в блоке `finally`.

### 3.4. Workflow CI

Файл `.github/workflows/ci.yml`:

```yaml
name: "CI / Build & Test"

on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]

jobs:
  build-test-pack:
    name: Build, test and pack
    runs-on: ubuntu-latest

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Setup .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Restore dependencies
        run: dotnet restore TodoSolution.sln

      - name: Build
        run: dotnet build TodoSolution.sln --no-restore -c Release

      - name: Test
        run: dotnet test TodoSolution.sln --no-build -c Release --verbosity normal

      - name: Pack
        run: dotnet pack Todo.Core/Todo.Core.csproj -c Release -o ./artifacts

      - name: Upload NuGet package artifact
        uses: actions/upload-artifact@v4
        with:
          name: nuget-package
          path: ./artifacts/*.nupkg
          if-no-files-found: error
```

Workflow запускается на `push` и `pull_request` в `master`, выполняет restore, build в Release, тесты и упаковку. Кэш NuGet настроен на `~/.nuget/packages`; ключ включает ОС и хеш всех `*.csproj`, что соответствует заданию. `setup-dotnet` вызывается один раз. Дополнительно пакет сохраняется как артефакт Actions, чтобы его можно было скачать со страницы запуска.

### 3.5. Workflow публикации в GitHub Packages

Файл `.github/workflows/publish-package.yml`:

```yaml
name: "Publish package to GitHub Packages"

on:
  workflow_run:
    workflows: [ "CI / Build & Test" ]
    types: [ completed ]

permissions:
  contents: read
  packages: write

jobs:
  publish:
    name: Publish NuGet package
    if: ${{ github.event.workflow_run.conclusion == 'success' && github.event.workflow_run.event == 'push' && github.event.workflow_run.head_branch == 'master' }}
    runs-on: ubuntu-latest

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          ref: ${{ github.event.workflow_run.head_sha }}

      - name: Setup .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore TodoSolution.sln

      - name: Build
        run: dotnet build TodoSolution.sln --no-restore -c Release

      - name: Pack
        run: dotnet pack Todo.Core/Todo.Core.csproj -c Release --no-build -o ./artifacts

      - name: List package artifacts
        run: ls -la ./artifacts

      - name: Push package to GitHub Packages
        run: dotnet nuget push ./artifacts/*.nupkg --source "https://nuget.pkg.github.com/${{ github.repository_owner }}/index.json" --api-key ${{ secrets.GITHUB_TOKEN }} --skip-duplicate
```

Публикация запускается по событию `workflow_run` после завершения workflow с именем `CI / Build & Test`. Джоб выполняется только при успешном завершении CI, а также при событии `push` в `master`: это защищает от публикации пакета из pull request и из чужих форков. Права `contents: read` и `packages: write` объявлены на уровне workflow, поэтому отдельный PAT в Actions не нужен — используется автоматически выдаваемый `GITHUB_TOKEN`.

Важное ограничение: `workflow_run` срабатывает только для workflow, файл которого находится в ветке по умолчанию (`master`). Пока `ci.yml` не попал в `master`, публикация не запустится. После первого успешного запуска пакет появится в разделе Packages репозитория. Повторные запуски не падают из-за уже существующей версии благодаря `--skip-duplicate`; при выпуске новой версии нужно изменить `<Version>` в `Todo.Core.csproj`.

### 3.6. README и установка пакета

В `README.md` описаны возможности библиотеки, пример использования, запуск тестов, схема CI/CD и бейдж статуса Actions. Команды установки:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/utos199/index.json" --username <GITHUB_ЛОГИН> --password <PAT> --store-password-in-clear-text --name github
dotnet add package Todo.Core --version 0.1.0 --source github
```

Для установки нужен GitHub PAT с правом `read:packages`. В README использованы владелец `utos199` и репозиторий `Todo.Core`; если репозиторий будет называться иначе, эти значения нужно заменить.

Ссылка на страницу пакета в GitHub Packages:

```text
https://github.com/<OWNER>/<REPO>/packages
```

Реальная ссылка на момент подготовки отчёта не проверена, потому что пакет не публиковался (нет авторизации GitHub). При владельце `utos199` и репозитории `Todo.Core` ожидаемый адрес:

```text
https://github.com/utos199/Todo.Core/packages
```

> **Подставить после публикации:** замените `<OWNER>` и `<REPO>` фактическими значениями и проверьте ссылку в браузере.

### 3.7. Локальная проверка

Проверка выполнялась на Windows с портативным **.NET SDK 8.0.427** (точное соответствие CI, где указано `8.0.x`). Глобально в системе также установлен SDK 10.0.401; проект целится в `net8.0` и успешно собирается обоими SDK. Версии Git: 2.55.0.

Команда восстановления зависимостей:

```bash
dotnet restore TodoSolution.sln
```

Результат:

```text
  Определение проектов для восстановления...
  Восстановлен D:\ПР3\Todo.Core\Todo.Core.csproj (за 521 мс).
  Восстановлен D:\ПР3\Todo.Core.Tests\Todo.Core.Tests.csproj (за 5,14 с).
```

Сборка:

```bash
dotnet build TodoSolution.sln -c Release --no-restore
```

Результат:

```text
  Todo.Core -> D:\ПР3\Todo.Core\bin\Release\net8.0\Todo.Core.dll
  Todo.Core.Tests -> D:\ПР3\Todo.Core.Tests\bin\Release\net8.0\Todo.Core.Tests.dll

Сборка успешно завершена.
    Предупреждений: 0
    Ошибок: 0
```

Тесты:

```bash
dotnet test TodoSolution.sln -c Release --no-build --verbosity normal
```

Результат:

```text
Сборка начата 27.10.2026 15:43:41.
     1>Проект "D:\ПР3\TodoSolution.sln" в узле 1 (целевые объекты VSTest).
     1>ValidateSolutionConfiguration:
         Создание конфигурации решения "Release|Any CPU".
Тестовый запуск для D:\ПР3\Todo.Core.Tests\bin\Release\net8.0\Todo.Core.Tests.dll (.NETCoreApp,Version=v8.0)
Версия VSTest 17.11.1 (x64)

Запуск выполнения тестов; подождите...
Общее количество тестовых файлов (1), соответствующих указанному шаблону.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.13]   Discovering: Todo.Core.Tests
[xUnit.net 00:00:00.22]   Discovered:  Todo.Core.Tests
[xUnit.net 00:00:00.22]   Starting:    Todo.Core.Tests
  Пройден Todo.Core.Tests.TodoItemTests.Rename_ThrowsArgumentException_WhenTitleIsEmpty(title: "") [7 ms]
  Пройден Todo.Core.Tests.TodoListTests.Find_ThrowsArgumentException_WhenSearchTextIsEmpty(searchText: "   ") [7 ms]
  Пройден Todo.Core.Tests.TodoListPersistenceTests.Load_ThrowsArgumentException_WhenPathIsInvalid(path: "") [7 ms]
  Пройден Todo.Core.Tests.TodoItemTests.Rename_ThrowsArgumentException_WhenTitleIsEmpty(title: "   ") [< 1 ms]
  Пройден Todo.Core.Tests.TodoListPersistenceTests.Load_ThrowsArgumentException_WhenPathIsInvalid(path: "   ") [< 1 ms]
  Пройден Todo.Core.Tests.TodoListTests.Find_ThrowsArgumentException_WhenSearchTextIsEmpty(searchText: "") [< 1 ms]
  Пройден Todo.Core.Tests.TodoListPersistenceTests.Load_ThrowsArgumentException_WhenPathIsInvalid(path: null) [< 1 ms]
  Пройден Todo.Core.Tests.TodoListPersistenceTests.Save_ThrowsArgumentException_WhenPathIsInvalid(path: "") [< 1 ms]
  Пройден Todo.Core.Tests.TodoItemTests.Rename_ChangesAndTrimsTitle [< 1 ms]
  Пройден Todo.Core.Tests.TodoListPersistenceTests.Save_ThrowsArgumentException_WhenPathIsInvalid(path: "   ") [< 1 ms]
  Пройден Todo.Core.Tests.TodoListPersistenceTests.Save_ThrowsArgumentException_WhenPathIsInvalid(path: null) [< 1 ms]
  Пройден Todo.Core.Tests.TodoListPersistenceTests.Load_ThrowsFileNotFoundException_WhenFileDoesNotExist [2 ms]
  Пройден Todo.Core.Tests.TodoItemTests.Constructor_TrimsTitleAndCreatesActiveItem [3 ms]
  Пройден Todo.Core.Tests.TodoItemTests.MarkUndone_SetsIsDoneFalse [< 1 ms]
  Пройден Todo.Core.Tests.TodoItemTests.MarkDone_SetsIsDoneTrue [< 1 ms]
  Пройден Todo.Core.Tests.TodoItemTests.Constructor_ThrowsArgumentException_WhenTitleIsEmpty(title: "") [< 1 ms]
  Пройден Todo.Core.Tests.TodoItemTests.Constructor_ThrowsArgumentException_WhenTitleIsEmpty(title: "   ") [< 1 ms]
  Пройден Todo.Core.Tests.TodoItemTests.Rename_ThrowsArgumentNullException_WhenTitleIsNull [< 1 ms]
  Пройден Todo.Core.Tests.TodoItemTests.Constructor_ThrowsArgumentNullException_WhenTitleIsNull [< 1 ms]
  Пройден Todo.Core.Tests.TodoListTests.Find_ReturnsMatches [7 ms]
  Пройден Todo.Core.Tests.TodoListTests.Add_IncreasesCount [< 1 ms]
  Пройден Todo.Core.Tests.TodoListTests.Find_ThrowsArgumentNullException_WhenSearchTextIsNull [< 1 ms]
  Пройден Todo.Core.Tests.TodoListTests.Remove_ById_Works [4 ms]
  Пройден Todo.Core.Tests.TodoListTests.Find_ReturnsEmptyList_WhenNothingMatches [< 1 ms]
[xUnit.net 00:00:00.42]   Finished:    Todo.Core.Tests
  Пройден Todo.Core.Tests.TodoListTests.Items_PreservesInsertionOrder [8 ms]
  Пройден Todo.Core.Tests.TodoListTests.Remove_UnknownId_ReturnsFalse [< 1 ms]
  Пройден Todo.Core.Tests.TodoListPersistenceTests.SaveAndLoad_EmptyList_Works [51 ms]
  Пройден Todo.Core.Tests.TodoListPersistenceTests.SaveAndLoad_RoundTrip_PreservesIdTitleAndIsDone [36 ms]

Тестовый запуск выполнен.
Всего тестов: 28
     Пройдено: 28
 Общее время: 1,2443 Секунды
     1>Сборка проекта "D:\ПР3\TodoSolution.sln" завершена (целевые объекты VSTest).

Сборка успешно завершена.
    Предупреждений: 0
    Ошибок: 0

Прошло времени 00:00:02.90
```

Упаковка:

```bash
dotnet pack Todo.Core/Todo.Core.csproj -c Release -o ./artifacts
```

Результат:

```text
  Todo.Core -> D:\ПР3\Todo.Core\bin\Release\net8.0\Todo.Core.dll
  Пакет "D:\ПР3\artifacts\Todo.Core.0.1.0.nupkg" успешно создан.
```

Содержимое пакета `Todo.Core.0.1.0.nupkg`:

```text
README.md
Todo.Core.nuspec
[Content_Types].xml
lib/net8.0/Todo.Core.dll
package/services/metadata/core-properties/nuget.psmdcp
_rels/.rels
```

Метаданные `Todo.Core.nuspec`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd">
  <metadata>
    <id>Todo.Core</id>
    <version>0.1.0</version>
    <authors>Utos199</authors>
    <readme>README.md</readme>
    <description>Simple Todo core library (for practice)</description>
    <repository type="git" url="https://github.com/utos199/Todo.Core" commit="97b32d9765a9d48cda3d971b0df000c73f1adb65" />
    <dependencies>
      <group targetFramework="net8.0" />
    </dependencies>
  </metadata>
</package>
```

Дополнительно выполнена проверка потребителя пакета: во временном консольном проекте подключён `Todo.Core` из локального источника, создана задача, вызваны `MarkDone`, `Save` и `Load`. Вывод:

```text
Count=1; Title=Проверка пакета; IsDone=True; IdMatch=True
```

Это подтверждает, что пакет устанавливается, а `Id` и `IsDone` сохраняются при JSON round-trip.

Проверка YAML:

```bash
actionlint -color .github/workflows/ci.yml .github/workflows/publish-package.yml
```

Использован `actionlint` v1.7.12, код возврата `0`, замечаний нет. Python/PyYAML в среде отсутствует, поэтому вместо `python -c "import yaml"` применён более строгий анализатор GitHub Actions, который проверяет и синтаксис YAML, и семантику workflow.

### 3.8. Скриншоты и что осталось проверить

Публикация на GitHub не выполнялась: `gh` установлен, но не авторизован (`You are not logged into any GitHub hosts`), поэтому ссылки на Actions и пакет не выдумывались.

> **Место для скриншота 1.** Страница Actions → workflow «CI / Build & Test» → успешный запуск (зелёная галочка), видны шаги restore, build, test, pack.
>
> _Вставьте скриншот сюда._

> **Место для скриншота 2.** Страница Actions → workflow «Publish package to GitHub Packages» → успешный запуск после CI.
>
> _Вставьте скриншот сюда._

> **Место для скриншота 3.** Страница пакета `Todo.Core` в GitHub Packages (версия 0.1.0, README, инструкция установки).
>
> _Вставьте скриншот сюда._

> **Место для скриншота 4.** Список веток/история коммитов с merge-коммитами `feature/core`, `feature/tests`, `feature/json-persistence`, `feature/ci`.
>
> _Вставьте скриншот сюда._

### 3.9. Команды для завершения публикации

Создание репозитория и отправка локальной истории (выполнять в `D:\ПР3`):

```bash
gh auth login
gh repo create Todo.Core --public --source . --remote origin --push
git push -u origin master
git push origin feature/core feature/tests feature/json-persistence feature/ci
```

Если репозиторий уже создан вручную:

```bash
git remote add origin https://github.com/<OWNER>/Todo.Core.git
git push -u origin master
git push origin --all
```

Поскольку все feature-ветки уже слиты в локальный `master`, содержательный pull request из них после push получить нельзя: GitHub покажет, что изменений нет. Если по заданию требуется именно PR, создайте отдельную ветку с небольшим дополнением (например, со скриншотами) и отправьте её:

```bash
git switch -c feature/screenshots
mkdir docs
# добавьте файлы скриншотов и описание в docs/
git add docs
git commit -m "docs: add CI/CD screenshots"
git push -u origin feature/screenshots
gh pr create --base master --head feature/screenshots --title "docs: add CI/CD screenshots" --body "Скриншоты успешных запусков CI/CD и страницы пакета."
```

После первого успешного CI в `master` проверьте, что workflow публикации создал пакет, и подставьте реальную ссылку в README и отчёт.

## 4. Краткий вывод

В результате выполнения ПЗ3 спроектирована и реализована библиотека `Todo.Core` для .NET 8 с операциями добавления, удаления, поиска, изменения состояния задач и JSON-персистенцией. Создан отдельный проект модульных тестов xUnit; все 28 тестов проходят. Настроены CI-workflow (restore, build, test, pack) и CD-workflow (публикация NuGet-пакета в GitHub Packages через `workflow_run` после успешного CI). Подготовлены README с инструкцией установки и отчёт. Локально подтверждены сборка без предупреждений, успешный запуск тестов, создание `.nupkg` и его подключение в тестовом приложении. Осталось выполнить действия, требующие доступа к GitHub: создать репозиторий, отправить ветки, проверить Actions, опубликовать пакет и вставить скриншоты.
