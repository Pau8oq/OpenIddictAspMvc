using Microsoft.Owin;
using Owin;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using System;
using System.Threading.Tasks;
using Server.Models;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using OpenIddict.Server.Owin;
using OpenIddict.Validation.Owin;
using System.Web.Http;
using System.Web.Mvc;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Owin.Security.Cookies;
using Microsoft.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(Server.Startup))]

namespace Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var services = new ServiceCollection();

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFramework().UseDbContext<ApplicationDbContext>();
                })
                .AddServer(options =>
                {
                    options.AllowAuthorizationCodeFlow()
                            .AllowClientCredentialsFlow()
                            .AllowPasswordFlow()
                            .AllowRefreshTokenFlow();

                    options.SetAuthorizationEndpointUris("/connect/authorize")
                          .SetIntrospectionEndpointUris("/connect/introspect")
                          .SetLogoutEndpointUris("/connect/logout")
                          .SetTokenEndpointUris("/connect/token")
                          .SetUserinfoEndpointUris("/connect/userinfo")
                          .SetVerificationEndpointUris("/connect/verify");

                    options.AddEncryptionKey(new SymmetricSecurityKey(
                                   Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

                    options.AddDevelopmentSigningCertificate();

                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "test_scope");

                    options.RequireProofKeyForCodeExchange();

                    // Register the OWIN host and configure the OWIN-specific options.
                    options.UseOwin()
                           //.EnableUserinfoEndpointPassthrough()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableLogoutEndpointPassthrough()
                           .EnableTokenEndpointPassthrough()
                           .EnableVerificationEndpointPassthrough();
                });
                

            var builder = new ContainerBuilder();
            builder.Populate(services);

            // Register the MVC controllers.
            builder.RegisterControllers(typeof(Startup).Assembly);

            // Register the Web API controllers.
            builder.RegisterApiControllers(typeof(Startup).Assembly);

            var container = builder.Build();

            // Register the Autofac scope injector middleware.
            app.UseAutofacLifetimeScopeInjector(container);

            // Register the cookie middleware responsible for storing the user sessions.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = TimeSpan.FromMinutes(50),
                SlidingExpiration = false
            });

            app.Use(async (context, et) =>
            {

                var t = context.GetOpenIddictClientResponse();

                await et.Invoke();

                if (context.Response.StatusCode == 302)
                {

                }
            });

            // Register the two OpenIddict server/validation middleware.
            app.UseMiddlewareFromContainer<OpenIddictServerOwinMiddleware>();
            //app.UseMiddlewareFromContainer<OpenIddictValidationOwinMiddleware>();

            // Configure ASP.NET MVC 5.2 to use Autofac when activating controller instances.
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Configure ASP.NET MVC 5.2 to use Autofac when activating controller instances
            // and infer the Web API routes using the HTTP attributes used in the controllers.
            var configuration = new HttpConfiguration
            {
                DependencyResolver = new AutofacWebApiDependencyResolver(container)
            };

            configuration.MapHttpAttributeRoutes();

            // Register the Autofac Web API integration and Web API middleware.
            app.UseAutofacWebApi(configuration);
            app.UseWebApi(configuration);

            // Seed the database with the sample client using the OpenIddict application manager.
            // Note: in a real world application, this step should be part of a setup script.
            Task.Run(async delegate
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var context = scope.Resolve<ApplicationDbContext>();
                    context.Database.CreateIfNotExists();

                    var manager = scope.Resolve<IOpenIddictApplicationManager>();

                    var appDescriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "test_client",
                        ClientSecret = "test_secret",
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "MVC client application",
                        RedirectUris =
                        {
                            new Uri("https://localhost:44302/callback_1/login/local")
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("https://localhost:44302/callback_1/logout/local")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,
                            Permissions.Endpoints.Introspection,
                            Permissions.GrantTypes.ClientCredentials,
                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,
                            Permissions.ResponseTypes.Code,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,

                            Permissions.Prefixes.Scope + "test_scope"
                        },
                        Requirements = 
                        {
                         Requirements.Features.ProofKeyForCodeExchange
                        }
                    };

                    var application = await manager.FindByClientIdAsync(appDescriptor.ClientId);

                    if (application != null)
                    {
                        await manager.PopulateAsync(application, appDescriptor);
                        //await manager.UpdateAsync(application);
                        //await manager.CreateAsync(appDescriptor);
                    }
                    else
                    {
                        await manager.CreateAsync(appDescriptor);
                    }
                }
                
            }).GetAwaiter().GetResult();

            Task.Run(async delegate
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var context = scope.Resolve<ApplicationDbContext>();
                    context.Database.CreateIfNotExists();

                    var manager = scope.Resolve<IOpenIddictScopeManager>();

                    var scopeDescriptor = new OpenIddictScopeDescriptor
                    {
                        Name = "test_scope",
                        Resources = { "test_resource" }
                    };

                    var scopeInstance = await manager.FindByNameAsync(scopeDescriptor.Name);

                    if (scopeInstance == null)
                    {
                        await manager.CreateAsync(scopeDescriptor);
                    }
                    else
                    {
                        await manager.UpdateAsync(scopeInstance, scopeDescriptor);
                    }
                }

            }).GetAwaiter().GetResult();

        }
    }
}
