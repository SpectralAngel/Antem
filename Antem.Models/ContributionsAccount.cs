using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class ContributionsAccount : Account
    {
        private DateTime liberation;

        public virtual DateTime Liberation
        {
            get { return liberation; }
            set { liberation = value; }
        }
    
        public override bool CanWithdraw()
        {
            if (Liberation >= DateTime.Now)
            {
                return true;
            }
            return false;
        }
    }
}
