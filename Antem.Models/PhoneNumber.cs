using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antem.Models
{
    public class PhoneNumber : Entity<int>
    {
        private string type;
        private string number;
        private Person person;

        public virtual Person Person
        {
            get { return person; }
            set { person = value; }
        }

        public virtual string Type
        {
            get { return type; }
            set { type = value; }
        }

        public virtual string Number
        {
            get { return number; }
            set { number = value; }
        }
    }
}
