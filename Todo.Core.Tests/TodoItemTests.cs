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
