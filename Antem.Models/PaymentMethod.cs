using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class PaymentMethod : Entity<int>
    {
        private string name;
        private IList<Member> members;
        private IList<Invoice> invoices;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public IList<Invoice> Invoices
        {
            get { return invoices; }
            set { invoices = value; }
        }

        public IList<Member> Members
        {
            get { return members; }
            set { members = value; }
        }
    }
}
