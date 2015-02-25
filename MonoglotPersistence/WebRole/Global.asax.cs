using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using WebRole.Logging;

namespace WebRole
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Logging.Logging.Start();
        }
        protected void Application_End()
        {
            Logging.Logging.End();
        }

    }
}
