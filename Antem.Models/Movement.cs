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
        private Account account;
        private Users user;
        private string type;

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

        public virtual Account Account
        {
            get { return account; }
            set { account = value; }
        }

        public Users User
        {
            get { return user; }
            set { user = value; }
        }

        public virtual string Type
        {
            get { return type; }
            set { type = value; }
        }

        private bool invoiceCreated;

        public virtual bool InvoiceCreated
        {
            get { return invoiceCreated; }
            set { invoiceCreated = value; }
        }
    }
}
