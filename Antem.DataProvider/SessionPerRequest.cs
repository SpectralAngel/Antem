using System;
using System.Web;

using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Tool.hbm2ddl;

namespace Zangetsu.NHibernateProvider
{
    /// <summary>
    /// http://www.bengtbe.com/blog/2009/10/08/nerddinner-with-fluent-nhibernate-part-3-the-infrastructure
    /// </summary>
    public class SessionPerRequest : IHttpModule
    {
        private static readonly ISessionFactory _sessionFactory;

        // Constructs our HTTP module
        static SessionPerRequest()
        {
            _sessionFactory = CreateSessionFactory();
        }

        // Initializes the HTTP module
        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequest;
            context.EndRequest += EndRequest;
        }

        // Disposes the HTTP module
        public void Dispose() { }

        // Returns the current session
        public static ISession GetCurrentSession()
        {
            return _sessionFactory.GetCurrentSession();
        }

        // Opens the session, begins the transaction, and binds the session
        private static void BeginRequest(object sender, EventArgs e)
        {
            ISession session = _sessionFactory.OpenSession();

            session.BeginTransaction();

            CurrentSessionContext.Bind(session);
        }

        // Unbinds the session, commits the transaction, and closes the session
        private static void EndRequest(object sender, EventArgs e)
        {
            ISession session = CurrentSessionContext.Unbind(_sessionFactory);

            if (session == null) return;

            try
            {
                session.Transaction.Commit();
            }
            catch (Exception)
            {
                session.Transaction.Rollback();
            }
            finally
            {
                session.Close();
                session.Dispose();
            }
        }

        private static ISessionFactory CreateSessionFactory()
        {
            AutoPersistenceModel model = CreateMappings();
            return Fluently.Configure()
                .Database(PostgreSQLConfiguration.PostgreSQL82
                    .ConnectionString(c => c
                        .FromConnectionStringWithKey("testConn")))
                .Mappings(m => m.AutoMappings.Add(model))
                .ExposeConfiguration(UpdateSchema)
                .BuildSessionFactory();
        }

        private static void BuildSchema(Configuration config)
        {
            new SchemaExport(config).Create(false, true);
        }

        private static void UpdateSchema(Configuration config)
        {
            new SchemaUpdate(config).Execute(false, true);
        }

        private static AutoPersistenceModel CreateMappings()
        {
            return AutoMap
                .AssemblyOf<Zangetsu.Models.Persona>()
                .Conventions.AddFromAssemblyOf<Conventions>()
                .Where(t => t.Namespace == "Zangetsu.Models");
        }
    }
}
