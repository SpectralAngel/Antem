using Antem.Models;
using Antem.Parts;
using Antem.Web.Filters;
using Antem.Web.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Antem.Web.Controllers.API
{
    [Transaction]
    public class TownController : ApiController
    {
        IRepository<Town> repository { get; set; }

        public TownController(IRepository<Town> repo,
            IRepository<State> states)
        {
            repository = repo;
        }

        [HttpGet]
        public IEnumerable<TownViewModel> State(int id)
        {
            return Mapper.Map<IEnumerable<Town>, IEnumerable<TownViewModel>>(repository.Where(t => t.State.Id == id));
        }

        // GET api/town/5
        public TownViewModel Get(int id)
        {
            return Mapper.Map<Town, TownViewModel>(repository.Get(id));
        }

        // POST api/town
        public void Post(TownViewModel town)
        {
            Mapper.Map<Town>(town);
            repository.Save(town);
        }

        // PUT api/town/5
        public void Put(int id, Town town)
        {
            throw new NotImplementedException();
        }

        // DELETE api/town/5
        public void Delete(int id)
        {
            var town = repository.Get(id);
            repository.Delete(town);
        }
    }
}
