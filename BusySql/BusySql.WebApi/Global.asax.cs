using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BusySql.Support;

namespace BusySql.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
           
            GlobalConfiguration.Configure(WebApiConfig.Register);
            BusySqlUtil.PutQuery("TooMuchSql", "TooMuchProcSql3.txt");
            BusySqlUtil.PutQuery("LessSql", "LessProcSql3.txt");
     
        }
    }
}
