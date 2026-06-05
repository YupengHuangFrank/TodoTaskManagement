using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TodoTaskManagement.Domain.Users;
using TodoTaskManagement.Infrastructure.Data;
using TodoTaskManagement.Infrastructure.Users;

namespace TodoTaskManagement.Tests.Infrastructure;

[TestFixture]
public class UserRepositoryTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _context = null!;
    private UserRepository _repository = null!;

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
        _repository = new UserRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private static User MakeUser(string? email = null) => new()
    {
        Id = Guid.NewGuid(),
        Email = email ?? $"user_{Guid.NewGuid():N}@example.com",
        PasswordHash = "hash"
    };

    // --- AddAsync ---

    [Test]
    public async Task AddAsync_PersistsUserToDatabase()
    {
        var user = MakeUser("add@example.com");

        await _repository.AddAsync(user);

        var verifyOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        await using var verifyContext = new AppDbContext(verifyOptions);
        var saved = await verifyContext.Users.FindAsync(user.Id);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Email, Is.EqualTo("add@example.com"));
    }

    // --- GetByIdAsync ---

    [Test]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        var user = MakeUser();
        await _repository.AddAsync(user);

        var result = await _repository.GetByIdAsync(user.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    // --- GetByEmailAsync ---

    [Test]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        var user = MakeUser("find@example.com");
        await _repository.AddAsync(user);

        var result = await _repository.GetByEmailAsync("find@example.com");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("find@example.com"));
    }

    [Test]
    public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
    {
        var result = await _repository.GetByEmailAsync("nobody@example.com");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByEmailAsync_MultipleUsers_ReturnsCorrectUser()
    {
        var user1 = MakeUser("first@example.com");
        var user2 = MakeUser("second@example.com");
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);

        var result = await _repository.GetByEmailAsync("second@example.com");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user2.Id));
    }
}
