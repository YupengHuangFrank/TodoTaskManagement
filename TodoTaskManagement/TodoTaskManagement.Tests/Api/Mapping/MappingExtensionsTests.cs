using NUnit.Framework;
using TodoTaskManagement.Mapping;
using TodoTaskManagement.Models;
using DomainTask = TodoTaskManagement.Domain.Tasks.Task;
using DomainTaskStatus = TodoTaskManagement.Domain.Tasks.TaskStatus;

namespace TodoTaskManagement.Tests.Api.Mapping;

[TestFixture]
public class MappingExtensionsTests
{
    [Test]
    public void ToApi_MapsAllFieldsCorrectly()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var domain = new DomainTask
        {
            Id = id,
            UserId = userId,
            Title = "Test task",
            Description = "A description",
            DueDate = dueDate,
            Status = DomainTaskStatus.InProgress,
            IsArchived = true,
            CreatedAt = createdAt
        };

        var api = domain.ToApi();

        Assert.That(api.Id, Is.EqualTo(id));
        Assert.That(api.Title, Is.EqualTo("Test task"));
        Assert.That(api.Description, Is.EqualTo("A description"));
        Assert.That(api.DueDate, Is.EqualTo(dueDate));
        Assert.That(api.Status, Is.EqualTo(DomainTaskStatus.InProgress));
        Assert.That(api.IsArchived, Is.True);
        Assert.That(api.CreatedAt, Is.EqualTo(createdAt));
    }

    [Test]
    public void ToApi_WithNullOptionalFields_MapsNullsCorrectly()
    {
        var domain = new DomainTask
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Task",
            Description = null,
            DueDate = null,
            Status = DomainTaskStatus.Todo,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var api = domain.ToApi();

        Assert.That(api.Description, Is.Null);
        Assert.That(api.DueDate, Is.Null);
        Assert.That(api.IsArchived, Is.False);
    }

    [Test]
    public void ToDomain_MapsAllFieldsCorrectly()
    {
        var userId = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var dueDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var api = new TaskApi
        {
            Id = existingId,
            Title = "Api task",
            Description = "Desc",
            DueDate = dueDate,
            Status = DomainTaskStatus.Done,
            IsArchived = true
        };

        var domain = api.ToDomain(userId);

        Assert.That(domain.Id, Is.EqualTo(existingId));
        Assert.That(domain.UserId, Is.EqualTo(userId));
        Assert.That(domain.Title, Is.EqualTo("Api task"));
        Assert.That(domain.Description, Is.EqualTo("Desc"));
        Assert.That(domain.DueDate, Is.EqualTo(dueDate));
        Assert.That(domain.Status, Is.EqualTo(DomainTaskStatus.Done));
        Assert.That(domain.IsArchived, Is.True);
        Assert.That(domain.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void ToDomain_WhenIdIsNull_GeneratesNewId()
    {
        var api = new TaskApi
        {
            Id = null,
            Title = "Task",
            Status = DomainTaskStatus.Todo
        };

        var domain = api.ToDomain(Guid.NewGuid());

        Assert.That(domain.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void ToDomain_WhenIsArchivedIsNull_DefaultsToFalse()
    {
        var api = new TaskApi
        {
            Title = "Task",
            Status = DomainTaskStatus.Todo,
            IsArchived = null
        };

        var domain = api.ToDomain(Guid.NewGuid());

        Assert.That(domain.IsArchived, Is.False);
    }

    [Test]
    public void ToDomain_SetsCreatedAtToUtcNow()
    {
        var before = DateTime.UtcNow;
        var api = new TaskApi { Title = "Task", Status = DomainTaskStatus.Todo };

        var domain = api.ToDomain(Guid.NewGuid());
        var after = DateTime.UtcNow;

        Assert.That(domain.CreatedAt, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(after));
        Assert.That(domain.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
    }
}
