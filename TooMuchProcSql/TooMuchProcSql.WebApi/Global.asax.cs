using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TooMuchProcSql.Support;

namespace TooMuchProcSql.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            SupportFiles.PutQuery("TooMuchSql", "TooMuchProcSql3.txt");
            SupportFiles.PutQuery("LessSql", "LessProcSql2.txt");


      //      AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        //    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        //    RouteConfig.RegisterRoutes(RouteTable.Routes);
        //    BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
