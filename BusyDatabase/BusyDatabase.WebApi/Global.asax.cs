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

            Queries.LoadFromResources("TooMuchSql", "TooMuchProcSql3.sql");
            Queries.LoadFromResources("LessSql", "LessProcSql3.sql");
        }
    }
}
