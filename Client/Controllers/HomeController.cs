using Microsoft.Owin.Security.Cookies;
using OpenIddict.Client;
using OpenIddict.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static OpenIddict.Client.Owin.OpenIddictClientOwinConstants;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> Auth()
        {
            var context = HttpContext.GetOwinContext();

            var result = await context.Authentication.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationType);
            var token = result.Properties.Dictionary[Tokens.BackchannelAccessToken];

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var content = await client.GetStringAsync("https://localhost:44355/api/values/get");

                return Content(content);
            }
        }
    }
}