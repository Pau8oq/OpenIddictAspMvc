using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using OpenIddict.Abstractions;
using OpenIddict.Client.Owin;
using OpenIddict.Server.Owin;
using Owin;
using static System.Net.Mime.MediaTypeNames;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;


namespace Server.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthorizationController(IOpenIddictScopeManager scopeManager)
        {
            _scopeManager = scopeManager;
        }

        [HttpGet, Route("~/connect/authorize")]
        public async Task<ActionResult> Authorize()
        {
            var context = HttpContext.GetOwinContext();
            var request = context.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

           
            var result = await context.Authentication.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationType);

            if (result?.Identity == null || (request.MaxAge != null && result.Properties?.IssuedUtc != null &&
                DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
            {

                var promt = string.Join(" ", request.GetPrompts().Remove(Prompts.None));

                //var parameters = Request.Form.Count > 0 ?
                //    Request.Form.Where()

                context.Authentication.Challenge(CookieAuthenticationDefaults.AuthenticationType);

                return new EmptyResult();
            }

            var identity = new ClaimsIdentity(
                        authenticationType: OpenIddictServerOwinDefaults.AuthenticationType,
                        nameType: Claims.Name,
                        roleType: Claims.Role);


            // Add the claims that will be persisted in the tokens.
            identity.SetClaim(Claims.Subject, result.Identity.GetClaim(Claims.Subject))
                    .SetClaim(Claims.Email, "admin@email")
                    .SetClaim(Claims.Name, result.Identity.GetClaim(Claims.Name))
                    .SetClaim(Claims.Role, result.Identity.GetClaim(Claims.Role));

            identity.SetScopes(request.GetScopes());
            identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

            context.Authentication.SignIn(identity);

            return new EmptyResult();
        }

        [HttpPost, Route("~/connect/token")]
        public async Task<ActionResult> Exchange()
        {
            var context = HttpContext.GetOwinContext();

            var request = context.GetOpenIddictServerRequest() ??
                 throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsClientCredentialsGrantType())
            {

                var identity = new ClaimsIdentity(authenticationType: OpenIddictServerOwinDefaults.AuthenticationType);
                identity.SetClaim(Claims.Subject, request.ClientId);
                identity.SetScopes(request.GetScopes());

                // Ask OpenIddict to issue the appropriate access/identity tokens.
                context.Authentication.SignIn(identity);

                return new EmptyResult();
            }

            if (request.IsRefreshTokenGrantType() || request.IsAuthorizationCodeGrantType())
            {
                var result = await context.Authentication.AuthenticateAsync(OpenIddictServerOwinDefaults.AuthenticationType);
                var userId = result.Identity.GetClaims(Claims.Subject);

                var identity = new ClaimsIdentity(
                    authenticationType: OpenIddictServerOwinDefaults.AuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);



                identity.SetClaim(Claims.Subject, result.Identity.GetClaim(Claims.Subject))
                    .SetClaim(Claims.Email, "admin@gmail")
                    .SetClaim(Claims.Name, result.Identity.GetClaim(Claims.Name))
                    .SetClaim(Claims.Role, result.Identity.GetClaim(Claims.Role));

                
                identity.SetScopes(request.GetScopes());
                identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

                context.Authentication.SignIn(identity);

                return new EmptyResult();
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        // Note: this controller uses the same callback action for all providers
        // but for users who prefer using a different action per provider,
        // the following action can be split into separate actions.
        [AcceptVerbs("GET", "POST"), Route("~/callback/login/{provider}")]
        public async Task<ActionResult> LogInCallback()
        {
            var context = HttpContext.GetOwinContext();

            // Retrieve the authorization data validated by OpenIddict as part of the callback handling.
            var result = await context.Authentication.AuthenticateAsync(OpenIddictClientOwinDefaults.AuthenticationType);

            // Multiple strategies exist to handle OAuth 2.0/OpenID Connect callbacks, each with their pros and cons:
            //
            //   * Directly using the tokens to perform the necessary action(s) on behalf of the user, which is suitable
            //     for applications that don't need a long-term access to the user's resources or don't want to store
            //     access/refresh tokens in a database or in an authentication cookie (which has security implications).
            //     It is also suitable for applications that don't need to authenticate users but only need to perform
            //     action(s) on their behalf by making API calls using the access token returned by the remote server.
            //
            //   * Storing the external claims/tokens in a database (and optionally keeping the essential claims in an
            //     authentication cookie so that cookie size limits are not hit). For the applications that use ASP.NET
            //     Core Identity, the UserManager.SetAuthenticationTokenAsync() API can be used to store external tokens.
            //
            //     Note: in this case, it's recommended to use column encryption to protect the tokens in the database.
            //
            //   * Storing the external claims/tokens in an authentication cookie, which doesn't require having
            //     a user database but may be affected by the cookie size limits enforced by most browser vendors
            //     (e.g Safari for macOS and Safari for iOS/iPadOS enforce a per-domain 4KB limit for all cookies).
            //
            //     Note: this is the approach used here, but the external claims are first filtered to only persist
            //     a few claims like the user identifier. The same approach is used to store the access/refresh tokens.

            // Important: if the remote server doesn't support OpenID Connect and doesn't expose a userinfo endpoint,
            // result.Principal.Identity will represent an unauthenticated identity and won't contain any claim.
            //
            // Such identities cannot be used as-is to build an authentication cookie in ASP.NET (as the
            // antiforgery stack requires at least a name claim to bind CSRF cookies to the user's identity) but
            // the access/refresh tokens can be retrieved using result.Properties.GetTokens() to make API calls.
            if (result.Identity.IsAuthenticated == false)
            {
                throw new InvalidOperationException("The external authorization data cannot be used for authentication.");
            }

            // Build an identity based on the external claims and that will be used to create the authentication cookie.
            //
            // By default, all claims extracted during the authorization dance are available. The claims collection stored
            // in the cookie can be filtered out or mapped to different names depending the claim name or its issuer.
            var claims = result.Identity.Claims.Where(claim => claim.Type is ClaimTypes.NameIdentifier
                   || claim.Type is ClaimTypes.Name
                   || claim.Type is Claims.Private.RegistrationId
                   || claim.Type is "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider");

            // Note: when using external authentication providers with ASP.NET Identity,
            // the user identity MUST be added to the external authentication cookie scheme.
            var identity = new ClaimsIdentity(claims,
                authenticationType: CookieAuthenticationDefaults.AuthenticationType,
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            // Build the authentication properties based on the properties that were added when the challenge was triggered.
            // Build the authentication properties based on the properties that were added when the challenge was triggered.
            var properties = new AuthenticationProperties(result.Properties.Dictionary
                .Where(item =>
                {
                    if (item.Key == ".redirect")
                    {
                        return true;
                    }
                    else if (item.Key == OpenIddictClientOwinConstants.Tokens.BackchannelAccessToken
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
    }
}
