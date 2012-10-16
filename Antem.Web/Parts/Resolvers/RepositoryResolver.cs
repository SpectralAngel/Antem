using Antem.Parts;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Antem.Web.Resolvers
{
    /// <summary>
    /// Allows retrieval of entities from a repository by specifying a ViewModel
    /// class as a source and the Model class as the  destination.
    /// </summary>
    /// <typeparam name="TSource">A ViewModel class that contains a member
    /// that represents the primary key of the TDestination that is named 
    /// as the TDestination class name</typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public class RepositoryResolver<TSource, TDestination> :
        ValueResolver<TSource, TDestination> where TDestination : class
    {
        protected IRepository<TDestination> repository;

        // Initializes the internal repository with the provided one.
        public RepositoryResolver(IRepository<TDestination> repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Retrieves a TDestination entity specified in the TSource instance by
        /// using reflection
        /// </summary>
        /// <param name="source">The origin instance to obtain the data from</param>
        /// <returns>A TDestination entity retrieved from the repository</returns>
        protected override TDestination ResolveCore(TSource source)
        {
            var target = typeof(TDestination).Name;
            var member = (int)source.GetType().GetProperty(target).GetValue(source);
            return repository.Get(member);
        }
    }
}
