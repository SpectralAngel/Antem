using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Antem.Web.Models.UI;
using Antem.Models;
using Antem.Composition.Mvc;

namespace Antem.Web
{
    public class AutoMapping
    {
        public void Configure()
        {
            MemberMappings();

            Mapper.AssertConfigurationIsValid();
        }

        private static void MemberMappings()
        {
            Mapper.CreateMap<MemberEditModel, Member>()
                .ForMember(dto => dto.Loans, opt => opt.Ignore())
                .ForMember(dto => dto.Invoices, opt => opt.Ignore())
                .ForMember(dto => dto.Beneficiaries, opt => opt.Ignore())
                .ForMember(dto => dto.Accounts, opt => opt.Ignore())
                .ForMember(dto => dto.PhoneNumbers, opt => opt.Ignore())
                .ForMember(dto => dto.Emails, opt => opt.Ignore())
                .ForMember(dto => dto.Addresses, opt => opt.Ignore())
                .ForMember(dto => dto.Id, opt => opt.Ignore())

                .ForMember(dto => dto.Branch,
                           opt => opt.ResolveUsing<ValueResolver<MemberEditModel, Branch>>()
                               .ConstructedBy(() => CompositionProvider.Current.GetExport<ValueResolver<MemberEditModel, Branch>>()))
                .ForMember(dto => dto.State,
                           opt => opt.ResolveUsing<ValueResolver<MemberEditModel, State>>()
                               .ConstructedBy(() => CompositionProvider.Current.GetExport<ValueResolver<MemberEditModel, State>>()))
                .ForMember(dto => dto.Town,
                           opt => opt.ResolveUsing<ValueResolver<MemberEditModel, Town>>()
                               .ConstructedBy(() => CompositionProvider.Current.GetExport<ValueResolver<MemberEditModel, Town>>()));
        }
    }
}
