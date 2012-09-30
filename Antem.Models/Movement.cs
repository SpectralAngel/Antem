using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class Movement : Entity<int>
    {
        private decimal amount;
        private DateTime day;
        private SavingAccount account;
        private User user;
        private string type;
        private bool invoiceCreated;

        public virtual decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public virtual DateTime Day
        {
            get { return day; }
            set { day = value; }
        }

        public virtual SavingAccount Account
        {
            get { return account; }
            set { account = value; }
        }

        public virtual User User
        {
            get { return user; }
            set { user = value; }
        }

        public virtual string Type
        {
            get { return type; }
            set { type = value; }
        }

        public virtual bool InvoiceCreated
        {
            get { return invoiceCreated; }
            set { invoiceCreated = value; }
        }
    }
}
