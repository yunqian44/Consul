using Consul.WebApi.IdentityServer.Services;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Consul.WebApi.IdentityServer.Extension;

namespace Consul.WebApi.IdentityServer.AuthHelper
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private IUserService _userService;
        private IRoleService _roleService;
        private IUserRoleService _userRoleService;

        public ResourceOwnerPasswordValidator(IUserService userService,
            IRoleService roleService,
            IUserRoleService userRoleService)
        {
            this._userService = userService;
            this._roleService = roleService;
            this._userRoleService=userRoleService;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = _userService.QueryUserByName(context.UserName);
            if (user.Result == null || user.Result.IsDeleted)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid client credential");
                return Task.CompletedTask;
            }

            var ss = GrantTypes.Implicit.First();

            if (context.Password != user.Result.Password)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "username or password fail");
                return Task.CompletedTask;
            }
            var roleId = _userRoleService.QueryUserRoleList().Result.FirstOrDefault(u=>u.UserId==user.Result.Id)?.RoleId;
           
            var roleName = _userRoleService.QueryUserRoleList().Result.Where(d => d.Id == roleId).Select(d => d.Id).ToArray().ArrayToString(new char[] { ','});
            if (roleId<=0||string.IsNullOrWhiteSpace(roleName))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "net found user role information");
                return Task.CompletedTask;
            }

            context.Result = new GrantValidationResult(
                subject: context.UserName,
                authenticationMethod: GrantTypes.ResourceOwnerPassword.First(),
                claims: new Claim[] {
                        new Claim("Name", context.UserName),
                        new Claim("Id", user.Result.Id.ToString()),
                        new Claim("RealName", user.Result.RealName),
                        new Claim("Roles", roleName)
                }
            );

            return Task.CompletedTask;
        }
    }
}
