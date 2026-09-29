namespace Domain._Common;

public abstract class AggregateRoot<TGuid>(TGuid id) : Entity<TGuid>(id)
    where TGuid : notnull;