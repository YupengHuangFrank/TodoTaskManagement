using System.ComponentModel.DataAnnotations;
using DomainTaskStatus = TodoTaskManagement.Domain.Tasks.TaskStatus;

namespace TodoTaskManagement.Models;

public class TaskApi : IValidatableObject
{
    public Guid? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DomainTaskStatus? Status { get; set; }
    public bool? IsArchived { get; set; }
    public DateTime? CreatedAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
            yield return new ValidationResult("Title must not be empty.", [nameof(Title)]);

        if (DueDate.HasValue && DueDate.Value.Kind != DateTimeKind.Utc)
            yield return new ValidationResult("DueDate must be a UTC datetime (e.g. 2026-06-10T00:00:00Z).", [nameof(DueDate)]);

        if (DueDate.HasValue && DueDate.Value.Date < DateTime.UtcNow.Date)
            yield return new ValidationResult("DueDate must be today or a future date.", [nameof(DueDate)]);

        if (!Status.HasValue)
            yield return new ValidationResult("Status is required.", [nameof(Status)]);
        else if (!Enum.IsDefined(typeof(DomainTaskStatus), Status.Value))
            yield return new ValidationResult("Status must be 0 (Todo), 1 (InProgress), or 2 (Done).", [nameof(Status)]);
    }
}
