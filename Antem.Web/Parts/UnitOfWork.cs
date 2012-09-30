using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace Antem.Parts
{
    /// <summary>
    /// Implementacion de <see cref="IUnitOfWork"/> que utiliza NHibernate
    /// </summary>
    public sealed class UnitOfWork : IUnitOfWork
    {
        ITransaction transaction;

        public UnitOfWork(IProvider dataProvider)
        {
            var provider = dataProvider as Provider;
            if (provider == null)
                throw new Exception("Invalid Data Provider");
            transaction = provider.Session.BeginTransaction();
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void Rollback()
        {
            transaction.Rollback();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~UnitOfWork()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (transaction != null)
                {
                    transaction.Dispose();
                    transaction = null;
                }
            }
        }
    }
}

