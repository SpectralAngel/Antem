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
    public class StateController : ApiController
    {
        IRepository<State> repository;
        ExportFactory<IUnitOfWork> unitFactory;

        [ImportingConstructor]
        public StateController(IRepository<State> repo,
            ExportFactory<IUnitOfWork> factory)
        {
            repository = repo;
            unitFactory = factory;
        }

        // GET api/state
        public IQueryable<State> Get()
        {
            return repository;
        }

        // GET api/state/5
        public State Get(int id)
        {
            return repository.Get(id);
        }

        // POST api/state
        public void Post(State state)
        {
            using (var unitOfWork = unitFactory.CreateExport().Value)
            {
                try
                {
                    repository.Save(state);
                    unitOfWork.Commit();
                }
                catch (Exception)
                {
                    unitOfWork.Rollback();
                    throw;
                }
            }
        }

        // PUT api/state/5
        public void Put(State state)
        {
            throw new NotImplementedException();
        }

        // DELETE api/state/5
        public void Delete(int id)
        {
            using (var unitOfWork = unitFactory.CreateExport().Value)
            {
                try
                {
                    var state = repository.Get(id);
                    repository.Delete(state);
                    unitOfWork.Commit();
                }
                catch (Exception)
                {
                    unitOfWork.Rollback();
                    throw;
                } 
            }
        }
    }
}
