using AutoMapper;
using Beattle.Application.Interfaces;
using Beattle.Identity;
using Beattle.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beattle.SPAUI.ViewModels.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region Authorization and Autentication Map Configurations
            CreateMap<ApplicationUser, UserViewModel>()
                   .ForMember(d => d.Roles, map => map.Ignore());
            CreateMap<UserViewModel, ApplicationUser>()
                .ForMember(d => d.Roles, map => map.Ignore());

            CreateMap<ApplicationUser, UserEditViewModel>()
                .ForMember(d => d.Roles, map => map.Ignore());
            CreateMap<UserEditViewModel, ApplicationUser>()
                .ForMember(d => d.Roles, map => map.Ignore());

            CreateMap<ApplicationUser, UserPatchViewModel>()
                .ReverseMap();

            CreateMap<ApplicationRole, RoleViewModel>()
                .ForMember(d => d.Permissions, map => map.MapFrom(s => s.Claims))
                .ForMember(d => d.UsersCount, map => map.MapFrom( s =>  s.Users == null ? 0 : s.Users.Count))
                .ReverseMap();
            CreateMap<RoleViewModel, ApplicationRole>();

            CreateMap<IdentityRoleClaim<string>, ClaimViewModel>()
                .ForMember(d => d.Type, map => map.MapFrom(s => s.ClaimType))
                .ForMember(d => d.Value, map => map.MapFrom(s => s.ClaimValue))
                .ReverseMap();

            CreateMap<Authorization, AuthorizationViewModel>()
                .ReverseMap();

            CreateMap<IdentityRoleClaim<string>, AuthorizationViewModel>()
                .ConvertUsing(s => Mapper.Map<AuthorizationViewModel>(AuthorizationManager.GetByValue(s.ClaimValue)));
            #endregion

            #region Application & Business logic Map Configurations
            //CreateMap<Product, ProductViewModel>()
            //    .ReverseMap();
            #endregion

        }

    }

}
