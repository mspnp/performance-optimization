// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BusyDatabase.Support
{
    public class Queries
    {
        private static readonly Dictionary<string, string> Query = new Dictionary<string, string>();

        private const string ResourceFileBase = "BusyDatabase.Support.";

        public static string Get(string key)
        {
            return Query[key];
        }

        public static void LoadFromResources(string key, string fileName)
        {
            var query = LoadEmbeddedResource(fileName);
            Query.Add(key, query);
        }

        private static string LoadEmbeddedResource(string file)
        {
            var resFile = ResourceFileBase + file;

            // Get the assembly that this class is in
            var assembly = Assembly.GetAssembly(typeof(Queries));

            var stream = assembly.GetManifestResourceStream(resFile);

            if (null == stream)
            {
                var resourceList = assembly.GetManifestResourceNames();

                throw new ApplicationException(string.Format("Could not find resource: {0} in [{1}]", resFile, String.Join(" , ", resourceList)));
            }

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
