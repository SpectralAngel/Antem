using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class ContributionsAccount : SavingAccount
    {
        private DateTime liberation = DateTime.UtcNow;
        private bool liberated = false;

        public virtual bool Liberated
        {
            get { return liberated; }
        }

        public virtual DateTime Liberation
        {
            get { return liberation; }
        }

        public virtual void Liberate(DateTime day)
        {
            liberated = true;
            liberation = day;
        }

        public override bool CanWithdraw()
        {
            if (Liberated && Liberation >= DateTime.UtcNow && Balance > 0)
            {
                return true;
            }
            return false;
        }
    }
}
