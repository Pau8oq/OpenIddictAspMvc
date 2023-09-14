using Microsoft.Owin.Security;
using OpenIddict.Client.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Client.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult LogIn(string provider, string returnUrl)
        {
            var context = HttpContext.GetOwinContext();

           
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                // Note: when only one client is registered in the client options,
                // specifying the issuer URI or the provider name is not required.
                [OpenIddictClientOwinConstants.Properties.ProviderName] = provider
            })
            {
                // Only allow local return URLs to prevent open redirect attacks.
                RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
            };

            // Ask the OpenIddict client middleware to redirect the user agent to the identity provider.
            context.Authentication.Challenge(properties, OpenIddictClientOwinDefaults.AuthenticationType);
            return new EmptyResult();
            
        }
    }
}