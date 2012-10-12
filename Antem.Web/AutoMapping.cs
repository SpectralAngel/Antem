using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Antem.Web.Models;
using Antem.Models;
using Antem.Composition.Mvc;

namespace Antem.Web
{
    public class AutoMapping
    {
        public void Configure()
        {
            MemberMappings();
            ViewModelMappings();
            Mapper.AssertConfigurationIsValid();
        }

        private static void MemberMappings()
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
                               .ConstructedBy(() => CompositionProvider.Current.GetExport<ValueResolver<MemberViewModel, Branch>>()))
                .ForMember(dto => dto.State,
                           opt => opt.ResolveUsing<ValueResolver<MemberViewModel, State>>()
                               .ConstructedBy(() => CompositionProvider.Current.GetExport<ValueResolver<MemberViewModel, State>>()))
                .ForMember(dto => dto.Town,
                           opt => opt.ResolveUsing<ValueResolver<MemberViewModel, Town>>()
                               .ConstructedBy(() => CompositionProvider.Current.GetExport<ValueResolver<MemberViewModel, Town>>()));
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
                               .ConstructedBy(() => CompositionProvider.Current.GetExport<ValueResolver<TownViewModel, State>>()));

            Mapper.CreateMap<State, StateViewModel>()
                .ForSourceMember(src => src.Towns, opt => opt.Ignore())
                .ForSourceMember(src => src.People, opt => opt.Ignore());
        }
    }
}
