using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Antem.Web.Models
{
    public class MemberViewModel
    {
        public int Id { get; set; }

        [Required]
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

        public int PaymentMethod { get; set; }

        public DateTime Retirement { get; set; }

        public bool IsActive { get; set; }

        public DateTime Joined { get; set; }
    }
}
