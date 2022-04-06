namespace MartenTest.Projections;

using Marten.Events.Projections;
using MartenTest.Events;

public class CaseProjection : ViewProjection<CollectionOrder, Guid>
{
    public CaseProjection()
    {
        Identity<CaseCreated>(@event => @event.CaseId);
        Identity<CaseIdentificationUndone>(@event => @event.CaseId);
        Identity<CaseIdentificationCompleted>(@event => @event.CaseId);
    }

    public CollectionOrder Create(CaseCreated e)
    {
        return new() { Id = e.CaseId, Email = e.Email };
    }

    public void Apply(CaseIdentificationCompleted e, CollectionOrder view)
    {
        view.Debtor = new() { Id = e.CustomerId };
    }

    public void Apply(CaseIdentificationUndone e, CollectionOrder view)
    {
        view.Debtor = new() { Id = Guid.Empty };
    }
}
