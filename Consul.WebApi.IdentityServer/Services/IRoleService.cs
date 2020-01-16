using Consul.WebApi.IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.Services
{
    public interface IRoleService
    {
        #region 01,get role listing+Task<List<RoleModel>> QueryRoleList(); 
        /// <summary>
        /// get role listing
        /// </summary>
        /// <returns></returns>
        Task<List<RoleModel>> QueryRoleList(); 
        #endregion
    }
}
