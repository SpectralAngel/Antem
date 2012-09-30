using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antem.Models.Exceptions;
using Antem.Parts;

namespace Antem.Models
{
    /// <summary>
    /// Represents the basic stuff a savings account must have
    /// </summary>
    public abstract class SavingAccount : Entity<int>
    {
        private Affiliate affiliate;
        private int number;
        private double rate;
        private decimal balance = 0;
        private DateTime opened = DateTime.UtcNow;
        private DateTime lastCapitalization = DateTime.UtcNow;

        /// <summary>
        /// The <see cref="Affiliate"/> that holds the <see cref="SavingAccount"/>
        /// </summary>
        public virtual Affiliate Affiliate
        {
            get { return affiliate; }
            set { affiliate = value; }
        }

        /// <summary>
        /// The code that is printed in the holder's card
        /// </summary>
        public virtual int Number
        {
            get { return number; }
            set { number = value; }
        }

        /// <summary>
        /// Represents the amount of money that is left in the <see cref="SavingAccount"/>
        /// </summary>
        public virtual decimal Balance
        {
            get { return balance; }
        }

        /// <summary>
        /// Represents the moment the <see cref="SavingAccount"/> is first created
        /// </summary>
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
        public virtual Movement Capitalize(User user)
        {
            var timespan = DateTime.UtcNow - LastCapitalization;
            if (timespan.Days >= 30)
            {
                var interest = Financial.MonthInterest(Rate, Balance);
                balance += interest;
                LastCapitalization = DateTime.UtcNow;

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

        /// <summary>
        /// Adds an amount of money to the <see cref="SavingAccount"/>'s Balance
        /// </summary>
        /// <param name="amount">The amount of money that will be added to the
        /// <see cref="SavingAccount"/></param>
        /// <returns></returns>
        public virtual Movement Deposit(decimal amount, User user)
        {
            balance += amount;
            var movement = new Movement()
            {
                Account = this,
                User = user,
                Day = LastCapitalization,
                Amount = amount,
                Type = "DEP",
                InvoiceCreated = false
            };
            return movement;
        }

        /// <summary>
        /// Tries to take out a certain amount of money out of the <see cref="SavingAccount"/>
        /// </summary>
        /// <param name="amount">The amount of money that is trying to be withdrawn</param>
        /// <returns></returns>
        public virtual Movement Withdraw(decimal amount, User user)
        {
            if (!CanWithdraw())
            {
                throw new CannotWithdrawException();
            }
            balance -= amount;
            var movement = new Movement()
            {
                Account = this,
                User = user,
                Day = LastCapitalization,
                Amount = amount,
                Type = "RET",
                InvoiceCreated = false
            };
            return movement;
        }
    }
}
