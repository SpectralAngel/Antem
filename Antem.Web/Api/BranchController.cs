using Antem.Models;
using Antem.Parts;
using Antem.Web.Filters;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Antem.Web.Controllers.API
{
    [Transaction]
    public class BranchController : ApiController
    {
        IRepository<Branch> repository;

        public BranchController(IRepository<Branch> repo)
        {
            repository = repo;
        }

        // GET api/branch
        public IQueryable<Branch> Get()
        {
            return repository;
        }

        // GET api/branch/5
        public Branch Get(int id)
        {
            return repository.Get(id);
        }

        // POST api/branch
        public void Post(Branch branch)
        {
            repository.Save(branch);
        }

        // PUT api/branch/5
        public void Put(Branch branch)
        {
            throw new NotImplementedException();
        }

        // DELETE api/branch/5
        public void Delete(int id)
        {
            var branch = repository.Get(id);
            repository.Delete(branch);
        }
    }
}
