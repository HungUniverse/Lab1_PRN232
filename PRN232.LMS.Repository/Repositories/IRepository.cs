namespace PRN232.Lab1.Repository.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query(bool tracking = false);
    ValueTask<TEntity?> FindAsync(params object[] keyValues);
    Task AddAsync(TEntity entity);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync();
}
