using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    /// <summary>
    /// Represents an amount applied to a <see cref="Loan"/>
    /// </summary>
    public class Payment : Entity<int>
    {
        private Loan loan;
        private DateTime day;
        private decimal amount;
        private decimal principal;
        private decimal interest;
        private bool invoiceCreated;
        private Invoice invoice;

        public Invoice Invoice
        {
            get { return invoice; }
            set { invoice = value; }
        }

        public virtual DateTime Day
        {
            get { return day; }
            set { day = value; }
        }

        public virtual decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public virtual decimal Principal
        {
            get { return principal; }
            set { principal = value; }
        }

        public virtual decimal Interest
        {
            get { return interest; }
            set { interest = value; }
        }

        public virtual Loan Loan
        {
            get { return loan; }
            set { loan = value; }
        }

        public virtual bool InvoiceCreated
        {
            get { return invoiceCreated; }
            set { invoiceCreated = value; }
        }
    }
}
