namespace Todo.Core;

/// <summary>
/// Коллекция задач с операциями добавления, удаления и поиска.
/// </summary>
public sealed class TodoList
{
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
}
