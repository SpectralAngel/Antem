using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Antem.Web.Models;
using Antem.Models;
using Antem.Composition.Mvc;
using Antem.Web.Resolvers;
using Antem.Parts;

namespace Antem.Web
{
    /// <summary>
    /// Helps configuring the various mappings used by AutoMapper to convert
    /// between the various ViewModels and Models used in the application
    /// </summary>
    public class AutoMapping
    {
        public void Configure()
        {
            MemberMappings();
            ViewModelMappings();
            Mapper.AssertConfigurationIsValid();
        }

        /// <summary>
        /// Configures the various mappers where <see cref="Member"/> is involved
        /// </summary>
        public static void MemberMappings()
        {
            Mapper.CreateMap<MemberViewModel, Member>()
                .ForMember(dto => dto.Loans, opt => opt.Ignore())
                .ForMember(dto => dto.Invoices, opt => opt.Ignore())
                .ForMember(dto => dto.Beneficiaries, opt => opt.Ignore())
                .ForMember(dto => dto.Accounts, opt => opt.Ignore())
                .ForMember(dto => dto.PhoneNumbers, opt => opt.Ignore())
                .ForMember(dto => dto.Emails, opt => opt.Ignore())
                .ForMember(dto => dto.Addresses, opt => opt.Ignore())
                .ForMember(dto => dto.Id, opt => opt.Ignore())

                .ForMember(dto => dto.Branch,
                           opt => opt.ResolveUsing<ValueResolver<MemberViewModel, Branch>>()
                            .ConstructedBy(() =>new RepositoryResolver<MemberViewModel, Branch>(
                                CompositionProvider.Current.GetExport<IRepository<Branch>>())))
                .ForMember(dto => dto.State,
                           opt => opt.ResolveUsing<ValueResolver<MemberViewModel, State>>()
                            .ConstructedBy(() => new RepositoryResolver<MemberViewModel, State>(
                                CompositionProvider.Current.GetExport<IRepository<State>>())))
                .ForMember(dto => dto.Town,
                           opt => opt.ResolveUsing<ValueResolver<MemberViewModel, Town>>()
                            .ConstructedBy(() => new RepositoryResolver<MemberViewModel, Town>(
                                CompositionProvider.Current.GetExport<IRepository<Town>>())));
        }

        private static void ViewModelMappings()
        {
            Mapper.CreateMap<Town, TownViewModel>()
                .ForSourceMember(src => src.People, opt => opt.Ignore())
                .ForMember(dto => dto.State, opt => opt.MapFrom(src => src.State.Id));
            Mapper.CreateMap<TownViewModel, Town>()
                .ForMember(dto => dto.Id, opt => opt.Ignore())
                .ForMember(dto => dto.People, opt => opt.Ignore())

                .ForMember(dto => dto.State,
                           opt => opt.ResolveUsing<ValueResolver<TownViewModel, State>>()
                               .ConstructedBy(() => new RepositoryResolver<TownViewModel, State>(
                                CompositionProvider.Current.GetExport<IRepository<State>>())));

            Mapper.CreateMap<State, StateViewModel>()
                .ForSourceMember(src => src.Towns, opt => opt.Ignore())
                .ForSourceMember(src => src.People, opt => opt.Ignore());
        }
    }
}
