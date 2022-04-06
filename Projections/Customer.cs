namespace MartenTest.Projections;

public class Customer
{
    public Guid Id { get; set; }

    public List<Email> Emails { get; set; } = new();

    public List<Guid> Cases { get; set; } = new();
}
