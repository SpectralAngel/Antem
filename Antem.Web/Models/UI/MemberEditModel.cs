using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Antem.Web.Models.UI
{
    public class MemberEditModel
    {
        public int Id { get; set; }

        public String Identification { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Sex { get; set; }

        public DateTime Birthday { get; set; }

        public string CivilState { get; set; }

        public string Profession { get; set; }

        public int State { get; set; }

        public int Town { get; set; }

        public int Branch { get; set; }

        public DateTime Retirement { get; set; }

        public bool IsActive { get; set; }

        public DateTime Joined { get; set; }
    }
}
