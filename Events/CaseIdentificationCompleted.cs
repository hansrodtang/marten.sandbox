namespace MartenTest.Events;

public record CaseIdentificationCompleted
{
    public Guid CustomerId { get; set; }

    public Guid CaseId { get; set; }
}
