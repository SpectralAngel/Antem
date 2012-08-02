using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    /// <summary>
    /// Cobros que se pueden aplicar a un <see cref="Loan"/>
    /// </summary>
    public class Retention : Entity<int>
    {
        private string name;
        private bool isCapitalizable;
        private IList<AppliedRetention> applied;

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual bool IsCapitalizable
        {
            get { return isCapitalizable; }
            set { isCapitalizable = value; }
        }

        public virtual IList<AppliedRetention> Applied
        {
            get { return applied; }
            set { applied = value; }
        }
    }
}
