using Antem.Models;
using Antem.Parts;
using Antem.Web.Filters;
using Antem.Web.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Web.Http;

namespace Antem.Web.Controllers.API
{
    [Transaction]
    public class StateController : ApiController
    {
        IRepository<State> repository;

        public StateController(IRepository<State> repo)
        {
            repository = repo;
        }

        // GET api/state
        public IQueryable<State> Get()
        {
            return repository;
        }

        // GET api/state/5
        public StateViewModel Get(int id)
        {
            return Mapper.Map<State, StateViewModel>(repository.Get(id));
        }

        // POST api/state
        public void Post(State state)
        {
            repository.Save(state);
        }

        // PUT api/state/5
        public void Put(State state)
        {
            throw new NotImplementedException();
        }

        // DELETE api/state/5
        public void Delete(int id)
        {
            var state = repository.Get(id);
            repository.Delete(state);
        }
    }
}
