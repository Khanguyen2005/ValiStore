using System;
using ValiModern.Models.EF;

namespace ValiModern.Services
{
    public abstract class BaseService : IDisposable
    {
        protected ValiModernDBEntities _db;
        private bool _disposed = false;

        protected BaseService()
        {
            _db = new ValiModernDBEntities();
        }

        protected BaseService(ValiModernDBEntities db)
        {
            _db = db;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
