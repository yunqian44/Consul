using Consul.WebApi.IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.Services
{
    public interface IUserService
    {
        #region 01,get user information based on user name+Task<List<UserModel>> QueryUserByName(string name);
        /// <summary>
        /// get user information based on user name
        /// </summary>
        /// <param name="name">name</param>
        /// <returns></returns>
        Task<List<UserModel>> QueryUserByName(string name); 
        #endregion
    }
}
