using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.Models
{
    public class RoleModel
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set;}

        /// <summary>
        /// 角色描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }
    }
}
