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
