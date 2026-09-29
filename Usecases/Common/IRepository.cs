using Usecases.Common.Specifications;

namespace Usecases.Common;

public interface IRepository<T>
{
    public abstract Task<IReadOnlyList<T>> GetAllAsync();
    public abstract Task<T?> GetByIdAsync(Guid id);
    public abstract Task<T> AddAsync(T entity);
    public abstract Task<T> UpdateAsync(T entity);
    public abstract Task<T> DeleteAsync(T entity);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
    Task<T?> GetBySpecAsync(ISpecification<T> spec);
}