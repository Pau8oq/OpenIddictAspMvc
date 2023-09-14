using Microsoft.Owin;
using Owin;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using System;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using OpenIddict.Validation.Owin;
using Microsoft.Owin.Host.SystemWeb;
using System.Web.Http;
using System.Web.Mvc;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client.Owin;

[assembly: OwinStartup(typeof(Resource.Startup))]

namespace Resource
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
                 //.AddClient(options =>
                 //{
                 //    // Note: this sample uses the authorization code and refresh token
                 //    // flows, but you can enable the other flows if necessary.
                 //    options.AllowAuthorizationCodeFlow()
                 //           .AllowRefreshTokenFlow();

                 //    // Register the signing and encryption credentials used to protect
                 //    // sensitive data like the state tokens produced by OpenIddict.
                 //    options.AddDevelopmentEncryptionCertificate()
                 //           .AddDevelopmentSigningCertificate();

                 //    // Register the OWIN host and configure the OWIN-specific options.
                 //    options.UseOwin()
                 //           .EnableRedirectionEndpointPassthrough()
                 //           .EnablePostLogoutRedirectionEndpointPassthrough();


                 //    // Register the System.Net.Http integration and use the identity of the current
                 //    // assembly as a more specific user agent, which can be useful when dealing with
                 //    // providers that use the user agent as a way to throttle requests (e.g Reddit).
                 //    options.UseSystemNetHttp()
                 //           .SetProductInformation(typeof(Startup).Assembly);

                 //    // Add a client registration matching the client application definition in the server project.
                 //    options.AddRegistration(new OpenIddictClientRegistration
                 //    {
                 //        Issuer = new Uri("https://localhost:44385/", UriKind.Absolute),

                 //        ClientId = "test_client",
                 //        ClientSecret = "test_secret",
                 //        Scopes = { Scopes.OfflineAccess, "test_scope" },

                 //        RedirectUri = new Uri("/callback_1/login/local", UriKind.Relative),
                 //        PostLogoutRedirectUri = new Uri("/callback_1/logout/local", UriKind.Relative)
                 //    });
                 //})
                 .AddValidation(options =>
                 {

                     options.SetIssuer("https://localhost:44385/");
                     options.AddAudiences("test_resource");


                     options.UseIntrospection()
                     .SetClientId("test_client")
                     .SetClientSecret("test_secret");

                     options.UseSystemNetHttp();

                     options.AddEncryptionKey(new SymmetricSecurityKey(
                                     Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

                     options.UseOwin();

                 });

            // Create a new Autofac container and import the OpenIddict services.
            var builder = new ContainerBuilder();
            builder.Populate(services);

            // Register the MVC controllers.
            builder.RegisterControllers(typeof(Startup).Assembly);
            builder.RegisterApiControllers(typeof(Startup).Assembly);
           

            var container = builder.Build();

            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver((IContainer)container);


            app.UseAutofacLifetimeScopeInjector(container);

            // Register the two OpenIddict server/validation middleware.
            app.UseMiddlewareFromContainer<OpenIddictValidationOwinMiddleware>();

            // Configure ASP.NET MVC 5.2 to use Autofac when activating controller instances.
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            var configuration = new HttpConfiguration
            {
                DependencyResolver = new AutofacWebApiDependencyResolver(container)
            };

            configuration.MapHttpAttributeRoutes();

            // Configure ASP.NET Web API to use token authentication.
            configuration.Filters.Add(new HostAuthenticationFilter(OpenIddictValidationOwinDefaults.AuthenticationType));

            // Register the Web API/Autofac integration middleware.
            app.UseAutofacWebApi(configuration);
            app.UseWebApi(configuration);
        }
    }
}
