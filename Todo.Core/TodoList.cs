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
