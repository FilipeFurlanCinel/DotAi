namespace Domain._Common;

public interface IHasTimeStamps
{
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public void SetCreatedAtNow();
    public void SetUpdatedAt();
}