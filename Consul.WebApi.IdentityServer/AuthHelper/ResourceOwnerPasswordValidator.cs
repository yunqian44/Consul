using Consul.WebApi.IdentityServer.Services;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.AuthHelper
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private IUserService _userService;

        public ResourceOwnerPasswordValidator(IUserService userService)
        {
            this._userService = userService;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //LoginUser loginUser = null;
            //bool isAuthenticated = _userService.Authenticate(context.UserName, context.Password, out loginUser);
            //if (!isAuthenticated)
            //{
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid client credential");
            //}
            //else
            //{
            //    context.Result = new GrantValidationResult(
            //        subject: context.UserName,
            //        authenticationMethod: "custom",
            //        claims: new Claim[] {
            //            new Claim("Name", context.UserName),
            //            new Claim("Id", loginUser.Id.ToString()),
            //            new Claim("RealName", loginUser.RealName),
            //            new Claim("Email", loginUser.Email)
            //        }
            //    );
            //}
            return Task.CompletedTask;
        }
    }
}
