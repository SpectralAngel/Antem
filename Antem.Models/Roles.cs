using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antem.Models;

namespace Antem.Models
{
    public class Roles : Entity<int>
    {
        public virtual string RoleName { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual IList<Users> UsersInRole { get; set; }

        public Roles()
        {
            UsersInRole = new List<Users>();
        }

    }
}
