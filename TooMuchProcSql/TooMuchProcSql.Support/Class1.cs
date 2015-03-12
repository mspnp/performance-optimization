﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using System;
using System.IO;
using System.Reflection;

namespace TooMuchProcSql.Support
{

   

    public class SupportFiles
    {
        private static Dictionary<string, string> Query = new Dictionary<string, string>();

        private const string ResourceFileBase = "TooMuchProcSql.Support.";

        public static string GetQuery(string word)
        {
            // Try to get the result in the static Dictionary
            string result;
            if (Query.TryGetValue(word, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static void PutQuery(string key, string file )
        {
           
            string result;
            Query.Add(key, GetSqlQuery(file));

        }

       



        private static string GetSqlQuery(string file)
        {


            var resFile = ResourceFileBase + file;

            //Get the assembly that this class is in
            var assembly = Assembly.GetAssembly(typeof(SupportFiles));

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
