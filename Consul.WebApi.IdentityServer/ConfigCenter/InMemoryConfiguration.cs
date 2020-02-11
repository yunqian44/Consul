using IdentityModel;
using IdentityServer4;
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
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource("name", "姓名", new List<string> { JwtClaimTypes.Name }),
                new IdentityResource("roles", "角色", new List<string> { JwtClaimTypes.Role }),
                new IdentityResource("rolename", "角色名", new List<string> { "rolename" }),
            };
        }

        // 这个 Authorization Server 保护了哪些 API （资源）
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                    new ApiResource("consul.webapi.servicea", "Consul.WebApi.ServiceA"),
                    new ApiResource("consul.webapi.serviceb", "Consul.WebApi.ServiceB"),
                    new ApiResource("consul.webapi.servicec", "Consul.WebApi.ServiceC"),
                    new ApiResource("consul.webapi.serviced", "Consul.WebApi.ServiceD")
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
                        ClientName = "consul.webapi.servicea",
                        ClientSecrets = new [] { new Secret("secret".Sha256()) },//Client用来获取token
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,//这里使用的是通过用户名密码和ClientCredentials来换取token的方式. ClientCredentials允许Client只使用ClientSecrets来获取token. 这比较适合那种没有用户参与的api动作
                        AllowedScopes = new [] { "consul.webapi.servicea", IdentityServerConstants.StandardScopes.OpenId }// 允许访问的 API 资源
                    },
                    new Client
                    {
                        ClientId = "serviceB",//定义客户端 Id
                        ClientName = "consul.webapi.serviceb",
                        ClientSecrets = new [] { new Secret("secret".Sha256()) },//Client用来获取token
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,//这里使用的是通过用户名密码和ClientCredentials来换取token的方式. ClientCredentials允许Client只使用ClientSecrets来获取token. 这比较适合那种没有用户参与的api动作
                        AllowedScopes = new [] { "consul.webapi.serviceb" }// 允许访问的 API 资源
                    },
                    new Client
                    {
                        ClientId = "serviceC",//定义客户端 Id
                        ClientName = "consul.webapi.servicec",
                        ClientSecrets = new [] { new Secret("secret".Sha256()) },//Client用来获取token
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,//这里使用的是通过用户名密码和ClientCredentials来换取token的方式. ClientCredentials允许Client只使用ClientSecrets来获取token. 这比较适合那种没有用户参与的api动作
                        AllowedScopes = new [] { "consul.webapi.servicec" }// 允许访问的 API 资源
                    },
                    new Client
                    {
                        ClientId = "serviceD",//定义客户端 Id
                        ClientName = "consul.webapi.serviced",
                        ClientSecrets = new [] { new Secret("secret".Sha256()) },//Client用来获取token
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,//这里使用的是通过用户名密码和ClientCredentials来换取token的方式. ClientCredentials允许Client只使用ClientSecrets来获取token. 这比较适合那种没有用户参与的api动作
                        AllowedScopes = new [] { "consul.webapi.serviced" }// 允许访问的 API 资源
                    },
                    new Client
                    {
                         ClientId = "consulwebapp",
                         ClientName = "Consul.WebApp",
                         AllowedGrantTypes = GrantTypes.Code,
                         ClientSecrets = new [] { new Secret("secret".Sha256()) },//Client用来获取token
                         RequireConsent = true,
                         RequirePkce = true,
                         //将用户所有的claims包含在IdToken内
                         AlwaysIncludeUserClaimsInIdToken=true,
                         // can return access_token to this client
                         //AllowAccessTokensViaBrowser = true,
                         RedirectUris = {"http://localhost:9014/signin-oidc"},
                         PostLogoutRedirectUris = {"http://localhost:9014/signout-callback-oidc" },
                         AllowedScopes = new List<string>
                         {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.Email,
                            "roles",
                            "rolename",
                            "name"
                         }
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
                        SubjectId = "1001",
                        Username = "huge",
                        Password = "qwer1234!"
                    }
            };
        }
    }
}
