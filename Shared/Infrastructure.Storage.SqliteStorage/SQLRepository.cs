using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Storage.Abstractions;
using Infrastructure.Storage.Abstractions.CustomAttributes;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Infrastructure.Storage.Sqlite
{
    public class SQLRepository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly PropertyInfo _rowKeyProp;
        private readonly SqliteConnection _databaseConnection;

        public SQLRepository(SqliteConnection databaseConnection)
        {
            _rowKeyProp = typeof(TEntity).GetProperties().First(x => Attribute.IsDefined(x, typeof(RowKeyAttribute)));
            _databaseConnection = databaseConnection;
        }

        public async Task Save(TEntity entity, CancellationToken cancellationToken)
        {
            var entityName = typeof(TEntity).Name;
            var tableExists = await TableExists(entityName);
            if (!tableExists)
            {
                await CreateTable(entityName);
            }

           await UpsertEntity(entity);
        }
        public async Task<TEntity> Read<TQuery>(TQuery query, CancellationToken cancellationToken)
        {
            var tableName = typeof(TEntity).Name;
            var selectStmt = "select ";
            var fromTable = " from " + tableName;
            var columnList = "";
            var whereClause = "";
            foreach (var prop in typeof(TEntity).GetProperties())
            {
                columnList += !string.IsNullOrWhiteSpace(columnList) ? ", " + prop.Name : prop.Name;
            }
            foreach(var prop in typeof(TQuery).GetProperties())
            {
                if (prop.GetValue(query) != null)
                {
                    whereClause += !string.IsNullOrWhiteSpace(whereClause) ? " AND " : " where ";
                    whereClause += $"{ prop.Name} = '{prop.GetValue(query)}'";
                }
            }
            selectStmt += columnList + fromTable + whereClause;
            var command = _databaseConnection.CreateCommand();
            command.CommandText = selectStmt;
            var reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
                return null;

            List<string> columnNames = new List<string>(columnList.Trim().Split(','));
            var returnEntity = new TEntity();
            while (reader.Read())
            {
                for (var columnIndex = 0; columnIndex < columnNames.Count; columnIndex++)
                {
                    var columnName = columnNames[columnIndex]?.Trim();
                    var returnProp = returnEntity.GetType().GetProperty(columnName);
                    var propertyType = returnProp.PropertyType;
                    var propTypeCode = System.Type.GetTypeCode(propertyType);

                    switch (propTypeCode)
                    {
                        case TypeCode.Int16:
                            returnProp.SetValue(returnEntity, Convert.ToInt32(reader.GetValue(columnIndex)), null);
                            break;
                        case TypeCode.Int32:
                            returnProp.SetValue(returnEntity, Convert.ToInt32(reader.GetValue(columnIndex)), null);
                            break;
                        case TypeCode.Int64:
                            returnProp.SetValue(returnEntity, Convert.ToInt64(reader.GetValue(columnIndex)), null);
                            break;
                        case TypeCode.String:
                            returnProp.SetValue(returnEntity, reader.GetValue(columnIndex), null);
                            break;
                        case TypeCode.Object:
                            if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                            {
                                returnProp.SetValue(returnEntity, Guid.Parse(reader.GetString(columnIndex)), null);
                            }
                            else
                            {
                                returnProp.SetValue(returnEntity, reader.GetValue(columnIndex), null);
                            }
                            break;
                        default:
                            returnProp.SetValue(returnEntity, reader.GetValue(columnIndex), null);
                            break;
                    }
                }
            }

            return returnEntity;
        }

        public Task<TEntity> ReadCustomQuery(TEntity entity, string query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        private async Task<bool> TableExists(string tableName)
        {

            var sql_stmt = "SELECT name from sqlite_master WHERE type ='table' AND name = '" + tableName + "'";
            var command = _databaseConnection.CreateCommand();
            command.CommandText = sql_stmt;
            var rdr = await command.ExecuteReaderAsync();
            if (rdr.Read()) return true;

            return false;
        }

        private async Task CreateTable(string tableName)
        {
            string createStmt = $"create table {tableName} (";
            foreach (var prop in typeof(TEntity).GetProperties())
            {
                createStmt = createStmt + prop.Name + " TEXT ";
                if (prop.Name == _rowKeyProp.Name)
                {
                    createStmt += "PRIMARY KEY ";
                }
                createStmt += ", ";
            }
            createStmt = createStmt.Substring(0, createStmt.Length - 2) +")";
            var cmd = _databaseConnection.CreateCommand();
            cmd.CommandText = createStmt;
            await cmd.ExecuteNonQueryAsync();

        }

        private string CreateInsertStatement(TEntity entity)
        {
            var entityName = typeof(TEntity).Name;
            string sql_insert_stmt = $"insert into {entityName} (";
            string values = "";
            foreach (var prop in typeof(TEntity).GetProperties())
            {
                sql_insert_stmt += prop.Name + ", ";
                values += "'" + prop.GetValue(entity) + "', ";
            }
            sql_insert_stmt = sql_insert_stmt.Substring(0, sql_insert_stmt.Length - 2) + ") VALUES (" +
                values.Substring(0, values.Length - 2) + ")";

            return sql_insert_stmt;
        }

        private string CreateUpdateStatement(TEntity entity)
        {
            var entityName = typeof(TEntity).Name;
            string sql_update_statement = $"update {entityName} set ";
            foreach (var prop in typeof(TEntity).GetProperties())
            {
                sql_update_statement += prop.Name + "= '" + prop.GetValue(entity) + "', ";
            }
            sql_update_statement = sql_update_statement.Substring(0, sql_update_statement.Length-2) + $" where {_rowKeyProp.Name} = '" + _rowKeyProp.GetValue(entity) + "'";

            return sql_update_statement;
        }

        private async Task<int> UpsertEntity(TEntity entity)
        {
            string insert_stmt = CreateInsertStatement(entity);
            string update_stmt = CreateUpdateStatement(entity);

            var cmd = _databaseConnection.CreateCommand();
            cmd.CommandText = update_stmt;
            var rows = await cmd.ExecuteNonQueryAsync();
            if (rows == 0)
            {
                cmd.CommandText = insert_stmt;
                rows = await cmd.ExecuteNonQueryAsync();
            }

            return rows;
        }
    }
}
