// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

﻿using System.Web;
using System.Web.Http;

namespace MonolithicPersistence.WebRole
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            SqldbLogConfig.CreateSqldbLogTableIfNotExist();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
