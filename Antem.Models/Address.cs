using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class Address : Entity<int>
    {
        private Affiliate affiliate;
        private string place;
        private string description;
        private Town town;
        private State state;

        public virtual Affiliate Affiliate
        {
            get { return affiliate; }
            set { affiliate = value; }
        }

        public virtual string Place
        {
            get { return place; }
            set { place = value; }
        }

        public virtual Town Town
        {
            get { return town; }
            set { town = value; }
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
