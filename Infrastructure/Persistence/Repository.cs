using Domain._Common;
using Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Usecases.Common;
using Usecases.Common.Specifications;

namespace Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : Entity<Guid>
{
    private readonly AppDbContext _dbContext;

    public Repository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
        return entity;
    }

    public Task<T> DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        return Task.FromResult(entity);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public async Task<T?> GetBySpecAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    public Task<T> UpdateAsync(T entity)
    {
        var local = _dbContext.Set<T>().Local.FirstOrDefault(e => e.Id.Equals(entity.Id));
        if (local is not null)
            _dbContext.Entry(local).State = EntityState.Detached;

        _dbContext.Set<T>().Update(entity);
        return Task.FromResult(entity);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>().AsQueryable(), spec);
    }
}
