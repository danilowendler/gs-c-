using Refresca.Api.Application.Interfaces;
using Refresca.Api.Infrastructure.Persistence;

namespace Refresca.Api.Infrastructure.Repositories;

public sealed class UnitOfWork(RefrescaDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
