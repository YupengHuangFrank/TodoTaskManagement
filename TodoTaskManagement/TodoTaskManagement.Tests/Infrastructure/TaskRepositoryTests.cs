using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TodoTaskManagement.Infrastructure.Data;
using TodoTaskManagement.Infrastructure.Tasks;
using DomainTask = TodoTaskManagement.Domain.Tasks.Task;
using DomainTaskStatus = TodoTaskManagement.Domain.Tasks.TaskStatus;

namespace TodoTaskManagement.Tests.Infrastructure;

[TestFixture]
public class TaskRepositoryTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _context = null!;
    private TaskRepository _repository = null!;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new TaskRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private static DomainTask MakeTask(Guid userId, DomainTaskStatus status = DomainTaskStatus.Todo, bool isArchived = false) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = "Test Task",
        Status = status,
        IsArchived = isArchived,
        CreatedAt = DateTime.UtcNow
    };

    private async Task SeedUserAsync(Guid userId)
    {
        _context.Users.Add(new TodoTaskManagement.Domain.Users.User
        {
            Id = userId,
            Email = $"{userId}@test.com",
            PasswordHash = "hash"
        });
        await _context.SaveChangesAsync();
    }

    // --- GetByUserIdAsync ---

    [Test]
    public async Task GetByUserIdAsync_ReturnsOnlyTasksForUser()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await SeedUserAsync(userId);
        await SeedUserAsync(otherUserId);

        await _repository.AddAsync(MakeTask(userId));
        await _repository.AddAsync(MakeTask(userId));
        await _repository.AddAsync(MakeTask(otherUserId));

        var result = await _repository.GetByUserIdAsync(userId, false);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(t => t.UserId == userId), Is.True);
    }

    [Test]
    public async Task GetByUserIdAsync_ArchivedFalse_ExcludesArchivedTasks()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);

        await _repository.AddAsync(MakeTask(userId, isArchived: false));
        await _repository.AddAsync(MakeTask(userId, isArchived: true));

        var result = await _repository.GetByUserIdAsync(userId, false);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].IsArchived, Is.False);
    }

    [Test]
    public async Task GetByUserIdAsync_ArchivedTrue_ReturnsOnlyArchivedTasks()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);

        await _repository.AddAsync(MakeTask(userId, isArchived: false));
        await _repository.AddAsync(MakeTask(userId, isArchived: true));

        var result = await _repository.GetByUserIdAsync(userId, true);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].IsArchived, Is.True);
    }

    // --- GetByIdAsync ---

    [Test]
    public async Task GetByIdAsync_ExistingId_ReturnsTask()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        var task = MakeTask(userId);
        await _repository.AddAsync(task);

        var result = await _repository.GetByIdAsync(task.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(task.Id));
    }

    [Test]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    // --- AddAsync ---

    [Test]
    public async Task AddAsync_PersistsTaskToDatabase()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        var task = MakeTask(userId);

        await _repository.AddAsync(task);

        var verifyOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        await using var verifyContext = new AppDbContext(verifyOptions);
        var saved = await verifyContext.Tasks.FindAsync(task.Id);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Title, Is.EqualTo(task.Title));
    }

    // --- DeleteAsync ---

    [Test]
    public async Task DeleteAsync_RemovesTaskFromDatabase()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        var task = MakeTask(userId);
        await _repository.AddAsync(task);

        await _repository.DeleteAsync(task);

        var verifyOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        await using var verifyContext = new AppDbContext(verifyOptions);
        var deleted = await verifyContext.Tasks.FindAsync(task.Id);

        Assert.That(deleted, Is.Null);
    }

    // --- SaveChangesAsync (CreatedAt preservation) ---

    [Test]
    public async Task SaveChangesAsync_PreservesOriginalCreatedAt()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        var originalCreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var task = MakeTask(userId);
        task.CreatedAt = originalCreatedAt;
        await _repository.AddAsync(task);

        // Detach so we can re-track it with modified CreatedAt
        _context.Entry(task).State = EntityState.Detached;

        var tracked = await _repository.GetByIdAsync(task.Id);
        tracked!.CreatedAt = DateTime.UtcNow.AddYears(5); // attempt to change it

        await _repository.SaveChangesAsync();

        var verifyOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        await using var verifyContext = new AppDbContext(verifyOptions);
        var saved = await verifyContext.Tasks.FindAsync(task.Id);

        Assert.That(saved!.CreatedAt, Is.EqualTo(originalCreatedAt));
    }

    // --- ArchiveAllDoneAsync ---

    [Test]
    public async Task ArchiveAllDoneAsync_ArchivesOnlyDoneTasksForUser()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        await SeedUserAsync(userId);
        await SeedUserAsync(otherUserId);

        var todoTask = MakeTask(userId, DomainTaskStatus.Todo);
        var doneTask = MakeTask(userId, DomainTaskStatus.Done);
        var otherUserDoneTask = MakeTask(otherUserId, DomainTaskStatus.Done);

        await _repository.AddAsync(todoTask);
        await _repository.AddAsync(doneTask);
        await _repository.AddAsync(otherUserDoneTask);

        await _repository.ArchiveAllDoneAsync(userId);

        var verifyOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        await using var verifyContext = new AppDbContext(verifyOptions);
        var all = await verifyContext.Tasks.ToListAsync();

        var userTodo = all.First(t => t.Id == todoTask.Id);
        var userDone = all.First(t => t.Id == doneTask.Id);
        var otherDone = all.First(t => t.Id == otherUserDoneTask.Id);

        Assert.That(userTodo.IsArchived, Is.False, "Todo task should not be archived");
        Assert.That(userDone.IsArchived, Is.True, "Done task for target user should be archived");
        Assert.That(otherDone.IsArchived, Is.False, "Done task for another user should not be archived");
    }

    [Test]
    public async Task ArchiveAllDoneAsync_AlreadyArchivedDoneTask_RemainsArchived()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        var alreadyArchived = MakeTask(userId, DomainTaskStatus.Done, isArchived: true);
        await _repository.AddAsync(alreadyArchived);

        await _repository.ArchiveAllDoneAsync(userId);

        var verifyOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        await using var verifyContext = new AppDbContext(verifyOptions);
        var task = await verifyContext.Tasks.FindAsync(alreadyArchived.Id);

        Assert.That(task!.IsArchived, Is.True);
    }
}
