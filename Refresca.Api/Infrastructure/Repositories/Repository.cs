using Microsoft.EntityFrameworkCore;
using Refresca.Api.Application.Interfaces;
using Refresca.Api.Infrastructure.Persistence;

namespace Refresca.Api.Infrastructure.Repositories;

public sealed class Repository<T>(RefrescaDbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _set.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
    public IQueryable<T> Query() => _set.AsQueryable();
}
