using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using OpenIddict.Abstractions;
using OpenIddict.Client.Owin;
using OpenIddict.Server.Owin;
using OpenIddict.Validation.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Server.Controllers
{
    public class UserinfoController : Controller
    {
        //[MyAuthorize(AuthSchemes = OpenIddictServerOwinDefaults.AuthenticationType)]
        //[System.Web.Http.HostAuthentication(OpenIddictServerOwinDefaults.AuthenticationType)]
        [HttpGet, HttpPost, Route("~/connect/userinfo")]
        public async Task<ActionResult> Userinfo()
        {
            var request = HttpContext.GetOwinContext();
            var userId = request.Authentication.User.GetClaims(Claims.Subject);


            if (userId == null)
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerOwinConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerOwinConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists."
                });

                request.Authentication.Challenge(properties, OpenIddictServerOwinDefaults.AuthenticationType);

                return new EmptyResult();
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
                [Claims.Subject] = userId   
            };

            return Json(claims);
        }
    }
}