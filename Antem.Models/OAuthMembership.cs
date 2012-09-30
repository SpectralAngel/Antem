using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antem.Models
{
    public class OAuthMembership : Entity<int>
    {
        string provider;
        string providerUserId;
        User user;

        public virtual string Provider
        {
            get { return provider; }
            set { provider = value; }
        }

        public virtual string ProviderUserId
        {
            get { return providerUserId; }
            set { providerUserId = value; }
        }

        public virtual User User
        {
            get { return user; }
            set { user = value; }
        }
    }
}
