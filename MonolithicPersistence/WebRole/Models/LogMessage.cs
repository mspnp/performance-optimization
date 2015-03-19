// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebRole.Models
{
    [DataContract]
    public class LogMessage
    {
        private static Random rand = new Random();
        [DataMember]
        public string LogId { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime LogTime { get; set; }

        public LogMessage()
        {
            LogId = Guid.NewGuid().ToString();
            Message = "My Log Message " + rand.Next();
            LogTime = DateTime.UtcNow;
        }
    }
}