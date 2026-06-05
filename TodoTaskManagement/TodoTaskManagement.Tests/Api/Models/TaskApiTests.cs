using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using TodoTaskManagement.Models;
using DomainTaskStatus = TodoTaskManagement.Domain.Tasks.TaskStatus;

namespace TodoTaskManagement.Tests.Api.Models;

[TestFixture]
public class TaskApiTests
{
    private static IList<ValidationResult> Validate(TaskApi model)
    {
        var context = new ValidationContext(model);
        return model.Validate(context).ToList();
    }

    [Test]
    public void Validate_ValidModel_ReturnsNoErrors()
    {
        var model = new TaskApi
        {
            Title = "Buy groceries",
            Status = DomainTaskStatus.Todo,
            DueDate = null
        };

        var results = Validate(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_ValidModelWithFutureDueDate_ReturnsNoErrors()
    {
        var model = new TaskApi
        {
            Title = "Task",
            Status = DomainTaskStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var results = Validate(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_NullTitle_ReturnsTitleError()
    {
        var model = new TaskApi { Title = null, Status = DomainTaskStatus.Todo };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Title"));
    }

    [Test]
    public void Validate_WhitespaceTitle_ReturnsTitleError()
    {
        var model = new TaskApi { Title = "   ", Status = DomainTaskStatus.Todo };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Title"));
    }

    [Test]
    public void Validate_DueDateWithLocalKind_ReturnsDueDateError()
    {
        var model = new TaskApi
        {
            Title = "Task",
            Status = DomainTaskStatus.Todo,
            DueDate = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Local)
        };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("DueDate"));
        Assert.That(results[0].ErrorMessage, Does.Contain("UTC"));
    }

    [Test]
    public void Validate_DueDateWithUnspecifiedKind_ReturnsDueDateError()
    {
        var model = new TaskApi
        {
            Title = "Task",
            Status = DomainTaskStatus.Todo,
            DueDate = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Unspecified)
        };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("DueDate"));
    }

    [Test]
    public void Validate_DueDateInThePast_ReturnsDueDateError()
    {
        var model = new TaskApi
        {
            Title = "Task",
            Status = DomainTaskStatus.Todo,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("DueDate"));
        Assert.That(results[0].ErrorMessage, Does.Contain("future"));
    }

    [Test]
    public void Validate_NullStatus_ReturnsStatusError()
    {
        var model = new TaskApi { Title = "Task", Status = null };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Status"));
        Assert.That(results[0].ErrorMessage, Does.Contain("required"));
    }

    [Test]
    public void Validate_InvalidStatusValue_ReturnsStatusError()
    {
        var model = new TaskApi { Title = "Task", Status = (DomainTaskStatus)99 };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Status"));
        Assert.That(results[0].ErrorMessage, Does.Contain("0").Or.Contain("1").Or.Contain("2"));
    }
}
