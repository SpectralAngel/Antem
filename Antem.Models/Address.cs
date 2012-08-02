using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class Address : Entity<int>
    {
        private string place;
        private string description;
        private County county;
        private State state;

        public virtual string Place
        {
            get { return place; }
            set { place = value; }
        }

        public virtual County County
        {
            get { return county; }
            set { county = value; }
        }

        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }

        public virtual State State
        {
            get { return state; }
            set { state = value; }
        }
    }
}
