using Antem.Models;
using Antem.Parts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Antem.Web.Controllers
{
    public class TownController : ApiController
    {
        IRepository<Town> repository { get; set; }
        IRepository<State> states { get; set; }
        ExportFactory<IUnitOfWork> unitFactory;

        [ImportingConstructor]
        public TownController(IRepository<Town> repo,
            IRepository<State> states,
            ExportFactory<IUnitOfWork> factory)
        {
            repository = repo;
            this.states = states;
            unitFactory = factory;
        }

        // GET api/town
        public IQueryable<Town> Get()
        {
            return repository;
        }

        [HttpGet]
        public IEnumerable<Town> State(int id)
        {
            return repository.Where(t => t.State.Id == id);
        }

        // GET api/town/5
        public Town Get(int id)
        {
            return repository.Get(id);
        }

        // POST api/town
        public void Post(Town town, int state)
        {
            using (var unitOfWork = unitFactory.CreateExport().Value)
            {
                try
                {
                    town.State = states.Get(state);
                    repository.Save(town);
                    unitOfWork.Commit();
                }
                catch (Exception)
                {
                    unitOfWork.Rollback();
                    throw;
                }
            }
        }

        // PUT api/town/5
        public void Put(int id, Town town)
        {
            throw new NotImplementedException();
        }

        // DELETE api/town/5
        public void Delete(int id)
        {
            using (var unitOfWork = unitFactory.CreateExport().Value)
            {
                try
                {
                    var town = repository.Get(id);
                    repository.Delete(town);
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
