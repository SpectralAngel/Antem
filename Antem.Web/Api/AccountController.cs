using Antem.Models;
using Antem.Parts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Antem.Web.Filters;

namespace Antem.Web.Api
{
    [Transaction]
    public class AccountController : ApiController
    {
        IRepository<SavingAccount> accounts;
        IRepository<Member> members;

        [ImportingConstructor]
        public AccountController(IRepository<SavingAccount> repo,
            IRepository<Member> members)
        {
            accounts = repo;
            this.members = members;
        }

        // GET api/accountapi
        public IQueryable<SavingAccount> Get()
        {
            return accounts;
        }

        // GET api/accountapi/5
        public SavingAccount Get(int id)
        {
            return accounts.Get(id);
        }

        [HttpPost]
        public void NewContributions(Member member)
        {
        }

        // DELETE api/accountapi/5
        public void Delete(int id)
        {
        }
    }
}
