using Newtonsoft.Json.Linq;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace Resource
{
    public class CustomOpenIddictAuthorize: System.Web.Http.AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            bool res = false;

            var task = Task.Run(async () =>
            {
                res = await IsOpenIddictTokenValidAsync(actionContext);
            });

            task.Wait();

            return res;
        }

       

        //protected override bool AuthorizeCore(HttpContextBase httpContext)
        //{
        //    return IsOpenIddictTokenValid(httpContext);
        //}

        private async Task<bool> IsOpenIddictTokenValidAsync(HttpActionContext httpContext)
        {
            //var _service = System.Web.Mvc.DependencyResolver.Current.GetService<OpenIddictValidationService>();

            try
            {
                string token = httpContext.Request.Headers.Authorization?.Parameter;

                if (!string.IsNullOrEmpty(token))
                {
                    using (var client = new HttpClient())
                    {
                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("client_id", "test_client"),
                            new KeyValuePair<string, string>("client_secret", "test_secret"),
                            new KeyValuePair<string, string>("token", token)
                        });

                        var response = await client.PostAsync("https://localhost:44385/connect/introspect", content);


                        if (response.IsSuccessStatusCode)
                        {
                            var str = await response.Content.ReadAsStringAsync();

                            JObject json = JObject.Parse(str);

                            if (!json.TryGetValue("active", out var active))
                            {
                                active = "false";
                            }

                            if (!json.TryGetValue("aud", out var audiences))
                            {
                                audiences = "";
                            }

                            if (active.ToString() == "True" && audiences.ToString() == "test_resource")
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}