using Infrastructure.Storage.Abstractions;
using Infrastructure.Storage.Sqlite;
using Microsoft.Data.Sqlite;
using System;

namespace Infrastructure.Sqlite.Storage
{
    public class SQLRepositoryFactory : IRepositoryFactory
    {
        private readonly SqliteConnection _databaseConnection;

        public SQLRepositoryFactory(SqliteConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public IRepository<TEntity> Create<TEntity>() where TEntity : class, new()
        {
            return new SQLRepository<TEntity>(_databaseConnection);
        }

        public object Create(Type entityType)
        {
            var methodInfo = GetType().GetMethod("Create");
            var typedMethod = methodInfo.MakeGenericMethod(entityType);
            return typedMethod.Invoke(this, new object[] { });
        }
    }
}
