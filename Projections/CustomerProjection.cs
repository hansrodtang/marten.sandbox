namespace MartenTest.Projections;

using Marten.Events.Projections;
using MartenTest.Events;

public class CustomerProjection : ViewProjection<Customer, Guid>
{
    public CustomerProjection()
    {
        Identity<CustomerCreated>(@event => @event.CustomerId);
        Identity<CaseIdentificationUndone>(@event => @event.CustomerId);
        Identity<CaseIdentificationCompleted>(@event => @event.CustomerId);

        CustomGrouping(new CaseUserGrouper());
    }

    public Customer Create(CustomerCreated e)
    {
        return new() { Id = e.CustomerId, Emails = new() { new() { Content = e.Email, Source = e.CustomerId } } };
    }
    public void Apply(CaseIdentificationCompleted e, Customer view)
    {
        view.Cases.Add(e.CaseId);
    }

    public void Apply(CaseIdentificationUndone e, Customer view)
    {
        view.Cases.Remove(e.CaseId);
        view.Emails = view.Emails.Where(x => x.Source != e.CaseId).ToList();
    }

    public void Apply(CaseCreated e, Customer view)
    {
        view.Emails.Add(new() { Content = e.Email, Source = e.CaseId });
    }
}
