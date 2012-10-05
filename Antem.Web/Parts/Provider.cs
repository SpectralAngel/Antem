using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Antem.Parts
{
    public class Provider : IProvider
    {
        protected ISession session;

        private static ISessionFactory SessionFactory = null;

        public Provider()
        {
            if (SessionFactory == null)
            {
                SessionFactory = BuildSessionFactory();
            }
        }

        public ISession Session
        {
            get
            {
                if (session == null || !session.IsOpen)
                    session = SessionFactory.OpenSession();
                return session;
            }
            set
            {
                session = value;
            }
        }

        public virtual ISessionFactory BuildSessionFactory()
        {
            AutoPersistenceModel bussiness = Provider.CreateBussinessMappings();
            Configuration cfg = new Configuration();
            cfg.Configure();
            return Fluently.Configure(cfg)
                .Mappings(m =>
                {
                    m.AutoMappings.Add(bussiness);
                })
                .ExposeConfiguration(UpdateSchema)
                .BuildSessionFactory();
        }

        protected static AutoPersistenceModel CreateBussinessMappings()
        {
            return AutoMap
                .AssemblyOf<Antem.Models.Person>()
                .IgnoreBase(typeof(Antem.Models.Entity<>))
                .IncludeBase<Antem.Models.SavingAccount>()
                .Override<Antem.Models.Person>(map =>
                {
                    map.IgnoreProperty(x => x.Age);
                })
                .Override<Antem.Models.Loan>(map =>
                {
                    map.IgnoreProperty(x => x.Balance);
                    map.IgnoreProperty(x => x.Net);
                    map.IgnoreProperty(x => x.PrincipalPayed);
                    map.IgnoreProperty(x => x.TotalPayed);
                    map.IgnoreProperty(x => x.TotalRetention);
                    map.IgnoreProperty(x => x.InterestPayed);
                }).Override<Antem.Models.Invoice>(map =>
                {
                    map.IgnoreProperty(x => x.Total);
                })
                .Override<Antem.Models.OAuthMembership>(map =>
                {
                    map.Map(x => x.ProviderUserId).UniqueKey("provider_user");
                    map.Map(x => x.Provider).UniqueKey("provider_user");
                })
                .Override<Antem.Models.OauthToken>(map =>
                {
                    map.Id(x => x.Token).GeneratedBy.Assigned();
                })
                .Where(t => t.Namespace == "Antem.Models");
        }

        protected static void UpdateSchema(Configuration config)
        {
            new SchemaUpdate(config).Execute(false, true);
        }
    }
}
