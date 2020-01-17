using Consul.WebApi.IdentityServer.Helper;
using Consul.WebApi.IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.Services
{
    public class UserRoleService : IUserRoleService
    {
        #region 01,get userrole listing+async Task<List<UserRoleModel>> QueryUserRoleList()
        /// <summary>
        /// get userrole listing
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserRoleModel>> QueryUserRoleList()
        {
            return await Task.Run(() => { return JsonHelper.ParseFormByJson<List<UserRoleModel>>(GetTableData.GetData(nameof(UserRoleModel))); });
        } 
        #endregion
    }
}
