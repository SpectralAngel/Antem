using Antem.Models;
using Antem.Parts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Antem.Web.Controllers.API
{
    public class BranchController : ApiController
    {
        IRepository<Branch> repository;
        ExportFactory<IUnitOfWork> factory;

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
            using (var unit = factory.CreateExport().Value)
            {
                try
                {
                    repository.Save(branch);
                    unit.Commit();
                }
                catch (Exception)
                {
                    unit.Rollback();
                    throw;
                }
            }
        }

        // PUT api/branch/5
        public void Put(Branch branch)
        {
            throw new NotImplementedException();
        }

        // DELETE api/branch/5
        public void Delete(int id)
        {
            using (var unit = factory.CreateExport().Value)
            {
                try
                {
                    var branch = repository.Get(id);
                    repository.Delete(branch);
                    unit.Commit();
                }
                catch (Exception)
                {
                    unit.Rollback();
                    throw;
                }
            }
        }
    }
}
