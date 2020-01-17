using Consul.WebApi.IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.Services
{
    public interface IUserRoleService
    {
        #region 01,get userrole listing+Task<List<UserRoleModel>> QueryUserRoleList();
        /// <summary>
        /// get userrole listing
        /// </summary>
        /// <returns></returns>
        Task<List<UserRoleModel>> QueryUserRoleList(); 
        #endregion
    }
}
