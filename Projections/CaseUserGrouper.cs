namespace MartenTest.Projections;

using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using MartenTest.Events;

public class CaseUserGrouper : IAggregateGrouper<Guid>
{
    public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping)
    {
        var caseAddedEvents = events
          .OfType<IEvent<CaseIdentificationCompleted>>()
          .ToList();

        if (!caseAddedEvents.Any())
        {
            return;
        }

        foreach (var x in caseAddedEvents)
        {
            var caseEvents = await session.Events.FetchStreamAsync(x.Data.CaseId);
            if (!caseEvents.Any())
            {
                continue;
            }

            grouping.AddEvents<CaseCreated>((e) => x.Data.CustomerId, caseEvents);
        }
    }
}