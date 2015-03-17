using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BusyDatabase.Support;

namespace BusyDatabase.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
           
            GlobalConfiguration.Configure(WebApiConfig.Register);
            BusyDatabaseUtil.PutQuery("TooMuchSql", "TooMuchProcSql3.txt");
            BusyDatabaseUtil.PutQuery("LessSql", "LessProcSql3.txt");
     
        }
    }
}
