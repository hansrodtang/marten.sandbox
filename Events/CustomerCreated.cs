namespace MartenTest.Events;

public record CustomerCreated
{
    public Guid CustomerId { get; set; }

    public string Email { get; set; }
}
