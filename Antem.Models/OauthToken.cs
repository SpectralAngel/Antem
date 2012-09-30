using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antem.Models
{
    public class OauthToken
    {
        private string token;
        private string secret;

        public virtual string Token
        {
            get { return token; }
            set { token = value; }
        }

        public virtual string Secret
        {
            get { return secret; }
            set { secret = value; }
        }
    }
}
