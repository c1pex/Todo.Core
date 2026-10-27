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
