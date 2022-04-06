using Marten;
using Marten.CommandLine;
using Marten.Events.Daemon.Resiliency;
using Oakton;
using Microsoft.AspNetCore.Mvc;
using Marten.Services.Json;
using MartenTest.Events;
using MartenTest.Projections;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(opts =>
{
    opts.Connection("host = localhost; database = hans.rodtang; username = hans.rodtang");
    opts.UseDefaultSerialization(serializerType: SerializerType.SystemTextJson);
    opts.Projections.Add(new CustomerProjection());
    opts.Projections.Add(new CaseProjection());
})
.AddAsyncDaemon(DaemonMode.Solo)
.OptimizeArtifactWorkflow();

builder.Services
.AddEndpointsApiExplorer()
.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();

app.MapPost("/customer", async (CreateCommand customer, IDocumentStore store) =>
{
    var customerid = Guid.NewGuid();
    using var session = store.OpenSession();
    session.Events.StartStream<Customer>(customerid, new CustomerCreated { Email = customer.Email, CustomerId = customerid });
    await session.SaveChangesAsync();

    return Results.Created($"/customer/{customerid}", await session.LoadAsync<Customer>(customerid));
});

app.MapPost("/case/{id}/customer", async (Guid id, [FromBody] Guid customerid, IDocumentStore store) =>
{
    using var session = store.OpenSession();
    session.Events.Append(id, new CaseIdentificationCompleted { CaseId = id, CustomerId = customerid });
    await session.SaveChangesAsync();

    return Results.Accepted();
});

app.MapDelete("/case/{id}/customer", async (Guid id, IDocumentStore store) =>
{
    using var session = store.OpenSession();

    var c = await session.LoadAsync<CollectionOrder>(id);
    if (c == null)
    {
        return Results.BadRequest();
    }

    session.Events.Append(id, new CaseIdentificationUndone { CaseId = id, CustomerId = c.Debtor.Id });
    await session.SaveChangesAsync();

    return Results.Accepted();
});

app.MapPost("/case", async (CreateCommand customer, IDocumentStore store) =>
{
    var caseid = Guid.NewGuid();
    using var session = store.OpenSession();
    session.Events.StartStream<CollectionOrder>(caseid, new CaseCreated { Email = customer.Email, CaseId = caseid });
    await session.SaveChangesAsync();

    return Results.Created($"/case/{caseid}", await session.LoadAsync<CollectionOrder>(caseid));
});

app.MapDelete("/case/{id}", async (Guid id, IDocumentSession session) =>
{
    session.Events.ArchiveStream(id);
    await session.SaveChangesAsync();
});

app.MapGet("/rebuild", async (IDocumentStore store) =>
{
    var daemon = await store.BuildProjectionDaemonAsync();
    await daemon.RebuildProjection<CollectionOrder>(CancellationToken.None);
    await daemon.RebuildProjection<Customer>(CancellationToken.None);
});

app.MapGet("/customer/{id}", async (Guid id, IDocumentStore store) =>
{
    using var query = store.LightweightSession();
    return await query.LoadAsync<Customer>(id);
});

app.MapGet("/case/{id}", async (Guid id, IDocumentStore store) =>
{
    using var query = store.LightweightSession();
    return await query.LoadAsync<CollectionOrder>(id);
});

app.UseSwaggerUI();

await app.RunOaktonCommands(args);

public record CreateCommand
{
    public string Email { get; init; }
}