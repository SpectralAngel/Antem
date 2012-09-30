using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.Configuration;
using System.Configuration;
using Entities = Antem.Models;
using System.Configuration.Provider;
using System.Data.Odbc;
using Antem.Parts;
using System.Composition;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Diagnostics;
using NHibernate;

namespace Antem.Security
{
    public sealed class AntemMembershipProvider : MembershipProvider
    {
        #region Private
        // Global connection string, generated password length, generic exception message, event log info.
        private int newPasswordLength = 8;
        private string eventSource = "AntemMembershipProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";

        private static ISessionFactory _sessionFactory;
        private string _applicationName;
        private bool _enablePasswordReset;
        private bool _enablePasswordRetrieval;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;
        private int _maxInvalidPasswordAttempts;
        private int _passwordAttemptWindow;
        private MembershipPasswordFormat _passwordFormat;
        // Used when determining encryption key values.
        private MachineKeySection _machineKey;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private string _passwordStrengthRegularExpression;

        #endregion

        #region Public Propeties
        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }


        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
        }


        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }


        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }


        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }


        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }


        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }


        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        public bool WriteExceptionsToEventLog { get; set; }

        /// <summary>Gets the session factory.</summary>
        private static ISessionFactory SessionFactory
        {
            get { return _sessionFactory; }
        }

        #endregion

        #region Helper functions
        // A Function to retrieve config values from the configuration file
        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

        //Fn to create a Membership user from a Entities.Users class
        private MembershipUser GetMembershipUserFromUser(Membership.User usr)
        {
            MembershipUser u = new MembershipUser(this.Name,
                                                  usr.Username,
                                                  usr.Id,
                                                  usr.Email,
                                                  usr.PasswordQuestion,
                                                  usr.Comment,
                                                  usr.IsApproved,
                                                  usr.IsLockedOut,
                                                  usr.CreationDate,
                                                  usr.LastLoginDate,
                                                  usr.LastActivityDate,
                                                  usr.LastPasswordChangedDate,
                                                  usr.LastLockedOutDate);

            return u;
        }

        //Fn that performs the checks and updates associated with password failure tracking
        private void UpdateFailureCount(string username, string failureType)
        {
            DateTime windowStart = new DateTime();
            int failureCount = 0;
            Entities.User usr = null;

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        usr = GetUserByUsername(username);

                        if (!(usr == null))
                        {
                            if (failureType == "password")
                            {
                                failureCount = usr.FailedPasswordAttemptCount;
                                windowStart = usr.FailedPasswordAttemptWindowStart;
                            }

                            if (failureType == "passwordAnswer")
                            {
                                failureCount = usr.FailedPasswordAnswerAttemptCount;
                                windowStart = usr.FailedPasswordAnswerAttemptWindowStart;
                            }
                        }

                        DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                        if (failureCount == 0 || DateTime.Now > windowEnd)
                        {
                            // First password failure or outside of PasswordAttemptWindow. 
                            // Start a new password failure count from 1 and a new window starting now.

                            if (failureType == "password")
                            {
                                usr.FailedPasswordAttemptCount = 1;
                                usr.FailedPasswordAttemptWindowStart = DateTime.Now; ;
                            }

                            if (failureType == "passwordAnswer")
                            {
                                usr.FailedPasswordAnswerAttemptCount = 1;
                                usr.FailedPasswordAnswerAttemptWindowStart = DateTime.Now; ;
                            }
                            session.Update(usr);
                            transaction.Commit();
                        }
                        else
                        {
                            if (failureCount++ >= MaxInvalidPasswordAttempts)
                            {
                                // Password attempts have exceeded the failure threshold. Lock out
                                // the user.
                                usr.IsLockedOut = true;
                                usr.LastLockedOutDate = DateTime.Now;
                                session.Update(usr);
                                transaction.Commit();
                            }
                            else
                            {
                                // Password attempts have not exceeded the failure threshold. Update
                                // the failure counts. Leave the window the same.

                                if (failureType == "password")
                                    usr.FailedPasswordAttemptCount = failureCount;

                                if (failureType == "passwordAnswer")
                                    usr.FailedPasswordAnswerAttemptCount = failureCount;

                                session.Update(usr);
                                transaction.Commit();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(e, "UpdateFailureCount");
                            throw new ProviderException("Unable to update failure count and window start." + exceptionMessage);
                        }
                        else
                            throw e;
                    }
                }
            }

        }

        //CheckPassword: Compares password values based on the MembershipPasswordFormat.
        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }

        //EncodePassword:Encrypts, Hashes, or leaves the password clear based on the PasswordFormat.
        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                      Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = HexToByte(_machineKey.ValidationKey);
                    encodedPassword =
                      Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }
            return encodedPassword;
        }

        // UnEncodePassword :Decrypts or leaves the password clear based on the PasswordFormat.
        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                      Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        //   Converts a hexadecimal string to a byte array. Used to convert encryption key values from the configuration.    
        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        // WriteToEventLog
        // A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.

        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }

        #endregion

        #region Private Methods

        //single fn to get a membership user by key or username
        private MembershipUser GetMembershipUserByKeyOrUser(bool isKeySupplied, string username, object providerUserKey, bool userIsOnline)
        {
            Entities.User usr = null;
            MembershipUser u = null;

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {

                    try
                    {
                        if (isKeySupplied)
                            usr = session.CreateCriteria(typeof(Entities.User))
                                            .Add(NHibernate.Criterion.Restrictions.Eq("Id", (int)providerUserKey))
                                            .UniqueResult<Entities.User>();

                        else
                            usr = session.CreateCriteria(typeof(Entities.User))
                                            .Add(NHibernate.Criterion.Restrictions.Eq("Username", username))
                                            .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                            .UniqueResult<Entities.User>();

                        if (usr != null)
                        {
                            u = GetMembershipUserFromUser(usr);

                            if (userIsOnline)
                            {
                                usr.LastActivityDate = System.DateTime.Now;
                                session.Update(usr);
                                transaction.Commit();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "GetUser(Object, Boolean)");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }
            return u;
        }

        private Membership.User GetUserByUsername(string username)
        {
            Membership.User usr = null;
            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {

                    try
                    {
                        usr = session.CreateCriteria(typeof(Entities.User))
                                        .Add(NHibernate.Criterion.Restrictions.Eq("Username", username))
                                        .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                        .UniqueResult<Entities.User>();


                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "UnlockUser");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }
            return usr;

        }

        private IList<Entities.User> GetUsers()
        {
            IList<Entities.User> usrs = null;
            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {

                    try
                    {
                        usrs = session.CreateCriteria(typeof(Entities.User))
                                        .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                        .List<Entities.User>();

                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "GetUsers");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }
            return usrs;

        }

        private IList<Entities.User> GetUsersLikeUsername(string usernameToMatch)
        {
            IList<Entities.User> usrs = null;
            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {

                    try
                    {
                        usrs = session.CreateCriteria(typeof(Entities.User))
                                        .Add(NHibernate.Criterion.Restrictions.Like("Username", usernameToMatch))
                                        .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                        .List<Entities.User>();

                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "GetUsersMatchByUsername");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }
            return usrs;

        }

        private IList<Entities.User> GetUsersLikeEmail(string emailToMatch)
        {
            IList<Entities.User> usrs = null;
            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {

                    try
                    {
                        usrs = session.CreateCriteria(typeof(Entities.User))
                                        .Add(NHibernate.Criterion.Restrictions.Like("Email", emailToMatch))
                                        .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                        .List<Entities.User>();

                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "GetUsersMatchByEmail");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }
            return usrs;

        }
        #endregion

        #region Public methods

        // Initilaize the provider 
        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "Antem MemebershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Antem Nhibernate Membership provider");
            }
            // Initialize the abstract base class.
            base.Initialize(name, config);

            _applicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            _maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            _minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            _minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            _passwordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            _enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            _enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            _requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            _requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            WriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));


            string temp_format = config["passwordFormat"];
            if (temp_format == null)
            {
                temp_format = "Hashed";
            }

            switch (temp_format)
            {
                case "Hashed":
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    _passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            //Encryption skipped
            Configuration cfg =
                            WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            _machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            if (_machineKey.ValidationKey.Contains("AutoGenerate"))
                if (PasswordFormat != MembershipPasswordFormat.Clear)
                    throw new ProviderException("Hashed or Encrypted passwords are not supported with auto-generated keys.");

            // create our Fluent NHibernate session factory
            var provider = new Provider();
            _sessionFactory = provider.BuildSessionFactory();

        }

        // Change password for a user
        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            int rowsAffected = 0;
            Entities.User usr = null;
            if (!ValidateUser(username, oldPwd))
                return false;

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPwd, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {

                    try
                    {
                        usr = GetUserByUsername(username);

                        if (usr != null)
                        {
                            usr.Password = EncodePassword(newPwd);
                            usr.LastPasswordChangedDate = System.DateTime.Now;
                            session.Update(usr);
                            transaction.Commit();
                            rowsAffected = 1;
                        }
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "ChangePassword");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }

            if (rowsAffected > 0)
                return true;
            return false;
        }

        // Change Password Question And Answer for a user
        public override bool ChangePasswordQuestionAndAnswer(string username,
                      string password,
                      string newPwdQuestion,
                      string newPwdAnswer)
        {
            Entities.User usr = null;
            int rowsAffected = 0;
            if (!ValidateUser(username, password))
                return false;

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        usr = GetUserByUsername(username);
                        if (usr != null)
                        {
                            usr.PasswordQuestion = newPwdQuestion;
                            usr.PasswordAnswer = newPwdAnswer;
                            session.Update(usr);
                            transaction.Commit();
                            rowsAffected = 1;
                        }
                    }
                    catch (OdbcException e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "ChangePasswordQuestionAndAnswer");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }

            if (rowsAffected > 0)
                return true;
            return false;
        }

        // Create a new Membership user 
        public override MembershipUser CreateUser(string username,
                 string password,
                 string email,
                 string passwordQuestion,
                 string passwordAnswer,
                 bool isApproved,
                 object providerUserKey,
                 out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);
            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                //provider user key in our case is auto int

                using (ISession session = SessionFactory.OpenSession())
                {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        Entities.User user = new Entities.User();
                        user.Username = username;
                        user.Password = EncodePassword(password);
                        user.Email = email;
                        user.PasswordQuestion = passwordQuestion;
                        user.PasswordAnswer = EncodePassword(passwordAnswer);
                        user.IsApproved = isApproved;
                        user.Comment = "";
                        user.CreationDate = createDate;
                        user.LastPasswordChangedDate = createDate;
                        user.LastActivityDate = createDate;
                        user.ApplicationName = _applicationName;
                        user.IsLockedOut = false;
                        user.LastLockedOutDate = createDate;
                        user.FailedPasswordAttemptCount = 0;
                        user.FailedPasswordAttemptWindowStart = createDate;
                        user.FailedPasswordAnswerAttemptCount = 0;
                        user.FailedPasswordAnswerAttemptWindowStart = createDate;

                        try
                        {
                            int retId = (int)session.Save(user);

                            transaction.Commit();
                            if ((retId < 1))
                                status = MembershipCreateStatus.UserRejected;
                            else
                                status = MembershipCreateStatus.Success;
                        }
                        catch (Exception e)
                        {
                            status = MembershipCreateStatus.ProviderError;
                            if (WriteExceptionsToEventLog)
                                WriteToEventLog(e, "CreateUser");
                        }
                    }
                }

                //retrive and return user by user name
                return GetUser(username, false);
            }
            else
                status = MembershipCreateStatus.DuplicateUserName;
            return null;
        }

        // Delete a user 
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            int rowsAffected = 0;
            Entities.User usr = null;

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        usr = GetUserByUsername(username);
                        if (usr != null)
                        {
                            session.Delete(usr);
                            transaction.Commit();
                            rowsAffected = 1;
                        }
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "DeleteUser");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }

            if (rowsAffected > 0)
                return true;
            return false;
        }

        // Get all users in db
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();
            totalRecords = 0;
            IList<Entities.User> allusers = null;
            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        totalRecords = (Int32)session.CreateCriteria(typeof(Entities.User))
                                    .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                    .SetProjection(NHibernate.Criterion.Projections.Count("Id")).UniqueResult();

                        if (totalRecords <= 0) { return users; }

                        allusers = GetUsers();
                        foreach (Entities.User u in allusers)
                        {
                            if (counter >= endIndex)
                                break;
                            if (counter >= startIndex)
                            {
                                MembershipUser mu = GetMembershipUserFromUser(u);
                                users.Add(mu);
                            }
                            counter++;
                        }
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "GetAllUsers");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }
            return users;
        }

        // Gets a number of online users
        public override int GetNumberOfUsersOnline()
        {
            TimeSpan onlineSpan = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);
            int numOnline = 0;

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        numOnline = (Int32)session.CreateCriteria(typeof(Entities.User))
                                           .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                           .Add(NHibernate.Criterion.Restrictions.Gt("LastActivityDate", compareTime))
                                           .SetProjection(NHibernate.Criterion.Projections.Count("Id")).UniqueResult();
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "GetNumberOfUsersOnline");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }

            return numOnline;
        }

        // Get a password fo a user
        public override string GetPassword(string username, string answer)
        {
            string password = string.Empty;
            string passwordAnswer = string.Empty;

            if (!EnablePasswordRetrieval)
                throw new ProviderException("Password Retrieval Not Enabled.");

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
                throw new ProviderException("Cannot retrieve Hashed passwords.");

            try
            {
                Entities.User usr = GetUserByUsername(username);

                if (usr == null)
                    throw new MembershipPasswordException("The supplied user name is not found.");
                else
                {
                    if (usr.IsLockedOut)
                        throw new MembershipPasswordException("The supplied user is locked out.");

                    password = usr.Password;
                    passwordAnswer = usr.PasswordAnswer;

                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                    WriteToEventLog(e, "GetPassword");
                throw new ProviderException(exceptionMessage);
            }

            if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
            {
                UpdateFailureCount(username, "passwordAnswer");
                throw new MembershipPasswordException("Incorrect password answer.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Encrypted)
                password = UnEncodePassword(password);

            return password;
        }

        // Get a membership user by username
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return GetMembershipUserByKeyOrUser(false, username, 0, userIsOnline);
        }

        //  // Get a membership user by key ( in our case key is int)
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return GetMembershipUserByKeyOrUser(true, string.Empty, providerUserKey, userIsOnline);
        }

        //Unlock a user given a username 
        public override bool UnlockUser(string username)
        {
            Entities.User usr = null;
            bool unlocked = false;


            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {

                    try
                    {
                        usr = GetUserByUsername(username);

                        if (usr != null)
                        {
                            usr.LastLockedOutDate = System.DateTime.Now;
                            usr.IsLockedOut = false;
                            session.Update(usr);
                            transaction.Commit();
                            unlocked = true;
                        }
                    }
                    catch (Exception e)
                    {
                        WriteToEventLog(e, "UnlockUser");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }

            return unlocked;
        }

        //Gets a membehsip user by email
        public override string GetUserNameByEmail(string email)
        {
            Entities.User usr = null;
            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {

                    try
                    {
                        usr = session.CreateCriteria(typeof(Entities.User))
                                            .Add(NHibernate.Criterion.Restrictions.Eq("Email", email))
                                            .UniqueResult<Entities.User>();
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "GetUserNameByEmail");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }
            if (usr == null)
                return string.Empty;
            else
                return usr.Username; ;
        }

        // Reset password for a user
        public override string ResetPassword(string username, string answer)
        {
            int rowsAffected = 0;
            Entities.User usr = null;

            if (!EnablePasswordReset)
                throw new NotSupportedException("Password reset is not enabled.");

            if (answer == null && RequiresQuestionAndAnswer)
            {
                UpdateFailureCount(username, "passwordAnswer");
                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword =
                            System.Web.Security.Membership.GeneratePassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);


            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");

            string passwordAnswer = "";

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        usr = GetUserByUsername(username);
                        if (usr == null)
                            throw new MembershipPasswordException("The supplied user name is not found.");

                        if (usr.IsLockedOut)
                            throw new MembershipPasswordException("The supplied user is locked out.");

                        if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                        {
                            UpdateFailureCount(username, "passwordAnswer");
                            throw new MembershipPasswordException("Incorrect password answer.");
                        }

                        usr.Password = EncodePassword(newPassword);
                        usr.LastPasswordChangedDate = System.DateTime.Now;
                        usr.Username = username;
                        usr.ApplicationName = this.ApplicationName;
                        session.Update(usr);
                        transaction.Commit();
                        rowsAffected = 1;
                    }
                    catch (OdbcException e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "ResetPassword");
                        throw new ProviderException(exceptionMessage);
                    }
                }
            }

            if (rowsAffected > 0)
                return newPassword;
            else
                throw new MembershipPasswordException("User not found, or user is locked out. Password not Reset.");
        }

        // Update a user information 
        public override void UpdateUser(MembershipUser user)
        {
            Entities.User usr = null;
            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        usr = GetUserByUsername(user.UserName);
                        if (usr != null)
                        {
                            usr.Email = user.Email;
                            usr.Comment = user.Comment;
                            usr.IsApproved = user.IsApproved;
                            session.Update(usr);
                            transaction.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(e, "UpdateUser");
                            throw new ProviderException(exceptionMessage);
                        }
                    }
                }
            }
        }

        // Validates as user
        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;
            Entities.User usr = null;

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        usr = GetUserByUsername(username);
                        if (usr == null)
                            return false;
                        if (usr.IsLockedOut)
                            return false;

                        if (CheckPassword(password, usr.Password))
                        {
                            if (usr.IsApproved)
                            {
                                isValid = true;
                                usr.LastLoginDate = DateTime.Now;
                                session.Update(usr);
                                transaction.Commit();
                            }
                        }
                        else
                            UpdateFailureCount(username, "password");
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(e, "ValidateUser");
                            throw new ProviderException(exceptionMessage);
                        }
                        else
                            throw e;
                    }
                }
            }
            return isValid;
        }

        // Find users by a name, note : does not do a like search
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            IList<Entities.User> allusers = null;
            MembershipUserCollection users = new MembershipUserCollection();
            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;
            totalRecords = 0;

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        allusers = GetUsersLikeUsername(usernameToMatch);
                        if (allusers == null)
                            return users;
                        if (allusers.Count > 0)
                            totalRecords = allusers.Count;
                        else
                            return users;

                        foreach (Entities.User u in allusers)
                        {
                            if (counter >= endIndex)
                                break;
                            if (counter >= startIndex)
                            {
                                MembershipUser mu = GetMembershipUserFromUser(u);
                                users.Add(mu);
                            }
                            counter++;
                        }
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(e, "FindUsersByName");
                            throw new ProviderException(exceptionMessage);
                        }
                        else
                            throw e;
                    }

                }
            }

            return users;
        }

        // Search users by email , NOT a Like match
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {

            IList<Entities.User> allusers = null;
            MembershipUserCollection users = new MembershipUserCollection();
            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;
            totalRecords = 0;

            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        allusers = GetUsersLikeEmail(emailToMatch);
                        if (allusers == null)
                            return users;
                        if (allusers.Count > 0)
                            totalRecords = allusers.Count;
                        else
                            return users;

                        foreach (Entities.User u in allusers)
                        {
                            if (counter >= endIndex)
                                break;
                            if (counter >= startIndex)
                            {
                                MembershipUser mu = GetMembershipUserFromUser(u);
                                users.Add(mu);
                            }
                            counter++;
                        }
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(e, "FindUsersByEmail");
                            throw new ProviderException(exceptionMessage);
                        }
                        else
                            throw e;
                    }
                }
            }
            return users;
        }

        #endregion
    }
}
