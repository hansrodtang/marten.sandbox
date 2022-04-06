namespace MartenTest.Events;

public record CaseCreated
{
    public Guid CaseId { get; set; }

    public string Email { get; set; }
}
