using System.Collections.Generic;

namespace Antem.Models
{
    public class State : Entity<int>
    {
        private string name;
        private IList<Person> people = new List<Person>();
        private IList<Town> towns = new List<Town>();

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual IList<Town> Towns
        {
            get { return towns; }
            set { towns = value; }
        }

        public virtual IList<Person> People
        {
            get { return people; }
            set { people = value; }
        }
    }
}