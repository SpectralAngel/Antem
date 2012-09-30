using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using System.Composition;

namespace Antem.Parts
{
    /// <summary>
    /// Allows Querying types from the Database while hidding the session management complexity
    /// from the client
    /// </summary>
    /// <typeparam name="T">The type to be queried</typeparam>
    [Export(typeof(IRepository<>))]
    public sealed class Repository<T> : IQueryable<T>, IRepository<T> where T : class
    {
        /// <summary>
        /// The <see cref="ISession"/> that will be used to perform the Queries
        /// </summary>
        private ISession Session { get; set; }

        private Provider DataProvider { get; set; }

        public Repository(IProvider dataProvider)
        {
            DataProvider = dataProvider as Provider;
            if (dataProvider == null)
                throw new Exception("Invalid Data Provider");
            Session = DataProvider.Session;
        }

        public Type ElementType
        {
            get { return Session.Query<T>().ElementType; }
        }

        public Expression Expression
        {
            get { return Session.Query<T>().Expression; }
        }

        public IQueryProvider Provider
        {
            get { return Session.Query<T>().Provider; }
        }

        public void Add(T entity)
        {
            Session.Save(entity);
        }

        public void Save(object value)
        {
            Session.Save(value);
        }
        public void Update(T entity)
        {
            Session.Update(entity);
        }

        public T Get(int id)
        {
            return Session.Get<T>(id);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Session.Query<T>().GetEnumerator();
        }

        public void Delete(T entity)
        {
            Session.Delete(entity);
        }

        public void Delete(object value)
        {
            Session.Delete(value);
        }

        public ICriteria CreateCriteria()
        {
            return Session.CreateCriteria<T>();
        }
    }
}
