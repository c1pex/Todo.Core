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
