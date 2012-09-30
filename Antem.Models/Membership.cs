using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antem.Models
{
    public class Membership : Entity<int>
    {
        private User user;
        private DateTime createDate = DateTime.UtcNow;
        private string confirmationToken;
        private bool isConfirmed = false;
        private DateTime lastPasswordFailure = DateTime.MinValue;
        private int passwordFailuresSinceLastSucces = 0;
        private string password;
        private DateTime passwordChangeDate = DateTime.UtcNow;
        private string passwordSalt;
        private string passwordVerificationToken;
        private DateTime passwordVerificationTokenExpirationDate = DateTime.MinValue;

        public virtual User User
        {
            get { return user; }
            set { user = value; }
        }

        public virtual DateTime CreateDate
        {
            get { return createDate; }
            set { createDate = value; }
        }

        public virtual string ConfirmationToken
        {
            get { return confirmationToken; }
            set { confirmationToken = value; }
        }

        public virtual bool IsConfirmed
        {
            get { return isConfirmed; }
            set { isConfirmed = value; }
        }

        public virtual DateTime LastPasswordFailure
        {
            get { return lastPasswordFailure; }
            set { lastPasswordFailure = value; }
        }

        public virtual int PasswordFailuresSinceLastSucces
        {
            get { return passwordFailuresSinceLastSucces; }
            set { passwordFailuresSinceLastSucces = value; }
        }

        public virtual string Password
        {
            get { return password; }
            set { password = value; }
        }

        public virtual DateTime PasswordChangeDate
        {
            get { return passwordChangeDate; }
            set { passwordChangeDate = value; }
        }

        public virtual string PasswordSalt
        {
            get { return passwordSalt; }
            set { passwordSalt = value; }
        }

        public virtual string PasswordVerificationToken
        {
            get { return passwordVerificationToken; }
            set { passwordVerificationToken = value; }
        }

        public virtual DateTime PasswordVerificationTokenExpirationDate
        {
            get { return passwordVerificationTokenExpirationDate; }
            set { passwordVerificationTokenExpirationDate = value; }
        }
    }
}
