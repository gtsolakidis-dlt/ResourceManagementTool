using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Infrastructure.Persistence
{
    public class DapperContext : IUnitOfWork
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);

        public IDbConnection GetOpenConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        public IDbTransaction BeginTransaction()
        {
            var conn = GetOpenConnection();
            _transaction = conn.BeginTransaction();
            return _transaction;
        }

        public void Commit()
        {
            _transaction?.Commit();
            DisposeConnection();
        }

        public void Rollback()
        {
            _transaction?.Rollback();
            DisposeConnection();
        }

        public void Dispose()
        {
            DisposeConnection();
            GC.SuppressFinalize(this);
        }

        private void DisposeConnection()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _transaction = null;
            _connection = null;
        }

    }
}

