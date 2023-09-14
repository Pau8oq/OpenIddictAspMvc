using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.Owin.Security;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using OpenIddict.Validation.Owin;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Resource.Controllers
{
    public class ValuesController : ApiController
    {
        //private readonly OpenIddictValidationService _service;

        //public ValuesController(OpenIddictValidationService service)
        //{
        //    _service = service;
        //}

        [CustomOpenIddictAuthorize]
        public async Task<IHttpActionResult> Get()
        {
           
            return Content(HttpStatusCode.OK, new string[] { "value1", "value2" });
            

            //return Unauthorized();
        }

        private async Task<bool> JwtTokenValidator()
        {
            try
            {
                //using (var client = new HttpClient())
                //{ 
                    
                //}

                //string authorizationHeader = HttpContext.Current.Request.Headers["Authorization"];

                //if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
                //{
                //    string token = authorizationHeader.Substring("Bearer ".Length);

                //    var res = await _service.ValidateAccessTokenAsync(token);

                //    if (res.GetAudiences().Contains("test_resource"))
                //    {
                //        return true;
                //    }

                //    return false;
                //}

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
