using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Security;
using OpenIddict.Abstractions;
using OpenIddict.Validation.Owin;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Server.Controllers
{
    [HostAuthentication(OpenIddictValidationOwinDefaults.AuthenticationType)]
    public class ResourceController : ApiController
    {
        [Authorize, HttpGet, Route("~/api/message")]
        public string Index()
        {
            return "fdsfsd";
        }
    }
}