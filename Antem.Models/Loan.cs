using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antem.Parts;

namespace Antem.Models
{
    /// <summary>
    /// Allows management of amounts of money that are loaned to
    /// <see cref="Affiliate"/>s
    /// </summary>
    public class Loan : Entity<int>
    {
        private Affiliate affiliate;
        private decimal principal;
        private decimal cuota;
        private DateTime given;
        private DateTime lastPayment;
        private int length;
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

        public virtual decimal Cuota
        {
            get { return cuota; }
            set { cuota = value; }
        }

        public virtual DateTime Given
        {
            get { return given; }
            set { given = value; }
        }

        public virtual int Length
        {
            get { return length; }
            set { length = value; }
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

        public virtual IList<AppliedRetention> Retentions
        {
            get { return retentions; }
            set { retentions = value; }
        }

        public virtual decimal Balance
        {
            get
            {
                return principal - PrincipalPayed;
            }
        }

        public virtual decimal TotalRetention
        {
            get
            {
                return retentions.Sum(x => x.Amount);
            }
        }

        public virtual decimal PrincipalPayed
        {
            get
            {
                return payments.Sum(x => x.Principal);
            }
        }

        public virtual decimal InterestPayed
        {
            get
            {
                return payments.Sum(x => x.Interest);
            }
        }

        public virtual decimal TotalPayed
        {
            get
            {
                return payments.Sum(x => x.Amount);
            }
        }

        public virtual decimal Net
        {
            get
            {
                return principal - TotalRetention;
            }
        }

        /// <summary>
        /// Creates a <see cref="Payment"/> that can be applied to this <see cref="Loan"/>
        /// </summary>
        /// <param name="amount">The brute amount of money that will be applied as a payment</param>
        /// <param name="day">The day the payment will be applied</param>
        /// <param name="chargeInterest">Whether interest calculations will be taken into consideration</param>
        /// <returns>A complete <see cref="Payment"/> that can be applied as the next payment</returns>
        public virtual Payment CreatePayment(decimal amount, DateTime day, bool chargeInterest)
        {
            var interest = 0M;
            if(chargeInterest)
            {
                interest = Financial.MonthInterest(rate, Balance);
            }
            var payedPrincipal = amount - interest;
            var payment = new Payment()
            {
                Interest = interest,
                Principal = payedPrincipal,
                Loan = this,
                Day = day,
                Amount = amount
            };
            return payment;
        }

        /// <summary>
        /// Creates a <see cref="Payment"/> for usage in the schedule
        /// </summary>
        /// <param name="amount">The brute amount of money that will be applied as a payment</param>
        /// <param name="day">The day the payment will be applied</param>
        /// <param name="balance">the balance that will be used</param>
        /// <returns></returns>
        public virtual Payment CreateFuturePayment(decimal amount, DateTime day, decimal balance)
        {
            var interest = Financial.MonthInterest(rate, balance);
            var payedPrincipal = amount - interest;
            var payment = new Payment()
            {
                Interest = interest,
                Principal = payedPrincipal,
                Day = day,
                Loan = this,
                Amount = amount
            };
            return payment;
        }

        /// <summary>
        /// Allows the calculation of the <see cref="Payment"/> schedule that
        /// is needed to pay the <see cref="Loan"/> on time without extra
        /// interests
        /// </summary>
        /// <returns></returns>
        public virtual SortedDictionary<decimal, Payment> PaymentSchedule()
        {
            var scheduledPayments = new SortedDictionary<decimal, Payment>();
            var futureBalance = Balance;
            var payday = lastPayment;
            while (futureBalance > 0)
            {
                payday = payday.AddMonths(1);
                var payment = CreateFuturePayment(Cuota, payday, futureBalance);
                futureBalance -= payment.Principal;
                scheduledPayments.Add(futureBalance, payment);
            }
            return scheduledPayments;
        }
    }
}
