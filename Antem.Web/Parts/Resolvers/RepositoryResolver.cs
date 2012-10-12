using Antem.Parts;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Antem.Web.Parts.Resolvers
{
    public abstract class RepositoryResolver<TSource, TDestination> : ValueResolver<TSource, TDestination> where TDestination : class
    {
        protected IRepository<TDestination> repository;

        public RepositoryResolver(IRepository<TDestination> repository)
        {
            this.repository = repository;
        }

        protected override TDestination ResolveCore(TSource source)
        {
            var target = typeof(TDestination).Name;
            var member = (int)source.GetType().GetProperty(target).GetValue(source);
            return repository.Get(member);
        }
    }
}
