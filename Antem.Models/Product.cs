﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class Product : Entity<int>
    {
        private string name;
        private IList<Charge> charges = new List<Charge>();
        private bool isOutcome;

        public virtual IList<Charge> Charges
        {
            get { return charges; }
            set { charges = value; }
        }

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual bool IsOutcome
        {
            get { return isOutcome; }
            set { isOutcome = value; }
        }
    }
}
