using Microsoft.Owin.Security.Cookies;
using OpenIddict.Abstractions;
using OpenIddict.Server.Owin;
using Server.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Server.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Email == "admin@gmail.com" && model.Password == "admin")
            {
                var claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("name", "admin_name"),
                    new Claim("role", "admin")
                };

                var ci = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationType);
                var cp = new ClaimsPrincipal(ci);

                HttpContext.GetOwinContext().Authentication.SignIn(ci);

                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Something wrong");
            return View(model);
        }
    }
}