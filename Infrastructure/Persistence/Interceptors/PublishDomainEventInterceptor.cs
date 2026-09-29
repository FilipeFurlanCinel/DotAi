using Domain._Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

// Overrides the SaveChanges and SaveChangesAsync methods to capture and execute the published domain events
public class PublishDomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _mediator;

    public PublishDomainEventInterceptor(IPublisher mediator)
    {
        _mediator = mediator;
    }

    // Publishes domain events before saving changes to the database
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        PublishDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    // Publishes domain events before async save to database
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken()
    )
    {
        await PublishDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PublishDomainEvents(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var entitiesWithDomainEvents = dbContext
            .ChangeTracker.Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Any()).ToList()
            .Select(entry => entry.Entity)
            .ToList();

        var domainEvents = entitiesWithDomainEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        foreach (IHasDomainEvents entity in entitiesWithDomainEvents)
        {
            entity.ClearDomainEvents();
        }

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);   
        }
    }
}