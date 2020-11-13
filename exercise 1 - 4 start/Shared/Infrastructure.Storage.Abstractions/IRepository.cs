using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Storage.Abstractions
{
    public interface IRepository<TEntity>
    {
        Task Save(TEntity entity, CancellationToken cancellationToken);
        Task<TEntity> Read<TQuery>(TQuery entity, CancellationToken cancellationToken);
        Task<TEntity> ReadCustomQuery(TEntity entity, string query, CancellationToken cancellationToken);
    }
}