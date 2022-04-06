namespace MartenTest.Projections;

public class CollectionOrder
{
    public Guid Id { get; set; }
    public Customer Debtor { get; set; }
    public string Email { get; set; }
}
