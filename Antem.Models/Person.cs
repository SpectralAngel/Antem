﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    /// <summary>
    /// Represents a person on any place of the application, in order
    /// to avoid repeating data
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
        private Town town;
        private IList<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
        private IList<Address> addresses = new List<Address>();
        private IList<Email> emails = new List<Email>();

        /// <summary>
        /// According to Honduran standards, it must be a 13 digit number,
        /// which most people divides using dashes
        /// </summary>
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

        public virtual Town Town
        {
            get { return town; }
            set { town = value; }
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

        public virtual int Age
        {
            get
            {
                return (DateTime.UtcNow - birthday).Days / 365;
            }
        }
    }
}
