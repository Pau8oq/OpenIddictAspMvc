using OpenIddict.Client;
using OpenIddict.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Resource.Controllers
{
    public class HomeController : Controller
    {
        private readonly OpenIddictValidationService _service;

        public HomeController(
            OpenIddictValidationService service)
        {
            _service = service;
        }

        public ActionResult Index()
        {
            return Content("Index");
        }
    }
}