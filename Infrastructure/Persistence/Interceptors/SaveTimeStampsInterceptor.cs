using Domain._Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

public class SaveTimeStampsInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyTimestamps(DbContext? dbContext)
    {
        if (dbContext is null)
            return;

        var entries = dbContext.ChangeTracker.Entries()
            .Where(e => e.Entity is IHasTimeStamps &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (IHasTimeStamps)entry.Entity;
            if (entry.State == EntityState.Added)
                entity.SetCreatedAtNow();

            entity.SetUpdatedAt();
        }
    }
}
