using Microsoft.Owin;
using Owin;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using System;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;
using System.Web.Mvc;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Client.Models;
using Microsoft.Owin.Host.SystemWeb;
using OpenIddict.Client;
using Microsoft.Owin.Security.Cookies;
using OpenIddict.Client.Owin;
using System.Net;

[assembly: OwinStartup(typeof(Client.Startup))]

namespace Client
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var services = new ServiceCollection();


            services.AddOpenIddict()
                //.AddCore(options =>
                //{
                //    options.UseEntityFramework()
                //           .UseDbContext<ApplicationDbContext>();
                //})
                .AddClient(options =>
                {
                    // Note: this sample uses the authorization code and refresh token
                    // flows, but you can enable the other flows if necessary.
                    options.AllowAuthorizationCodeFlow()
                           .AllowRefreshTokenFlow();

                    // Register the signing and encryption credentials used to protect
                    // sensitive data like the state tokens produced by OpenIddict.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    // Register the OWIN host and configure the OWIN-specific options.
                    options.UseOwin()
                           .EnableRedirectionEndpointPassthrough()
                           .EnablePostLogoutRedirectionEndpointPassthrough();


                    // Register the System.Net.Http integration and use the identity of the current
                    // assembly as a more specific user agent, which can be useful when dealing with
                    // providers that use the user agent as a way to throttle requests (e.g Reddit).
                    options.UseSystemNetHttp()
                           .SetProductInformation(typeof(Startup).Assembly);

                    // Add a client registration matching the client application definition in the server project.
                    options.AddRegistration(new OpenIddictClientRegistration
                    {
                        Issuer = new Uri("https://localhost:44385/", UriKind.Absolute),

                        ClientId = "test_client",
                        ClientSecret = "test_secret",
                        Scopes = { Scopes.OfflineAccess, "test_scope" },

                        RedirectUri = new Uri("/callback_1/login/local", UriKind.Relative),
                        PostLogoutRedirectUri = new Uri("/callback_1/logout/local", UriKind.Relative)
                    });

                    options.DisableTokenStorage();
                });

            // Create a new Autofac container and import the OpenIddict services.
            var builder = new ContainerBuilder();
            builder.Populate(services);

            // Register the MVC controllers.
            builder.RegisterControllers(typeof(Startup).Assembly);

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

            // Register the OpenIddict middleware.
            app.UseMiddlewareFromContainer<OpenIddictClientOwinMiddleware>();

            // Configure ASP.NET MVC 5.2 to use Autofac when activating controller instances.
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Create the database used by the OpenIddict client stack to store tokens.
            // Note: in a real world application, this step should be part of a setup script.
            //Task.Run(async delegate
            //{
            //    using(var scope = container.BeginLifetimeScope())
            //    {
            //        var context = scope.Resolve<ApplicationDbContext>();
            //        context.Database.CreateIfNotExists();
            //    }
                
            //}).GetAwaiter().GetResult();
        }
    }
}
