using System.Collections.Generic;

namespace Antem.Models
{
    public class Town : Entity<int>
    {
        private string name;
        private State state;
        private IList<Person> people = new List<Person>();

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual State State
        {
            get { return state; }
            set { state = value; }
        }

        public virtual IList<Person> People
        {
            get { return people; }
            set { people = value; }
        }
    }
}
