using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    /// <summary>
    /// Represents a money transaction made on an <see cref="Invoice"/>
    /// </summary>
    public class Charge : Entity<int>
    {
        private Invoice invoice;
        private Product product;
        private decimal amount;

        public virtual decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public virtual Product Product
        {
            get { return product; }
            set { product = value; }
        }

        public virtual Invoice Invoice
        {
            get { return invoice; }
            set { invoice = value; }
        }
    }
}
