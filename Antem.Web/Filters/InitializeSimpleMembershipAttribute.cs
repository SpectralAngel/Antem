using System;
using System.Data.Entity;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using Antem.Web.Models;
using Antem.Models;
using Antem.Parts;
using System.Composition;

namespace Antem.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        [Import]
        private IRepository<Antem.Models.Membership> membershipRepository { get; set; }

        [Import]
        private IRepository<OAuthMembership> oAuthMembershipRepository { get; set; }

        [Import]
        private IRepository<OauthToken> oAuthTokenRepository { get; set; }

        [Import]
        private ExportFactory<IUnitOfWork> unitOfWork { get; set; }

        [Import]
        private IRepository<User> userRepository { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                try
                {
                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: false);
                    // WebSecurity.
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }
            }
        }
    }
}
