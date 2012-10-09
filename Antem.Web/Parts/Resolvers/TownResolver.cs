using Antem.Models;
using Antem.Parts;
using Antem.Web.Models.UI;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Antem.Web.Parts.Resolvers
{
    public class TownResolver : ValueResolver<MemberEditModel, Town>
    {
        IRepository<Town> towns;

        public TownResolver(IRepository<Town> towns)
        {
            this.towns = towns;
        }

        protected override Town ResolveCore(MemberEditModel source)
        {
            return towns.Get(source.Town);
        }
    }
}
