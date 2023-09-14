using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Server
{
    public class MyAuthorizeAttribute: AuthorizeAttribute
    {
        public string AuthSchemes { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (this.AuthSchemes != null)   
            {
                string scheme = httpContext?.User?.Identity?.AuthenticationType;
                if (string.IsNullOrWhiteSpace(scheme))
                {
                    return false;
                }

                return this.AuthSchemes.Split(',').Contains(scheme);
            }

            return base.AuthorizeCore(httpContext);
        }
    }
}