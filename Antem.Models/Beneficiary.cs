using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class Beneficiary : Entity<int>
    {
        private Affiliate affiliate;
        private string name;
        private double percent;

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual double Percent
        {
            get { return percent; }
            set { percent = value; }
        }

        public virtual Affiliate Affiliate
        {
            get { return affiliate; }
            set { affiliate = value; }
        }
    }
}
