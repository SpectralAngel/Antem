using System;
using System.Collections.Generic;

namespace Antem.Models
{
    public class Member : Person
    {
        private Branch branch;
        private DateTime joined = DateTime.UtcNow;
        private bool active = true;
        private DateTime retirement = DateTime.MinValue;
        private IList<SavingAccount> accounts = new List<SavingAccount>();
        private IList<Beneficiary> beneficiaries = new List<Beneficiary>();
        private IList<Invoice> invoices = new List<Invoice>();
        private IList<Loan> loans = new List<Loan>();

        public virtual Branch Branch
        {
            get { return branch; }
            set { branch = value; }
        }

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

        public virtual bool IsActive
        {
            get { return active; }
            set { active = value; }
        }

        public virtual DateTime Joined
        {
            get { return joined; }
            set { joined = value; }
        }
    }
}
