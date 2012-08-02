using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    public class WithdrawableAccount : Account
    {
        public override bool CanWithdraw()
        {
            if (Balance > 0)
            {
                return true;
            }
            return false;
        }
    }
}
