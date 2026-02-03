using System;
using System.Data;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDbTransaction BeginTransaction();
        void Commit();
        void Rollback();
    }
}
