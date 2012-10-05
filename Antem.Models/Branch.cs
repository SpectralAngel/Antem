using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antem.Models
{
    public class Branch : Entity<int>
    {
        string name;

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
