using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class Affiliate : Person
    {
        private DateTime joined;
        private Boolean isActive;
        private DateTime retirement;
        private IList<SavingAccount> accounts = new List<SavingAccount>();
        private IList<Beneficiary> beneficiaries = new List<Beneficiary>();
        private IList<Invoice> invoices;
        private IList<Loan> loans;

        public virtual IList<Loan> Loans
        {
            get { return loans; }
            set { loans = value; }
        }

        public virtual IList<Invoice> Invoices
        {
            get { return invoices; }
            set { invoices = value; }
        }

        public virtual IList<Beneficiary> Beneficiaries
        {
            get { return beneficiaries; }
            set { beneficiaries = value; }
        }

        public virtual IList<SavingAccount> Accounts
        {
            get { return accounts; }
            set { accounts = value; }
        }

        public virtual DateTime Retirement
        {
            get { return retirement; }
            set { retirement = value; }
        }

        public virtual Boolean IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public virtual DateTime Joined
        {
            get { return joined; }
            set { joined = value; }
        }
    }
}
