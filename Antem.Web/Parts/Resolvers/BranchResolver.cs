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
    public class BranchResolver : ValueResolver<MemberEditModel, Branch>
    {
        IRepository<Branch> branches;

        public BranchResolver(IRepository<Branch> towns)
        {
            this.branches = towns;
        }

        protected override Branch ResolveCore(MemberEditModel source)
        {
            return branches.Get(source.Branch);
        }
    }
}
