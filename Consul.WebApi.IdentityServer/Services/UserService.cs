using Consul.WebApi.IdentityServer.Helper;
using Consul.WebApi.IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.Services
{
    public class UserService : IUserService
    {
        #region 01,get user information based on user name+async Task<List<UserModel>> QueryUserByName(string name)
        /// <summary>
        /// get user information based on user name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<UserModel> QueryUserByName(string name)
        {
            var list= JsonHelper.ParseFormByJson<List<UserModel>>(GetTableData.GetData(nameof(UserModel)));
            return await Task.Run(() => { return list.FirstOrDefault(u => u.UserName.Equals(name)); });
        } 
        #endregion
    }
}
