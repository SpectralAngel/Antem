using Antem.Models;
using Antem.Parts;
using Antem.Web.Models.UI;
using AutoMapper;
using System;
using System.Linq;

namespace Antem.Web.Parts.Resolvers
{
    public class StateResolver : RepositoryResolver<MemberEditModel, Branch>
    {
        IRepository<State> states;

        public StateResolver(IRepository<State> states)
        {
            this.states = states;
        }

        protected override State ResolveCore(MemberEditModel source)
        {
            return states.Get(source.State);
        }
    }
}
