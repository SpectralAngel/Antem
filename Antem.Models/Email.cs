using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antem.Models
{
    public class Email : Entity<int>
    {
        private string type;
        private string address;
        private Person person;

        public virtual string Type
        {
            get { return type; }
            set { type = value; }
        }

        public virtual string Address
        {
            get { return address; }
            set { address = value; }
        }

        public virtual Person Person
        {
            get { return person; }
            set { person = value; }
        }
    }
}
