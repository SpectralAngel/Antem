using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antem.Services;

namespace Antem.Models
{
    public abstract class Account : Entity<int>
    {
        private Affiliate affiliate;
        private int number;
        private decimal balance;
        private DateTime opened;
        private double rate;
        private DateTime lastCapitalization;

        public virtual Affiliate Affiliate
        {
            get { return affiliate; }
            set { affiliate = value; }
        }

        public virtual int Number
        {
            get { return number; }
            set { number = value; }
        }

        public virtual decimal Balance
        {
            get { return balance; }
            set { balance = value; }
        }

        public virtual DateTime Opened
        {
            get { return opened; }
            set { opened = value; }
        }

        public virtual double Rate
        {
            get { return rate; }
            set { rate = value; }
        }

        public virtual DateTime LastCapitalization
        {
            get { return lastCapitalization; }
            set { lastCapitalization = value; }
        }

        public abstract bool CanWithdraw();

        /// <summary>
        /// Calculates the interest that will go into the account
        /// </summary>
        public Movement Capitalize(Users user)
        {
            var timespan = DateTime.Now - LastCapitalization;
            if (timespan.Days >= 30)
            {
                var interest = Financial.InteresMensual(Rate, Balance);
                Balance += interest;
                LastCapitalization = DateTime.Now;

                var movement = new Movement()
                {
                    Account = this,
                    User = user,
                    Day = LastCapitalization,
                    Amount = interest,
                    Type = "INT",
                    InvoiceCreated = true
                };
                return movement;
            }
            return new Movement() { Amount = 0 };
        }

        public Movement Deposit(decimal amount, Users user)
        {
            Balance += amount;
            var movement = new Movement()
            {
                Account = this,
                User = user,
                Day = DateTime.Now,
                Amount = amount,
                Type = "DEP",
                InvoiceCreated = false
            };
            return movement;
        }

        public Movement Retirar(decimal monto, string ejecutor, Users user)
        {
            Balance -= monto;
            var movement = new Movement()
            {
                Account = this,
                User = user,
                Day = DateTime.Now,
                Amount = monto,
                Type = "RET",
                InvoiceCreated = false
            };
            return movement;
        }
    }
}
