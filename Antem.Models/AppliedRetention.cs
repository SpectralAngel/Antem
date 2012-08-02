using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class AppliedRetention : Entity<int>
    {
        private Retention retention;
        private bool isApplied;
        private decimal amount;
        private Loan loan;

        public virtual Retention Retention
        {
            get { return retention; }
            set { retention = value; }
        }

        public virtual bool IsApplied
        {
            get { return isApplied; }
            set { isApplied = value; }
        }

        public virtual decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public virtual Loan Loan
        {
            get { return loan; }
            set { loan = value; }
        }
    }
}
