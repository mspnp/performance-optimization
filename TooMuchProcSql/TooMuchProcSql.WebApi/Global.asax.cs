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
           
            GlobalConfiguration.Configure(WebApiConfig.Register);
            TooMuchProcUtil.PutQuery("TooMuchSql", "TooMuchProcSql3.txt");
            TooMuchProcUtil.PutQuery("LessSql", "LessProcSql2.txt");
     
        }
    }
}
