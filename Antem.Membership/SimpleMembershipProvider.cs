using Antem.Parts;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Helpers;
using System.Web.Security;
using WebMatrix.WebData;
using WebMatrix.WebData.Resources;
using Antem.Models;
using Antem.Composition.Mvc;

namespace Antem.Membership
{
    public class SimpleMembershipProvider : ExtendedMembershipProvider
    {
        private const int TokenSizeInBytes = 16;
        private readonly MembershipProvider _previousProvider;

        [Import]
        private IRepository<Antem.Models.Membership> membershipRepository { get; set; }

        [Import]
        private IRepository<OAuthMembership> oAuthMembershipRepository { get; set; }

        [Import]
        private IRepository<OauthToken> oAuthTokenRepository { get; set; }

        [Import]
        private ExportFactory<IUnitOfWork> unitOfWork { get; set; }

        [Import]
        private IRepository<User> userRepository { get; set; }

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

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool EnablePasswordReset
        {
            get { return InitializeCalled ? false : PreviousProvider.EnablePasswordReset; }
        }

        // Public properties
        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool EnablePasswordRetrieval
        {
            get { return InitializeCalled ? false : PreviousProvider.EnablePasswordRetrieval; }
        }

        public bool InitializeCalled { get; set; }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int MaxInvalidPasswordAttempts
        {
            get { return InitializeCalled ? Int32.MaxValue : PreviousProvider.MaxInvalidPasswordAttempts; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return InitializeCalled ? 0 : PreviousProvider.MinRequiredNonAlphanumericCharacters; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int MinRequiredPasswordLength
        {
            get { return InitializeCalled ? 0 : PreviousProvider.MinRequiredPasswordLength; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override int PasswordAttemptWindow
        {
            get { return InitializeCalled ? Int32.MaxValue : PreviousProvider.PasswordAttemptWindow; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return InitializeCalled ? MembershipPasswordFormat.Hashed : PreviousProvider.PasswordFormat; }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override string PasswordStrengthRegularExpression
        {
            get { return InitializeCalled ? String.Empty : PreviousProvider.PasswordStrengthRegularExpression; }
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

        // Represents the User created id column, i.e. ID;
        // REVIEW: we could get this from the primary key of UserTable in the future
        public string UserIdColumn { get; set; }

        // represents the User created UserName column, i.e. Email
        public string UserNameColumn { get; set; }

        // represents the User table for the app
        public string UserTableName { get; set; }

        private MembershipProvider PreviousProvider
        {
            get
            {
                if (_previousProvider == null)
                {
                    throw new InvalidOperationException(Resources.Security_InitializeMustBeCalledFirst);
                }
                else
                {
                    return _previousProvider;
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
            if (String.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty username");
            }
            if (String.IsNullOrEmpty(oldPassword))
            {
                throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty oldPassword");
            }
            if (String.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty newPassword");
            }
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var user = GetUser(username);
                if (user == null) { return false; }
                if (!CheckPassword(user, oldPassword)) { return false; }
                return SetPassword(user, newPassword);
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
                    if (String.Equals(accountConfirmationToken, membership.ConfirmationToken, StringComparison.Ordinal))
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

            if (String.IsNullOrEmpty(password))
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            string hashedPassword = Crypto.HashPassword(password);
            if (hashedPassword.Length > 128)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            if (String.IsNullOrEmpty(userName))
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
                var membership = new Antem.Models.Membership()
                {
                    User = user,
                    Password = hashedPassword,
                    PasswordSalt = String.Empty,
                    IsConfirmed = !requireConfirmationToken,
                    ConfirmationToken = token
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

        public override void CreateOrUpdateOAuthAccount(string provider, string providerUserId, string userName)
        {
            VerifyInitialized();
            if (String.IsNullOrEmpty(userName))
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
            }
            var user = GetUser(userName);

            var oauth = GetOAuth(provider, providerUserId);
            using (var unit = unitOfWork.CreateExport().Value)
            {
                if (oauth == null)
                {
                    oauth = new OAuthMembership()
                    {
                        Provider = provider,
                        ProviderUserId = providerUserId,
                        User = user
                    };
                    oAuthMembershipRepository.Add(oauth);
                }
                else
                {
                    oauth.User = user;
                    oAuthMembershipRepository.Update(oauth);
                }
                try
                {
                    unit.Commit();
                }
                catch (Exception)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }
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

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override string CreateUserAndAccount(string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            VerifyInitialized();

            CreateUserRow(userName, values);
            return CreateAccount(userName, password, requireConfirmation);
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override bool DeleteAccount(string userName)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var membership = membershipRepository.Where(m => m.User.Username == userName).Single();
                if (membership == null)
                {
                    return false; // User not found
                }
                membershipRepository.Delete(membership);
                unit.Commit();
                return true;
            }
        }

        public override void DeleteOAuthAccount(string provider, string providerUserId)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var oauth = oAuthMembershipRepository.Where(o => o.Provider == provider && o.ProviderUserId == providerUserId).Single();
                    oAuthMembershipRepository.Delete(oauth);
                    unit.Commit();
                }
                catch (Exception)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }
            }
        }

        /// <summary>
        /// Deletes the OAuth token from the backing store from the database.
        /// </summary>
        /// <param name="token">The token to be deleted.</param>
        public override void DeleteOAuthToken(string token)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var found = oAuthTokenRepository.Where(o => o.Token == token).Single();
                oAuthTokenRepository.Delete(found);
                unit.Commit();
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.DeleteUser(username, deleteAllRelatedData);
            }
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var user = GetUser(username);
                if (user == null)
                {
                    return false; // User not found
                }
                userRepository.Delete(user);
                unit.Commit();
                return true;
            }
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

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
            }
            throw new NotSupportedException();
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            VerifyInitialized();
            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty userName");
            }
            using (var unit = unitOfWork.CreateExport().Value)
            {
                User user = VerifyUserNameHasConfirmedAccount(userName, throwException: true);
                var membership = membershipRepository.Where(m => m.User == user && m.PasswordVerificationTokenExpirationDate > DateTime.UtcNow).First();
                if (membership.PasswordVerificationToken == null)
                {
                    membership.PasswordVerificationToken = GenerateToken();
                    membership.PasswordVerificationTokenExpirationDate = DateTime.UtcNow.AddMinutes(tokenExpirationInMinutesFromNow);
                    membershipRepository.Update(membership);
                    unit.Commit();
                }
                return membership.PasswordVerificationToken;
            }
        }

        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var tokens = oAuthMembershipRepository.Where(o => o.User.Username == userName).ToList();
                var accounts = new List<OAuthAccountData>();
                foreach (var token in tokens)
                {
                    accounts.Add(new OAuthAccountData(token.Provider, token.ProviderUserId));
                }
                return accounts;
            }
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

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override DateTime GetCreateDate(string userName)
        {
            using (var unit = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var membership = membershipRepository.Where(m => m.User.Username == userName).First();
                    return membership.CreateDate;
                }
                catch
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, userName));
                }
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            using (var unit = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var membership = membershipRepository.Where(m => m.User.Username == userName).First();
                    return membership.LastPasswordFailure;
                }
                catch
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, userName));
                }
            }
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

        public OAuthMembership GetOAuth(string provider, string providerUserId)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var oauth = oAuthMembershipRepository.Where(o => o.ProviderUserId == providerUserId && o.Provider == provider).First();
                    return oauth;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override string GetOAuthTokenSecret(string token)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var foundToken = oAuthTokenRepository.Where(o => o.Token == token).Single();
                return foundToken.Secret;
            }
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

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override DateTime GetPasswordChangedDate(string userName)
        {
            using (var unit = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var membership = membershipRepository.Where(m => m.User.Username == userName).First();
                    return membership.PasswordChangeDate;
                }
                catch
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, userName));
                }
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            using (var unit = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var membership = membershipRepository.Where(m => m.User.Username == userName).First();
                    return membership.PasswordFailuresSinceLastSucces;
                }
                catch
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, userName));
                }
            }
        }

        public User GetUser(string username)
        {
            VerifyInitialized();
            using (var unitofwork = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var user = userRepository.Where(u => u.Username == username).Single();
                    return user;
                }
                catch
                {
                    return null;
                }
            }
        }

        // Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            VerifyInitialized();
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
            var user = GetUser(username);
            if (user == null)
            {
                return null;
            }
            return new MembershipUser(this.Name, user.Username, user.Id, null, null, null, true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
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

        public override int GetUserIdFromOAuth(string provider, string providerUserId)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                try
                {
                    var oauth = oAuthMembershipRepository.Where(o => o.ProviderUserId == providerUserId && o.Provider == provider).First();
                    return oauth.User.Id;
                }
                catch (Exception)
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
        public override string GetUserNameByEmail(string email)
        {
            if (!InitializeCalled)
            {
                return PreviousProvider.GetUserNameByEmail(email);
            }
            throw new NotSupportedException();
        }

        public override string GetUserNameFromId(int userId)
        {
            VerifyInitialized();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var user = userRepository.Get(userId);
                if (user != null)
                {
                    return user.Username;
                }
            }
            return null;
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
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var membership = membershipRepository.Where(m => m.User.Id == userId);
                return membership.Count() != 0;
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
                config.Add("description", "Antem SimpleMembership Provider");
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
                    throw new ProviderException(String.Format(CultureInfo.CurrentCulture, Resources.SimpleMembership_ProviderUnrecognizedAttribute, attribUnrecognized));
                }
            }
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override bool IsConfirmed(string userName)
        {
            VerifyInitialized();
            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty userName");
            }

            var user = VerifyUserNameHasConfirmedAccount(userName, throwException: false);
            return user != null;
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
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var token = oAuthTokenRepository.Where(o => o.Token == requestToken).Single();
                oAuthTokenRepository.Delete(token);
                unit.Commit();

                // Although there are two different types of tokens, request token and access token,
                // we treat them the same in database records.
                StoreOAuthRequestToken(accessToken, accessTokenSecret);
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

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            VerifyInitialized();
            if (String.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty newPassword");
            }
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var membership = membershipRepository.Where(m => m.PasswordVerificationToken == token && m.PasswordVerificationTokenExpirationDate > DateTime.UtcNow).First();
                if (membership != null)
                {
                    string hashedPassword = Crypto.HashPassword(newPassword);
                    if (hashedPassword.Length > 128)
                    {
                        throw new ArgumentException(Resources.SimpleMembership_PasswordTooLong);
                    }
                    membership.Password = hashedPassword;
                    membership.PasswordChangeDate = DateTime.UtcNow;
                    membership.PasswordVerificationToken = null;
                    membership.PasswordVerificationTokenExpirationDate = DateTime.MinValue;
                    try
                    {
                        membershipRepository.Update(membership);
                        unit.Commit();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                return false;
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
                using (var unit = unitOfWork.CreateExport().Value)
                {
                    var token = oAuthTokenRepository.Where(o => o.Token == requestToken).Single();
                    token.Secret = requestTokenSecret;
                    oAuthTokenRepository.Update(token);
                    unit.Commit();
                }
            }
            else
            {
                using (var unit = unitOfWork.CreateExport().Value)
                {
                    try
                    {
                        var token = new OauthToken()
                                    {
                                        Token = requestToken,
                                        Secret = requestTokenSecret
                                    };
                        oAuthTokenRepository.Add(token);
                        unit.Commit();
                    }
                    catch (Exception)
                    {
                        throw new ProviderException(Resources.SimpleMembership_FailToStoreOAuthToken);
                    }
                }
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
        public override bool ValidateUser(string username, string password)
        {
            VerifyInitialized();
            if (!InitializeCalled)
            {
                return PreviousProvider.ValidateUser(username, password);
            }
            if (String.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty {0}", "username");
            }
            if (String.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Argument_Cannot_Be_Null_Or_Empty {0}", "password");
            }
            using (var unit = unitOfWork.CreateExport().Value)
            {
                var user = GetUser(username);
                if (user == null)
                {
                    return false;
                }
                else
                {
                    var membership = user.Memberships.First();
                    return CheckPassword(membership, password);
                }
            }
        }

        internal static string GenerateToken(RandomNumberGenerator generator)
        {
            byte[] tokenBytes = new byte[TokenSizeInBytes];
            generator.GetBytes(tokenBytes);
            return HttpServerUtility.UrlTokenEncode(tokenBytes);
        }

        internal bool DeleteUserAndAccountInternal(string userName)
        {
            return (DeleteAccount(userName) && DeleteUser(userName, false));
        }

        internal void VerifyInitialized()
        {
            /*if (!InitializeCalled)
            {
                throw new InvalidOperationException(Resources.Security_InitializeMustBeCalledFirst);
            }*/
            CompositionProvider.Current.SatisfyImports(this);
            InitializeCalled = true;
        }
        private static string GenerateToken()
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                return GenerateToken(prng);
            }
        }

        private bool CheckPassword(User user, string password)
        {
            string hashedPassword = GetHashedPassword(user);
            bool verificationSucceeded = (hashedPassword != null && Crypto.VerifyHashedPassword(hashedPassword, password));
            var membership = user.Memberships.First();
            using (var unit = unitOfWork.CreateExport().Value)
            {
                if (verificationSucceeded)
                {
                    membership.PasswordFailuresSinceLastSucces = 0;
                }
                else
                {
                    if (membership.PasswordFailuresSinceLastSucces != -1)
                    {
                        membership.PasswordFailuresSinceLastSucces++;
                        membership.LastPasswordFailure = DateTime.UtcNow;
                    }
                }
                try
                {
                    membershipRepository.Update(membership);
                    unit.Commit();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return verificationSucceeded;
        }

        private bool CheckPassword(Antem.Models.Membership membership, string password)
        {
            bool verificationSucceeded = (membership.Password != null && Crypto.VerifyHashedPassword(membership.Password, password));
            using (var unit = unitOfWork.CreateExport().Value)
            {
                if (verificationSucceeded)
                {
                    membership.PasswordFailuresSinceLastSucces = 0;
                }
                else
                {
                    if (membership.PasswordFailuresSinceLastSucces != -1)
                    {
                        membership.PasswordFailuresSinceLastSucces++;
                        membership.LastPasswordFailure = DateTime.UtcNow;
                    }
                }
                try
                {
                    membershipRepository.Update(membership);
                    unit.Commit();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return verificationSucceeded;
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
                        if (key.Equals("UserName"))
                        {
                            continue;
                        }
                        var prop = user.GetType().GetProperty(key);
                        if (prop != null && prop.CanWrite)
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
        private string GetHashedPassword(User user)
        {
            var membership = membershipRepository.Where(m => m.User == user).Single();
            return membership.Password;
        }

        private bool SetPassword(User user, string newPassword)
        {
            string hashedPassword = Crypto.HashPassword(newPassword);
            if (hashedPassword.Length > 128)
            {
                throw new ArgumentException(Resources.SimpleMembership_PasswordTooLong);
            }

            using (var unit = unitOfWork.CreateExport().Value)
            {
                var membership = user.Memberships.First();
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
        // Ensures the user exists in the accounts table
        private User VerifyUserNameHasConfirmedAccount(string username, bool throwException)
        {
            var user = GetUser(username);
            if (user == null)
            {
                if (throwException)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, username));
                }
                else
                {
                    return null;
                }
            }
            try
            {
                var membership = user.Memberships.First();
                return user;
            }
            catch
            {
                if (throwException)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoAccountFound, username));
                }
                return null;
            }
        }
    }
}
