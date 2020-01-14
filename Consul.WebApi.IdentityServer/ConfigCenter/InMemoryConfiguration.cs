using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.IdentityServer.ConfigCenter
{
    public static class InMemoryConfiguration
    {
        // 这个 Authorization Server 保护了哪些 API （资源）
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                    new ApiResource("consul.webapi.servicea", "Consul.WebApi.ServiceA"),
                    new ApiResource("consul.webapi.serviceb", "Consul.WebApi.ServiceB"),
                    new ApiResource("consul.webapi.servicec", "Consul.WebApi.ServiceC")
            };
        }
        // 哪些客户端 Client（应用） 可以使用这个 Authorization Server
        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                    new Client
                    {
                        ClientId = "serviceA",//定义客户端 Id
                        ClientSecrets = new [] { new Secret("secret".Sha256()) },//Client用来获取token
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,//这里使用的是通过用户名密码和ClientCredentials来换取token的方式. ClientCredentials允许Client只使用ClientSecrets来获取token. 这比较适合那种没有用户参与的api动作
                        AllowedScopes = new [] { "consul.webapi.servicea" }// 允许访问的 API 资源
                    },
                    new Client
                    {
                        ClientId = "serviceB",//定义客户端 Id
                        ClientSecrets = new [] { new Secret("secret".Sha256()) },//Client用来获取token
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,//这里使用的是通过用户名密码和ClientCredentials来换取token的方式. ClientCredentials允许Client只使用ClientSecrets来获取token. 这比较适合那种没有用户参与的api动作
                        AllowedScopes = new [] { "consul.webapi.serviceb" }// 允许访问的 API 资源
                    },
                    new Client
                    {
                        ClientId = "serviceC",//定义客户端 Id
                        ClientSecrets = new [] { new Secret("secret".Sha256()) },//Client用来获取token
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,//这里使用的是通过用户名密码和ClientCredentials来换取token的方式. ClientCredentials允许Client只使用ClientSecrets来获取token. 这比较适合那种没有用户参与的api动作
                        AllowedScopes = new [] { "consul.webapi.servicec" }// 允许访问的 API 资源
                    }
                };
        }
        // 指定可以使用 Authorization Server 授权的 Users（用户）
        public static IEnumerable<TestUser> Users()
        {
            return new[]
            {
                    new TestUser
                    {
                        SubjectId = "1",
                        Username = "yunqian",
                        Password = "qwer1234!"
                    }
            };
        }
    }
}
