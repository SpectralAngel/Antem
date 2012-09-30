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
        private Country county;
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

        public virtual Country County
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
