namespace MartenTest.Events;

public record CaseIdentificationUndone
{
    public Guid CustomerId { get; set; }

    public Guid CaseId { get; set; }
}
