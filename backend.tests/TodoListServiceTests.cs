using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.TodoLists;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests;

public class TodoListServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private async Task<(AppDbContext db, User owner, TodoList list)> SeedListAsync()
    {
        var db = CreateDb();
        var owner = new User { Email = "owner@example.com", PasswordHash = "x" };
        var list = new TodoList { Name = "Test List", OwnerId = owner.Id };
        db.Users.Add(owner);
        db.TodoLists.Add(list);
        await db.SaveChangesAsync();
        return (db, owner, list);
    }

    [Fact]
    public async Task GetListsForUser_ReturnsOwnedLists()
    {
        var (db, owner, list) = await SeedListAsync();
        var service = new TodoListService(db);

        var lists = await service.GetListsForUserAsync(owner.Id);

        Assert.Single(lists);
        Assert.Equal("Owner", lists.First().Role);
    }

    [Fact]
    public async Task GetListsForUser_ReturnsSharedLists()
    {
        var (db, owner, list) = await SeedListAsync();
        var sharedUser = new User { Email = "shared@example.com", PasswordHash = "x" };
        db.Users.Add(sharedUser);
        db.ListPermissions.Add(new ListPermission
        {
            TodoListId = list.Id,
            UserId = sharedUser.Id,
            Role = PermissionRole.Editor
        });
        await db.SaveChangesAsync();

        var service = new TodoListService(db);
        var lists = await service.GetListsForUserAsync(sharedUser.Id);

        Assert.Single(lists);
        Assert.Equal("Editor", lists.First().Role);
    }

    [Fact]
    public async Task GetListsForUser_DoesNotReturnOtherUsersLists()
    {
        var (db, owner, list) = await SeedListAsync();
        var otherUser = new User { Email = "other@example.com", PasswordHash = "x" };
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        var service = new TodoListService(db);
        var lists = await service.GetListsForUserAsync(otherUser.Id);

        Assert.Empty(lists);
    }

    [Fact]
    public async Task DeleteList_Throws_WhenUserIsNotOwner()
    {
        var (db, owner, list) = await SeedListAsync();
        var sharedUser = new User { Email = "shared@example.com", PasswordHash = "x" };
        db.Users.Add(sharedUser);
        db.ListPermissions.Add(new ListPermission
        {
            TodoListId = list.Id,
            UserId = sharedUser.Id,
            Role = PermissionRole.Editor
        });
        await db.SaveChangesAsync();

        var service = new TodoListService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.DeleteListAsync(list.Id, sharedUser.Id));
    }

    [Fact]
    public async Task GetList_Throws_WhenUserHasNoAccess()
    {
        var (db, owner, list) = await SeedListAsync();
        var otherUser = new User { Email = "other@example.com", PasswordHash = "x" };
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        var service = new TodoListService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetListAsync(list.Id, otherUser.Id));
    }
}
