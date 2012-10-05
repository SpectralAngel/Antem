using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Antem.Web.Models
{
    public class PersonViewModel
    {
        public virtual String Identification { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }

        public virtual int Sex { get; set; }

        public virtual DateTime Birthday { get; set; }

        public virtual string CivilState { get; set; }

        public virtual string Profession { get; set; }

        public virtual int State { get; set; }

        public virtual int County { get; set; }
    }
}
