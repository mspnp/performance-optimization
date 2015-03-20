// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Web.Http;
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
