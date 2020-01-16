using Consul.WebApi.IdentityServer.Helper;
using Consul.WebApi.IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.Services
{
    public class RoleService : IRoleService
    {
        #region 01,get role listing+async Task<List<RoleModel>> QueryRoleList()
        /// <summary>
        /// get role listing
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleModel>> QueryRoleList()
        {
            return await Task.Run(() => { return JsonHelper.ParseFormByJson<List<RoleModel>>(GetTableData.GetData(nameof(RoleModel))); });
        } 
        #endregion
    }
}
