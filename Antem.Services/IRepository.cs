using System.Linq;

namespace Antem.Parts
{
    /// <summary>
    /// Allows strongly typed abstraction of storage ability of clases,
    /// Including querying by using LINQ.
    /// </summary>
    /// <typeparam name="T">The type to be stored and retrieved</typeparam>
    public interface IRepository<T> : IQueryable<T> where T : class
    {
        /// <summary>
        /// Adds a new object to the storage
        /// </summary>
        /// <param name="Entity">The object to store</param>
        void Save(T Entity);

        /// <summary>
        /// Obtains an Entity from the storage
        /// </summary>
        /// <param name="id">The unique id of the storage</param>
        /// <returns>An strongly typed Entity that matches the id</returns>
        T Get(int id);

        /// <summary>
        /// Removes an Entity from the storage
        /// </summary>
        /// <param name="Entity"></param>
        void Delete(T Entity);

        void Update(T Entity);

        /// <summary>
        /// Convinience method for storing other types of objects
        /// </summary>
        /// <param name="value">object to store</param>
        void Save(object value);

        /// <summary>
        /// Convinience method for removing other types of objects from storage
        /// </summary>
        /// <param name="value">The object to remove</param>
        void Delete(object value);
    }
}
