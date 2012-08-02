using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    /// <summary>
    /// Permite llevar el control de cantidades monetarias que se prestan al
    /// <see cref="Affiliate"/>
    /// </summary>
    public class Loan : Entity<int>
    {
        private Affiliate affiliate;
        private decimal principal;
        private DateTime given;
        private DateTime lastPayment;
        private double rate;

        private IList<Payment> payments = new List<Payment>();
        private IList<AppliedRetention> retentions = new List<AppliedRetention>();

        public virtual Affiliate Affiliate
        {
            get { return affiliate; }
            set { affiliate = value; }
        }

        public virtual decimal Principal
        {
            get { return principal; }
            set { principal = value; }
        }

        public virtual DateTime Given
        {
            get { return given; }
            set { given = value; }
        }

        public virtual double Rate
        {
            get { return rate; }
            set { rate = value; }
        }

        public virtual DateTime LastPayment
        {
            get { return lastPayment; }
            set { lastPayment = value; }
        }

        public virtual IList<Payment> Payments
        {
            get { return payments; }
            set { payments = value; }
        }

        public IList<AppliedRetention> Retentions
        {
            get { return retentions; }
            set { retentions = value; }
        }

        public virtual decimal Balance
        {
            get
            {
                return principal - payments.Sum(x => x.Principal);
            }
        }

        public virtual decimal Net
        {
            get
            {
                return principal - retentions.Sum(x => x.Amount);
            }
        }
    }
}
