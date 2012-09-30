using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Security;
using System.Web.WebPages;
using Microsoft.Internal.Web.Utils;
using WebMatrix.Data;
using WebMatrix.WebData.Resources;
using WebMatrix.WebData;
using Antem.Parts;
using System.Composition;

namespace Antem.Membership
{
    public class SimpleMembershipProvider : ExtendedMembershipProvider
    {
        private const int TokenSizeInBytes = 16;
        private readonly MembershipProvider _previousProvider;
        private IRepository<User> userRepository;
        private IRepository<Membership> membershipRepository;
        private IRepository<OauthToken> oauthTokenRepository;
        private ExportFactory<IUnitOfWork> unitOfWork;

        public SimpleMembershipProvider()
            : this(null)
        {
        }

        public SimpleMembershipProvider(MembershipProvider previousProvider)
        {
            _previousProvider = previousProvider;
            if (_previousProvider != null)
            {
                _previousProvider.ValidatingPassword += (sender, args) =>
                {
                    if (!InitializeCalled)
                    {
                        OnValidatingPassword(args);
                    }
                };
            }
        }

        private MembershipProvider PreviousProvider
        {
            get
            {
                if (_previousProvider == null)
                {
                    throw new InvalidOperationException(WebDataResources.Security_InitializeMustBeCalledFirst);
                }
                else
                {
                    return _previousProvider;
                }
            }
        }

        // Public properties
        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool EnablePasswordRetrieval
        {
            get { return InitializeCalled ? false : PreviousProvider.EnablePasswordRetrieval; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool EnablePasswordReset
        {
            get { return InitializeCalled ? false : PreviousProvider.EnablePasswordReset; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool RequiresQuestionAndAnswer
        {
            get { return InitializeCalled ? false : PreviousProvider.RequiresQuestionAndAnswer; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool RequiresUniqueEmail
        {
            get { return InitializeCalled ? false : PreviousProvider.RequiresUniqueEmail; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return InitializeCalled ? MembershipPasswordFormat.Hashed : PreviousProvider.PasswordFormat; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int MaxInvalidPasswordAttempts
        {
            get { return InitializeCalled ? Int32.MaxValue : PreviousProvider.MaxInvalidPasswordAttempts; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int PasswordAttemptWindow
        {
            get { return InitializeCalled ? Int32.MaxValue : PreviousProvider.PasswordAttemptWindow; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int MinRequiredPasswordLength
        {
            get { return InitializeCalled ? 0 : PreviousProvider.MinRequiredPasswordLength; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return InitializeCalled ? 0 : PreviousProvider.MinRequiredNonAlphanumericCharacters; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string PasswordStrengthRegularExpression
        {
            get { return InitializeCalled ? String.Empty : PreviousProvider.PasswordStrengthRegularExpression; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string ApplicationName
        {
            get
            {
                if (InitializeCalled)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    return PreviousProvider.ApplicationName;
                }
            }
            set
            {
                if (InitializeCalled)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    PreviousProvider.ApplicationName = value;
                }
            }
        }

        private string SafeUserNameColumn
        {
            get { return "[" + UserNameColumn + "]"; }
        }

        // represents the User table for the app
        public string UserTableName { get; set; }

        // represents the User created UserName column, i.e. Email
        public string UserNameColumn { get; set; }

        // Represents the User created id column, i.e. ID;
        // REVIEW: we could get this from the primary key of UserTable in the future
        public string UserIdColumn { get; set; }

        internal bool InitializeCalled { get; set; }

        internal override void VerifyInitialized()
        {
            if (!InitializeCalled)
            {
                throw new InvalidOperationException(WebDataResources.Security_InitializeMustBeCalledFirst);
            }
        }

        // Inherited from ProviderBase - The "previous provider" we get has already been initialized by the Config system,
        // so we shouldn't forward this call
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (String.IsNullOrEmpty(name))
            {
                name = "SimpleMembershipProvider";
            }
            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Antem Simple Membership Provider");
            }
            base.Initialize(name, config);

            config.Remove("connectionStringName");
            config.Remove("enablePasswordRetrieval");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("applicationName");
            config.Remove("requiresUniqueEmail");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("passwordFormat");
            config.Remove("name");
            config.Remove("description");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            config.Remove("hashAlgorithmType");

            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                {
                    throw new ProviderException(String.Format(CultureInfo.CurrentCulture, WebDataResources.SimpleMembership_ProviderUnrecognizedAttribute, attribUnrecognized));
                }
            }
        }

        // Not an override ==> Simple Membership MUST be enabled to use this method
        public int GetUserId(string userName)
        {
            VerifyInitialized();
            using (var unitofwork = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var user = userRepository.Where(u => u.Username == userName).Single();
                    return user.Id;
                }
                catch
                {
                    return -1;
                }
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override int GetUserIdFromPasswordResetToken(string token)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var membership = membershipRepository.Where(m => m.PasswordVerificationToken == token).Single();
                    return membership.User.Id;
                }
                catch
                {
                    return -1;
                }
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the confirmed flag for the username if it is correct.
        /// </summary>
        /// <returns>True if the account could be successfully confirmed. False if the username was not found or the confirmation token is invalid.</returns>
        /// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
        public override bool ConfirmAccount(string userName, string accountConfirmationToken)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var membership = membershipRepository.Where(m => m.User.Username == userName).Single();
                    if(String.Equals(accountConfirmationToken, membership.ConfirmationToken, StringComparison.Ordinal))
                    {
                        membership.IsConfirmed = true;
                        membershipRepository.Add(membership);
                        unit.Commit();
                        return true;
                    }
                }
                catch
                {
                    unit.Rollback();
                    return false;
                }
                return false;
            }
        }

        /// <summary>
        /// Sets the confirmed flag for the username if it is correct.
        /// </summary>
        /// <returns>True if the account could be successfully confirmed. False if the username was not found or the confirmation token is invalid.</returns>
        /// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method.
        /// There is a tiny possibility where this method fails to work correctly. Two or more users could be assigned the same token but specified using different cases.
        /// A workaround for this would be to use the overload that accepts both the user name and confirmation token.
        /// </remarks>
        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var memberships = membershipRepository.Where(m => m.ConfirmationToken.Equals(accountConfirmationToken, StringComparison.Ordinal)).ToList();
                Debug.Assert(memberships.Count < 2, "By virtue of the fact that the ConfirmationToken is random and unique, we can never have two tokens that are identical.");
                var membership = memberships.First();
                membership.IsConfirmed = true;
                try
                {
                    membershipRepository.Add(membership);
                    unit.Commit();
                    return membership.IsConfirmed;
                }
                catch
                {
                    unit.Rollback();
                }
                return false;
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            VerifyInitialized();

            if (password.IsEmpty())
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            string hashedPassword = Crypto.HashPassword(password);
            if (hashedPassword.Length > 128)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            if (userName.IsEmpty())
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidUserName);
            }

            using (var unit = unitOfWork.CreateExport().Value)
            {
                // Check if the user exists
                User user;
                try
                {
                    user = userRepository.Where(u => u.Username == userName).Single();
                }
                catch
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }
                // Step 2: Check if the user exists in the Membership table: Error if yes.
                if (user.Memberships.Count > 0)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.DuplicateUserName);
                }
                // Step 3: Create user in Membership table
                string token = null;
                object dbtoken = DBNull.Value;
                if (requireConfirmationToken)
                {
                    token = GenerateToken();
                    dbtoken = token;
                }
                int defaultNumPasswordFailures = 0;
                var membership = new Membership()
                {
                    User = user,
                    Password = hashedPassword,
                    PasswordSalt = String.Empty,
                    IsConfirmed = !requireConfirmationToken,
                    ConfirmationToken = token,

                };
                try
                {
                    membershipRepository.Add(membership);
                    unit.Commit();
                }
                catch
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }
                return token;

            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
            }
            throw new NotSupportedException();
        }

        private void CreateUserRow(string userName, IDictionary<string, object> values)
        {
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var users = userRepository.Where(u => u.Username == userName).Count();
                if (users > 0)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.DuplicateUserName);
                }
                var user = new User();
                user.Username = userName;
                if (values != null)
                {
                    foreach (string key in values.Keys)
                    {
                        if(key.Equals("UserName"))
                        {
                            continue;
                        }
                        var prop = user.GetType().GetProperty(key);
                        if(prop != null && prop.CanWrite)
                        {
                            prop.SetValue(user, values[key]);
                        }
                    }
                }
                try
                {
                    userRepository.Add(user);
                    unit.Commit();
                }
                catch
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override string CreateUserAndAccount(string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            VerifyInitialized();

            CreateUserRow(userName, values);
            return CreateAccount(userName, password, requireConfirmation);
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string GetPassword(string username, string answer)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetPassword(username, answer);
            }
            throw new NotSupportedException();
        }

        private bool SetPassword(int userId, string newPassword)
        {
            string hashedPassword = Crypto.HashPassword(newPassword);
            if (hashedPassword.Length > 128)
            {
                throw new ArgumentException(WebDataResources.SimpleMembership_PasswordTooLong);
            }

            using (var unit = unitOfWork.CreateExport().Value)
            {
                var membership = membershipRepository.Where(m => m.User.Id == userId).Single();
                membership.Password = hashedPassword;
                membership.PasswordChangeDate = DateTime.UtcNow;
                try
                {
                    membershipRepository.Save(membership);
                    unit.Commit();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.ChangePassword(username, oldPassword, newPassword);
            }

            // REVIEW: are commas special in the password?
            if (username.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "username");
            }
            if (oldPassword.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "oldPassword");
            }
            if (newPassword.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "newPassword");
            }

            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
                if (userId == -1)
                {
                    return false; // User not found
                }

                // First check that the old credentials match
                if (!CheckPassword(db, userId, oldPassword))
                {
                    return false;
                }

                return SetPassword(db, userId, newPassword);
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string ResetPassword(string username, string answer)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.ResetPassword(username, answer);
            }
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetUser(providerUserKey, userIsOnline);
            }
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetUser(username, userIsOnline);
            }

            // Due to a bug in v1, GetUser allows passing null / empty values.
            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
                if (userId == -1)
                {
                    return null; // User not found
                }

                return new MembershipUser(Membership.Provider.Name, username, userId, null, null, null, true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string GetUserNameByEmail(string email)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetUserNameByEmail(email);
            }
            throw new NotSupportedException();
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override bool DeleteAccount(string userName)
        {
            VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == -1)
                {
                    return false; // User not found
                }

                int deleted = db.Execute(@"DELETE FROM " + MembershipTableName + " WHERE UserId = @0", userId);
                return (deleted == 1);
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.DeleteUser(username, deleteAllRelatedData);
            }

            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
                if (userId == -1)
                {
                    return false; // User not found
                }

                int deleted = db.Execute(@"DELETE FROM " + SafeUserTableName + " WHERE " + SafeUserIdColumn + " = @0", userId);
                bool returnValue = (deleted == 1);

                //if (deleteAllRelatedData) {
                // REVIEW: do we really want to delete from the user table?
                //}
                return returnValue;
            }
        }

        internal bool DeleteUserAndAccountInternal(string userName)
        {
            return (DeleteAccount(userName) && DeleteUser(userName, false));
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetAllUsers(pageIndex, pageSize, out totalRecords);
            }
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int GetNumberOfUsersOnline()
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetNumberOfUsersOnline();
            }
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
            }
            throw new NotSupportedException();
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
            }
            throw new NotSupportedException();
        }

        private static int GetPasswordFailuresSinceLastSuccess(IDatabase db, int userId)
        {
            var failure = db.QueryValue(@"SELECT PasswordFailuresSinceLastSuccess FROM " + MembershipTableName + " WHERE (UserId = @0)", userId);
            if (failure != null)
            {
                return failure;
            }
            return -1;
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, userName));
                }

                return GetPasswordFailuresSinceLastSuccess(db, userId);
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override DateTime GetCreateDate(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, userName));
                }

                var createDate = db.QueryValue(@"SELECT CreateDate FROM " + MembershipTableName + " WHERE (UserId = @0)", userId);
                if (createDate != null)
                {
                    return createDate;
                }
                return DateTime.MinValue;
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override DateTime GetPasswordChangedDate(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, userName));
                }

                var pwdChangeDate = db.QuerySingle(@"SELECT PasswordChangedDate FROM " + MembershipTableName + " WHERE (UserId = @0)", userId);
                if (pwdChangeDate != null && pwdChangeDate[0] != null)
                {
                    return (DateTime)pwdChangeDate[0];
                }
                return DateTime.MinValue;
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, userName);
                if (userId == -1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, userName));
                }

                var failureDate = db.QuerySingle(@"SELECT LastPasswordFailureDate FROM " + MembershipTableName + " WHERE (UserId = @0)", userId);
                if (failureDate != null && failureDate[0] != null)
                {
                    return (DateTime)failureDate[0];
                }
                return DateTime.MinValue;
            }
        }

        private bool CheckPassword(IDatabase db, int userId, string password)
        {
            string hashedPassword = GetHashedPassword(db, userId);
            bool verificationSucceeded = (hashedPassword != null && Crypto.VerifyHashedPassword(hashedPassword, password));
            if (verificationSucceeded)
            {
                // Reset password failure count on successful credential check
                db.Execute(@"UPDATE " + MembershipTableName + " SET PasswordFailuresSinceLastSuccess = 0 WHERE (UserId = @0)", userId);
            }
            else
            {
                int failures = GetPasswordFailuresSinceLastSuccess(db, userId);
                if (failures != -1)
                {
                    db.Execute(@"UPDATE " + MembershipTableName + " SET PasswordFailuresSinceLastSuccess = @1, LastPasswordFailureDate = @2 WHERE (UserId = @0)", userId, failures + 1, DateTime.UtcNow);
                }
            }
            return verificationSucceeded;
        }

        private string GetHashedPassword(IDatabase db, int userId)
        {
            var pwdQuery = db.Query(@"SELECT m.[Password] " +
                                    @"FROM " + MembershipTableName + " m, " + SafeUserTableName + " u " +
                                    @"WHERE m.UserId = " + userId + " AND m.UserId = u." + SafeUserIdColumn).ToList();
            // REVIEW: Should get exactly one match, should we throw if we get > 1?
            if (pwdQuery.Count != 1)
            {
                return null;
            }
            return pwdQuery[0].Password;
        }

        // Ensures the user exists in the accounts table
        private int VerifyUserNameHasConfirmedAccount(IDatabase db, string username, bool throwException)
        {
            int userId = GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, username);
            if (userId == -1)
            {
                if (throwException)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoUserFound, username));
                }
                else
                {
                    return -1;
                }
            }

            int result = db.QueryValue(@"SELECT COUNT(*) FROM " + MembershipTableName + " WHERE (UserId = @0 AND IsConfirmed = 1)", userId);
            if (result == 0)
            {
                if (throwException)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, WebDataResources.Security_NoAccountFound, username));
                }
                else
                {
                    return -1;
                }
            }
            return userId;
        }

        private static string GenerateToken()
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                return GenerateToken(prng);
            }
        }

        internal static string GenerateToken(RandomNumberGenerator generator)
        {
            byte[] tokenBytes = new byte[TokenSizeInBytes];
            generator.GetBytes(tokenBytes);
            return HttpServerUtility.UrlTokenEncode(tokenBytes);
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            VerifyInitialized();
            if (userName.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "userName");
            }
            using (var db = ConnectToDatabase())
            {
                int userId = VerifyUserNameHasConfirmedAccount(db, userName, throwException: true);

                string token = db.QueryValue(@"SELECT PasswordVerificationToken FROM " + MembershipTableName + " WHERE (UserId = @0 AND PasswordVerificationTokenExpirationDate > @1)", userId, DateTime.UtcNow);
                if (token == null)
                {
                    token = GenerateToken();

                    int rows = db.Execute(@"UPDATE " + MembershipTableName + " SET PasswordVerificationToken = @0, PasswordVerificationTokenExpirationDate = @1 WHERE (UserId = @2)", token, DateTime.UtcNow.AddMinutes(tokenExpirationInMinutesFromNow), userId);
                    if (rows != 1)
                    {
                        throw new ProviderException(WebDataResources.Security_DbFailure);
                    }
                }
                else
                {
                    // TODO: should we update expiry again?
                }
                return token;
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override bool IsConfirmed(string userName)
        {
            VerifyInitialized();
            if (userName.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "userName");
            }

            using (var db = ConnectToDatabase())
            {
                int userId = VerifyUserNameHasConfirmedAccount(db, userName, throwException: false);
                return (userId != -1);
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            VerifyInitialized();
            if (newPassword.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "newPassword");
            }
            using (var db = ConnectToDatabase())
            {
                int? userId = db.QueryValue(@"SELECT UserId FROM " + MembershipTableName + " WHERE (PasswordVerificationToken = @0 AND PasswordVerificationTokenExpirationDate > @1)", token, DateTime.UtcNow);
                if (userId != null)
                {
                    bool success = SetPassword(db, userId.Value, newPassword);
                    if (success)
                    {
                        // Clear the Token on success
                        int rows = db.Execute(@"UPDATE " + MembershipTableName + " SET PasswordVerificationToken = NULL, PasswordVerificationTokenExpirationDate = NULL WHERE (UserId = @0)", userId);
                        if (rows != 1)
                        {
                            throw new ProviderException(WebDataResources.Security_DbFailure);
                        }
                    }
                    return success;
                }
                else
                {
                    return false;
                }
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override void UpdateUser(MembershipUser user)
        {
            if (!InitializeCalled)
            {
                PreviousProvider.UpdateUser(user);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool UnlockUser(string userName)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.UnlockUser(userName);
            }
            throw new NotSupportedException();
        }

        internal void ValidateUserTable()
        {
            using (var db = ConnectToDatabase())
            {
                // GetUser will fail with an exception if the user table isn't set up properly
                try
                {
                    GetUserId(db, SafeUserTableName, SafeUserNameColumn, SafeUserIdColumn, "z");
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, WebDataResources.Security_FailedToFindUserTable, UserTableName), e);
                }
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool ValidateUser(string username, string password)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.ValidateUser(username, password);
            }
            if (username.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "username");
            }
            if (password.IsEmpty())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "password");
            }

            using (var db = ConnectToDatabase())
            {
                int userId = VerifyUserNameHasConfirmedAccount(db, username, throwException: false);
                if (userId == -1)
                {
                    return false;
                }
                else
                {
                    return CheckPassword(db, userId, password);
                }
            }
        }

        public override string GetUserNameFromId(int userId)
        {
            VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                dynamic username = db.QueryValue("SELECT " + SafeUserNameColumn + " FROM " + SafeUserTableName + " WHERE (" + SafeUserIdColumn + "=@0)", userId);
                return (string)username;
            }
        }

        public override void CreateOrUpdateOAuthAccount(string provider, string providerUserId, string userName)
        {
            VerifyInitialized();

            if (userName.IsEmpty())
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
            }

            int userId = GetUserId(userName);
            if (userId == -1)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidUserName);
            }

            var oldUserId = GetUserIdFromOAuth(provider, providerUserId);
            using (var db = ConnectToDatabase())
            {
                if (oldUserId == -1)
                {
                    // account doesn't exist. create a new one.
                    int insert = db.Execute(@"INSERT INTO [" + OAuthMembershipTableName + "] (Provider, ProviderUserId, UserId) VALUES (@0, @1, @2)", provider, providerUserId, userId);
                    if (insert != 1)
                    {
                        throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                    }
                }
                else
                {
                    // account already exist. update it
                    int insert = db.Execute(@"UPDATE [" + OAuthMembershipTableName + "] SET UserId = @2 WHERE UPPER(Provider)=@0 AND UPPER(ProviderUserId)=@1", provider, providerUserId, userId);
                    if (insert != 1)
                    {
                        throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                    }
                }
            }
        }

        public override void DeleteOAuthAccount(string provider, string providerUserId)
        {
            VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                // account doesn't exist. create a new one.
                int insert = db.Execute(@"DELETE FROM [" + OAuthMembershipTableName + "] WHERE UPPER(Provider)=@0 AND UPPER(ProviderUserId)=@1", provider, providerUserId);
                if (insert != 1)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }
            }
        }

        public override int GetUserIdFromOAuth(string provider, string providerUserId)
        {
            VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                dynamic id = db.QueryValue(@"SELECT UserId FROM [" + OAuthMembershipTableName + "] WHERE UPPER(Provider)=@0 AND UPPER(ProviderUserId)=@1", provider.ToUpperInvariant(), providerUserId.ToUpperInvariant());
                if (id != null)
                {
                    return (int)id;
                }

                return -1;
            }
        }

        public override string GetOAuthTokenSecret(string token)
        {
            VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                CreateOAuthTokenTableIfNeeded(db);

                // Note that token is case-sensitive
                dynamic secret = db.QueryValue(@"SELECT Secret FROM [" + OAuthTokenTableName + "] WHERE Token=@0", token);
                return (string)secret;
            }
        }

        public override void StoreOAuthRequestToken(string requestToken, string requestTokenSecret)
        {
            VerifyInitialized();

            string existingSecret = GetOAuthTokenSecret(requestToken);
            if (existingSecret != null)
            {
                if (existingSecret == requestTokenSecret)
                {
                    // the record already exists
                    return;
                }

                using (var db = ConnectToDatabase())
                {
                    CreateOAuthTokenTableIfNeeded(db);

                    // the token exists with old secret, update it to new secret
                    db.Execute(@"UPDATE [" + OAuthTokenTableName + "] SET Secret = @1 WHERE Token = @0", requestToken, requestTokenSecret);
                }
            }
            else
            {
                using (var db = ConnectToDatabase())
                {
                    CreateOAuthTokenTableIfNeeded(db);

                    // insert new record
                    int insert = db.Execute(@"INSERT INTO [" + OAuthTokenTableName + "] (Token, Secret) VALUES(@0, @1)", requestToken, requestTokenSecret);
                    if (insert != 1)
                    {
                        throw new ProviderException(WebDataResources.SimpleMembership_FailToStoreOAuthToken);
                    }
                }
            }
        }

        /// <summary>
        /// Replaces the request token with access token and secret.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="accessTokenSecret">The access token secret.</param>
        public override void ReplaceOAuthRequestTokenWithAccessToken(string requestToken, string accessToken, string accessTokenSecret)
        {
            VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                CreateOAuthTokenTableIfNeeded(db);

                // insert new record
                db.Execute(@"DELETE FROM [" + OAuthTokenTableName + "] WHERE Token = @0", requestToken);

                // Although there are two different types of tokens, request token and access token,
                // we treat them the same in database records.
                StoreOAuthRequestToken(accessToken, accessTokenSecret);
            }
        }

        /// <summary>
        /// Deletes the OAuth token from the backing store from the database.
        /// </summary>
        /// <param name="token">The token to be deleted.</param>
        public override void DeleteOAuthToken(string token)
        {
            VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                CreateOAuthTokenTableIfNeeded(db);

                // Note that token is case-sensitive
                db.Execute(@"DELETE FROM [" + OAuthTokenTableName + "] WHERE Token=@0", token);
            }
        }

        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            VerifyInitialized();

            int userId = GetUserId(userName);
            if (userId != -1)
            {
                using (var db = ConnectToDatabase())
                {
                    IEnumerable<dynamic> records = db.Query(@"SELECT Provider, ProviderUserId FROM [" + OAuthMembershipTableName + "] WHERE UserId=@0", userId);
                    if (records != null)
                    {
                        var accounts = new List<OAuthAccountData>();
                        foreach (DynamicRecord row in records)
                        {
                            accounts.Add(new OAuthAccountData((string)row["Provider"], (string)row["ProviderUserId"]));
                        }
                        return accounts;
                    }
                }
            }

            return new OAuthAccountData[0];
        }

        /// <summary>
        /// Determines whether there exists a local account (as opposed to OAuth account) with the specified userId.
        /// </summary>
        /// <param name="userId">The user id to check for local account.</param>
        /// <returns>
        ///   <c>true</c> if there is a local account with the specified user id]; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasLocalAccount(int userId)
        {
            VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                dynamic id = db.QueryValue(@"SELECT UserId FROM [" + MembershipTableName + "] WHERE UserId=@0", userId);
                return id != null;
            }
        }
    }
}
