using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antem.Models
{
    public class User : Entity<int>
    {
        private string username;
        private Branch branch;
        private IList<Role> roles = new List<Role>();
        private IList<Membership> memberships = new List<Membership>();
        
        public virtual Branch Branch
        {
            get { return branch; }
            set { branch = value; }
        }

        public virtual string Username
        {
            get { return username; }
            set { username = value; }
        }

        public virtual IList<Role> Roles
        {
            get { return roles; }
            set { roles = value; }
        }

        public virtual IList<Membership> Memberships
        {
            get { return memberships; }
            set { memberships = value; }
        }

        public virtual void AddMembership(Membership membership)
        {
            membership.User = this;
            memberships.Add(membership);
        }

        public virtual void AddRole(Role role)
        {
            role.UsersInRole.Add(this);
            Roles.Add(role);
        }

        public virtual void RemoveRole(Role role)
        {
            role.UsersInRole.Remove(this);
            Roles.Remove(role);
        }
    }
}
