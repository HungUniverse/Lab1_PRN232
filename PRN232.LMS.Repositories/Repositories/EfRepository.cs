using Microsoft.EntityFrameworkCore;

namespace PRN232.Lab1.Repository.Repositories;

public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDBContext _context;
    private readonly DbSet<TEntity> _entities;

    public EfRepository(AppDBContext context)
    {
        _context = context;
        _entities = context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query(bool tracking = false)
    {
        return tracking ? _entities : _entities.AsNoTracking();
    }

    public ValueTask<TEntity?> FindAsync(params object[] keyValues)
    {
        return _entities.FindAsync(keyValues);
    }

    public async Task AddAsync(TEntity entity)
    {
        await _entities.AddAsync(entity);
    }

    public void Remove(TEntity entity)
    {
        _entities.Remove(entity);
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
