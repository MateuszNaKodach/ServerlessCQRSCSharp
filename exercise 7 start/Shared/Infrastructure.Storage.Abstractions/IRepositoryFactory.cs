using System;

namespace Infrastructure.Storage.Abstractions
{
    public interface IRepositoryFactory
    {
        IRepository<TEntity> Create<TEntity>() where TEntity : class, new();
        object Create(Type entityType);
    }
}