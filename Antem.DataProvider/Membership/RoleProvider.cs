/*
//Software usage License : Fluent Nhibernate membership and role provider
//--------------------------------------------------
//Copyright (c) 2010, Suhel Shah (suhel.shah@gmail.com)
//All rights reserved.

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

//1.  Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2.  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3.  The name of the Author may not be used to endorse or promote products derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
//BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
//OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
//PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
//OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
//POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Data;
using System.Data.Odbc;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using NHibernate;
using Zangetsu.Models;
using Zangetsu.Contracts;
using System.Web.Mvc;
using Zangetsu.NHibernateProvider;

namespace Zangetsu.Membership
{
    public sealed class RoleProvider : System.Web.Security.RoleProvider
    {
        #region private
        private string eventSource = "FNHRoleProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";
        private string connectionString;
        private string _applicationName;

        #endregion

        #region Properties 

        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public bool WriteExceptionsToEventLog { get; set; }
        #endregion

        #region Helper Functions
        // A helper function to retrieve config values from the configuration file
        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = exceptionMessage + "\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }
        #endregion

        #region Private Methods
        //get a role by name
        private Roles GetRole(string rolename)
        {
            Roles role = null;
            var iRepository = DependencyResolver.Current.GetService<IRepository<Roles>>();
            var roleRepository = iRepository as Repository<Roles>;
            
            using(var unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>())
            {
                try
                {
                    role = roleRepository.CreateCriteria()
                        .Add(NHibernate.Criterion.Restrictions.Eq("RoleName", rolename))
                        .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                        .UniqueResult<Roles>();

                    //just to lazy init the collection, otherwise get the error 
                    //NHibernate.LazyInitializationException: failed to lazily initialize a collection, no session or session was closed
                    IList<Users> us = role.UsersInRole;

                }
                catch (Exception e)
                {
                    if (WriteExceptionsToEventLog)
                        WriteToEventLog(e, "GetRole");
                    else
                        throw e;
                }
            }
            return role;
        }

        #endregion

        #region Public Methods 
        //initializes the FNH role provider
        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.

            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "FNHRoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Sample Fluent Nhibernate Role provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            _applicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            WriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));
        }

        //adds a user collection toa roles collection
        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            Users usr = null; 
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                    throw new ProviderException(String.Format("Role name {0} not found.",rolename ));
            }

            foreach (string username in usernames)
            {
                if (username.Contains(","))
                    throw new ArgumentException(String.Format("User names {0} cannot contain commas.", username));
                //is user not exiting //throw exception

                foreach (string rolename in rolenames)
                {
                    if (IsUserInRole(username, rolename))
                        throw new ProviderException(String.Format("User {0} is already in role {1}.", username, rolename));
                }
            }
            var iRepository = DependencyResolver.Current.GetService<IRepository<Roles>>();
            var roleRepository = iRepository as Repository<Roles>;
            var uRepo = DependencyResolver.Current.GetService<IRepository<Users>>();
            var userRepository = uRepo as Repository<Users>;
            
            using(var unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>())
            {
                try
                {
                    foreach (string username in usernames)
                    {
                        foreach (string rolename in rolenames)
                        {
                            //get the user
                            usr = userRepository.CreateCriteria()
                                        .Add(NHibernate.Criterion.Restrictions.Eq("Username", username))
                                        .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                        .UniqueResult<Users>();

                            if (usr != null)
                            {
                                //get the role first from db
                                Roles role = roleRepository.CreateCriteria()
                                        .Add(NHibernate.Criterion.Restrictions.Eq("RoleName", rolename))
                                        .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                        .UniqueResult<Roles>();

                                //Roles role = GetRole(rolename);
                                usr.AddRole(role);
                            }
                        }
                        userRepository.Save(usr);
                    }
                    unitOfWork.Commit();
                }
                catch (Exception e)
                {
                    if (WriteExceptionsToEventLog)
                        WriteToEventLog(e, "AddUsersToRoles");
                    else
                        throw e;
                }
            }
        }

        //create  a new role with a given name
        public override void CreateRole(string rolename)
        {
            if (rolename.Contains(","))
                throw new ArgumentException("Role names cannot contain commas.");

            if (RoleExists(rolename))
                throw new ProviderException("Role name already exists.");

            var repository = DependencyResolver.Current.GetService<IRepository<Roles>>();

            using (var unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>())
            {
                try
                {
                    Roles  role = new Roles();
                    role.ApplicationName = this.ApplicationName;
                    role.RoleName = rolename;
                    repository.Save(role);
                    unitOfWork.Commit();
                }
                catch (OdbcException e)
                {
                    if (WriteExceptionsToEventLog)
                        WriteToEventLog(e, "CreateRole");
                    else
                        throw e;
                }
            }
        }

        //delete a role with given name
        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            bool deleted = false;
            if (!RoleExists(rolename))
                throw new ProviderException("Role does not exist.");
            
            if (throwOnPopulatedRole && GetUsersInRole(rolename).Length > 0)
                throw new ProviderException("Cannot delete a populated role.");
            var repository = DependencyResolver.Current.GetService<IRepository<Roles>>();

            using (var unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>())
            {
                try
                {
                    Roles role = GetRole(rolename);
                    repository.Delete(role);

                }
                catch (OdbcException e)
                {
                    if (WriteExceptionsToEventLog)
                    {
                        WriteToEventLog(e, "DeleteRole");
                        return deleted;
                    }
                    else
                        throw e;
                }
            }
        
            return deleted;
        }

        //get an array of all the roles
        public override string[] GetAllRoles()
        {
            StringBuilder sb = new StringBuilder();
            var iRepository = DependencyResolver.Current.GetService<IRepository<Roles>>();
            var roleRepository = iRepository as Repository<Roles>;

            using (var unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>())
            {
                try
                {
                    IList<Roles> allroles = roleRepository.CreateCriteria()
                                    .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                    .List<Roles>();

                    foreach (Roles r in allroles)
                    {
                        sb.Append(r.RoleName + ",");
                    }
                }
                catch (Exception e)
                {
                    if (WriteExceptionsToEventLog)
                        WriteToEventLog(e, "GetAllRoles");
                    else
                        throw e;
                }
            }

            if (sb.Length > 0)
            {
                // Remove trailing comma.
                sb.Remove(sb.Length - 1, 1);
                return sb.ToString().Split(',');
            }

            return new string[0];
        }

        //Get roles for a user by username
        public override string[] GetRolesForUser(string username)
        {
            Users usr = null;
            IList<Roles> usrroles = null;
            StringBuilder sb = new StringBuilder();
            var iRepository = DependencyResolver.Current.GetService<IRepository<Roles>>();
            var roleRepository = iRepository as Repository<Roles>;
            var uRepo = DependencyResolver.Current.GetService<IRepository<Users>>();
            var userRepository = uRepo as Repository<Users>;
            using (var unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>())
            {
                try
                {
                    usr = userRepository.CreateCriteria()
                                    .Add(NHibernate.Criterion.Restrictions.Eq("Username", username))
                                    .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                    .UniqueResult<Users>();

                    if (usr != null)
                    {
                        usrroles = usr.Roles;
                        foreach (Roles r in usrroles)
                        {
                            sb.Append(r.RoleName + ",");
                        }
                    }
                }
                catch (Exception e)
                {
                    if (WriteExceptionsToEventLog)
                        WriteToEventLog(e, "GetRolesForUser");
                    else
                        throw e;
                }
            }

            if (sb.Length > 0)
            {
                // Remove trailing comma.
                sb.Remove(sb.Length - 1, 1);
                return sb.ToString().Split(',');
            }

            return new string[0];
        }
        //Get users in a givenrolename
        public override string[] GetUsersInRole(string rolename)
        {
            StringBuilder sb = new StringBuilder();
            var iRepository = DependencyResolver.Current.GetService<IRepository<Roles>>();
            var roleRepository = iRepository as Repository<Roles>;
            using (var unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>())
            {
                try
                {
                    Roles role = roleRepository.CreateCriteria()
                                    .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                    .Add(NHibernate.Criterion.Restrictions.Eq("RoleName", rolename))
                                    .UniqueResult<Roles>();

                    IList<Users> usrs = role.UsersInRole;

                    foreach (Users u in usrs)
                    {
                        sb.Append(u.Username + ",");
                    }
                }
                catch (Exception e)
                {
                    if (WriteExceptionsToEventLog)
                        WriteToEventLog(e, "GetUsersInRole");
                    else
                        throw e;
                }
            }
            if (sb.Length > 0)
            {
                // Remove trailing comma.
                sb.Remove(sb.Length - 1, 1);
                return sb.ToString().Split(',');
            }

            return new string[0];
        }

        //determine is a user has a given role
        public override bool IsUserInRole(string username, string rolename)
        {
            bool userIsInRole = false;
            Users usr = null;
            IList<Roles> usrroles = null;
            StringBuilder sb = new StringBuilder();
            var uRepo = DependencyResolver.Current.GetService<IRepository<Users>>();
            var userRepository = uRepo as Repository<Users>;
            using (var unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>())
            {
                try
                {
                    usr = userRepository.CreateCriteria()
                                    .Add(NHibernate.Criterion.Restrictions.Eq("Username", username))
                                    .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                    .UniqueResult<Users>();

                    if (usr != null)
                    {
                        usrroles = usr.Roles;
                        foreach (Roles r in usrroles)
                        {
                            if (r.RoleName.Equals(rolename))
                            {
                                userIsInRole = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (WriteExceptionsToEventLog)
                        WriteToEventLog(e, "IsUserInRole");
                    else
                        throw e;
                }
            }
            return userIsInRole;            
        }

        //remeove users from roles
        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            Users usr = null;
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                    throw new ProviderException(String.Format("Role name {0} not found.",rolename));
            }

            foreach (string username in usernames)
            {
                foreach (string rolename in rolenames)
                {
                    if (!IsUserInRole(username, rolename))
                        throw new ProviderException(String.Format("User {0} is not in role {1}.", username, rolename));
                }
            }

            //get user , get his roles , the remove the role and save   
            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (string username in usernames)
                        {
                            usr = session.CreateCriteria(typeof(Users))
                                .Add(NHibernate.Criterion.Restrictions.Eq("Username", username))
                                .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                .UniqueResult<Users>();

                           var  rolestodelete = new List<Roles>() ; 
                            foreach (string rolename in rolenames)
                            {
                                IList<Roles> roles = usr.Roles;  
                                foreach (Roles r in roles)
                                {
                                    if (r.RoleName.Equals(rolename))
                                        rolestodelete.Add(r);  
                                        
                                }
                            }
                            foreach (Roles rd in rolestodelete)
                                usr.RemoveRole(rd);
                            
                            
                            session.SaveOrUpdate(usr); 
                        }
                       transaction.Commit();
                    }
                    catch (OdbcException e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "RemoveUsersFromRoles");
                        else
                            throw e;
                    }
                }
            }
 
        }

        //boolen to check if a role exists given a role name
        public override bool RoleExists(string rolename)
        {
            bool exists = false;

            StringBuilder sb = new StringBuilder();
            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        Roles role = session.CreateCriteria(typeof(Roles))
                                            .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                            .Add(NHibernate.Criterion.Restrictions.Eq("RoleName", rolename))
                                            .UniqueResult<Roles>();
                        if (role != null)
                            exists = true;

                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "RoleExists");
                        else
                            throw e;
                    }
                }
            }
            return exists;
        }

        //find users that beloeng to a particular role , given a username, Note : does not do a LIke search
        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            StringBuilder sb = new StringBuilder();
            using (ISession session = SessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        Roles role = session.CreateCriteria(typeof(Roles))
                                        .Add(NHibernate.Criterion.Restrictions.Eq("ApplicationName", this.ApplicationName))
                                        .Add(NHibernate.Criterion.Restrictions.Eq("RoleName", this.ApplicationName))
                                        .UniqueResult<Roles>();

                        IList<Users> users =  role.UsersInRole;
                        if (users != null)
                        {
                            foreach (Users u in users)
                            {
                                if(String.Compare(u.Username, usernameToMatch, true) == 0)
                                    sb.Append(u.Username +",");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (WriteExceptionsToEventLog)
                            WriteToEventLog(e, "FindUsersInRole");
                        else
                            throw e;
                    }
                }
                if (sb.Length > 0)
                {
                    // Remove trailing comma.
                    sb.Remove(sb.Length - 1, 1);
                    return sb.ToString().Split(',');
                }
                return new string[0];
            }
        }

        #endregion
    }
}
