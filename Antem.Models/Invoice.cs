using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    /// <summary>
    /// Documents the diverse Cashier Movements
    /// </summary>
    public class Invoice : Entity<int>
    {
        private Affiliate affiliate;
        private DateTime day;
        private IList<Charge> cargos = new List<Charge>();
        private User cashier;

        public virtual User Cashier
        {
            get { return cashier; }
            set { cashier = value; }
        }

        public virtual Affiliate Affiliate
        {
            get { return affiliate; }
            set { affiliate = value; }
        }

        public virtual DateTime Day
        {
            get { return day; }
            set { day = value; }
        }

        public virtual IList<Charge> Charges
        {
            get { return cargos; }
            set { cargos = value; }
        }

        /// <summary>
        /// Calculates the total of the Invoice
        /// </summary>
        public virtual decimal Total
        {
            get
            {
                return cargos.Sum(x => x.Amount);
            }
        }
    }
}
