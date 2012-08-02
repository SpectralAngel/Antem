using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class State : Entity<int>
    {
        private string name;
        private IList<Person> people = new List<Person>();
        private IList<County> counties = new List<County>();

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual IList<County> Counties
        {
            get { return counties; }
            set { counties = value; }
        }

        public virtual IList<Person> People
        {
            get { return people; }
            set { people = value; }
        }
    }
}
