# Todo.Core

[![CI / Build & Test](https://github.com/utos199/Todo.Core/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/utos199/Todo.Core/actions/workflows/ci.yml)

Учебная библиотека для работы со списком задач: добавление, удаление, поиск,
отметка выполнения и сохранение в JSON. Целевая платформа — **.NET 8**.

> В примерах ниже владелец репозитория — `utos199`, репозиторий — `Todo.Core`.
> Если вы используете другие имена, замените их в URL, бейдже и источнике NuGet.

## Возможности

- `TodoList.Add(string title)` — добавить задачу; название обрезается по краям.
- `TodoList.Remove(Guid id)` — удалить задачу по `Id`; возвращает `true`, если задача найдена.
- `TodoList.Find(string searchText)` — найти задачи по подстроке без учёта регистра.
- `TodoList.Count` и `TodoList.Items` — количество задач и доступный только для чтения список.
- `TodoItem.MarkDone()` / `TodoItem.MarkUndone()` — изменить состояние задачи.
- `TodoItem.Rename(string title)` — переименовать задачу.
- `TodoList.Save(string path)` / `TodoList.Load(string path)` — сохранить и загрузить список в JSON.
  При загрузке сохраняются `Id`, `Title` и `IsDone`, а текущее содержимое списка заменяется.

## Установка пакета

Пакет публикуется в **GitHub Packages**. Для установки нужен
[personal access token (classic)](https://github.com/settings/tokens) с правом
`read:packages` (для публикации — `write:packages`).

### 1. Добавить источник GitHub Packages

```bash
dotnet nuget add source "https://nuget.pkg.github.com/utos199/index.json" --username <GITHUB_ЛОГИН> --password <PAT> --store-password-in-clear-text --name github
```

Параметр `--store-password-in-clear-text` нужен, потому что NuGet не умеет
хранить токены GitHub Packages в зашифрованном виде на всех платформах.

### 2. Установить пакет

```bash
dotnet add package Todo.Core --version 0.1.0 --source github
```

## Пример использования

```csharp
using Todo.Core;

var list = new TodoList();

var bread = list.Add("  Купить хлеб  ");
list.Add("Позвонить маме");

bread.MarkDone();
bread.Rename("Купить хлеб и молоко");

var found = list.Find("хлеб");          // поиск без учёта регистра
Console.WriteLine($"Найдено задач: {found.Count}");

list.Save("todo.json");                 // сохранить в JSON
list.Load("todo.json");                 // заменить содержимое данными из файла

Console.WriteLine($"Всего задач: {list.Count}");
foreach (var item in list.Items)
{
    Console.WriteLine($"{(item.IsDone ? "[x]" : "[ ]")} {item.Title}");
}
```

`Save` и `Load` выбрасывают `ArgumentException`, если путь пустой или состоит
из пробелов. `Load` также выбрасывает `FileNotFoundException`, если файл не
существует.

## Запуск тестов

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

Для запуска только тестов библиотеки:

```bash
dotnet test Todo.Core.Tests/Todo.Core.Tests.csproj -c Release
```

## CI/CD

- **CI / Build & Test** (`.github/workflows/ci.yml`) запускается при `push` и
  `pull_request` в ветку `master`: восстанавливает зависимости, собирает решение
  в Release, запускает тесты и создаёт NuGet-пакет, который сохраняется как
  артефакт сборки.
- **Publish package to GitHub Packages**
  (`.github/workflows/publish-package.yml`) запускается через `workflow_run`
  после успешного завершения CI и отправляет `*.nupkg` в GitHub Packages.
  Используется `GITHUB_TOKEN`, права `packages: write` объявлены в workflow.

`workflow_run` срабатывает только для workflow, файл которого находится в
ветке по умолчанию (`master`). Поэтому первая публикация произойдёт после
слияния `ci.yml` в `master`.

## Структура репозитория

```text
.
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── publish-package.yml
├── Todo.Core/
│   ├── Todo.Core.csproj
│   ├── TodoItem.cs
│   └── TodoList.cs
├── Todo.Core.Tests/
│   ├── Todo.Core.Tests.csproj
│   ├── TodoItemTests.cs
│   ├── TodoListTests.cs
│   └── TodoListPersistenceTests.cs
├── TodoSolution.sln
├── README.md
└── REPORT.md
```
