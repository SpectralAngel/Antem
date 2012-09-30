using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    /// <summary>
    /// Representa una Persona cualquiera en el sistema, permitiendo que la
    /// misma tome varias funciones sin necesidad de repetir los datos.
    /// </summary>
    public class Person : Entity<int>
    {
        private String identification;
        private string firstName;
        private string lastName;
        private int sex;
        private DateTime birthday;
        private string civilState;
        private string profession;
        private State state;
        private Country county;
        private IList<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
        private IList<Address> addresses = new List<Address>();
        private IList<Email> emails = new List<Email>();

        public virtual String Identification
        {
            get { return identification; }
            set { identification = value; }
        }

        public virtual string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        /// <summary>
        /// </summary>
        public virtual string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public virtual int Sex
        {
            get { return sex; }
            set { sex = value; }
        }

        public virtual DateTime Birthday
        {
            get { return birthday; }
            set { birthday = value; }
        }

        public virtual string CivilState
        {
            get { return civilState; }
            set { civilState = value; }
        }

        public virtual string Profession
        {
            get { return profession; }
            set { profession = value; }
        }

        public virtual State State
        {
            get { return state; }
            set { state = value; }
        }

        public virtual Country County
        {
            get { return county; }
            set { county = value; }
        }

        public virtual IList<PhoneNumber> PhoneNumbers
        {
            get { return phoneNumbers; }
            set { phoneNumbers = value; }
        }

        public virtual IList<Address> Addresses
        {
            get { return addresses; }
            set { addresses = value; }
        }

        public virtual IList<Email> Emails
        {
            get { return emails; }
            set { emails = value; }
        }
    }
}
