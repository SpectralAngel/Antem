using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public abstract class Entity<T>
    {
        private T id;

        public virtual T ID
        {
            get { return id; }
            set { id = value; }
        }
    }
}
