using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using OpenIddict.Abstractions;
using OpenIddict.Client.Owin;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Client.Controllers
{
    public class AuthenticationController : Controller
    {
        // Note: this controller uses the same callback action for all providers
        // but for users who prefer using a different action per provider,
        // the following action can be split into separate actions.
        [HttpGet, Route("~/callback_1/login/{provider}")]
        public async Task<ActionResult> LogInCallback_1()
        {
            var context = HttpContext.GetOwinContext();

            // Retrieve the authorization data validated by OpenIddict as part of the callback handling.
            var result = await context.Authentication.AuthenticateAsync(OpenIddictClientOwinDefaults.AuthenticationType);

            if (result.Identity.IsAuthenticated == true)
            {
                // Build an identity based on the external claims and that will be used to create the authentication cookie.
                //
                // By default, all claims extracted during the authorization dance are available. The claims collection stored
                // in the cookie can be filtered out or mapped to different names depending the claim name or its issuer.
                var claims = result.Identity.Claims;

                var identity = new ClaimsIdentity(claims,
                    authenticationType: CookieAuthenticationDefaults.AuthenticationType,
                    nameType: ClaimTypes.Name,
                    roleType: ClaimTypes.Role);

                // Build the authentication properties based on the properties that were added when the challenge was triggered.
                var properties = new AuthenticationProperties(result.Properties.Dictionary
                    .Where(item =>
                    {
                        if (item.Key == ".redirect")
                        {
                            return true;
                        }
                        else if (item.Key == OpenIddictClientOwinConstants.Tokens.BackchannelAccessToken
                        || item.Key == OpenIddictClientOwinConstants.Tokens.BackchannelIdentityToken
                        || item.Key == OpenIddictClientOwinConstants.Tokens.RefreshToken)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    })
                    .ToDictionary(pair => pair.Key, pair => pair.Value));

                context.Authentication.SignIn(properties, identity);
                return Redirect(properties.RedirectUri ?? "/");
            }
            throw new InvalidOperationException("The external authorization data cannot be used for authentication.");
        }
    }
}