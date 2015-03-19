// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using WebRole.Controllers;
using WebRole.App_Start;


namespace WebRole
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            SqldbLogConfig.CreateSqldbLogTableIfNotExist();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
