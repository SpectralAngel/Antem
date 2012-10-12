using System;

namespace Antem.Parts
{
    /// <summary>
    /// Defines an interface for atomicity and isolating storage operations
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Tells the storage that the current operation completed succesfully
        /// </summary>
        void Commit();

        /// <summary>
        /// Tells the storage that the operation had errors on it and that it must not
        /// be persisted
        /// </summary>
        void Rollback();

        bool isActive { get; }
    }
}
