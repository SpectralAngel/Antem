using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
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

        public virtual Product Concepto
        {
            get { return product; }
            set { product = value; }
        }

        public virtual Invoice Comprobante
        {
            get { return invoice; }
            set { invoice = value; }
        }
    }
}
