// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

﻿using System;
using System.Runtime.Serialization;

namespace WebRole.Models
{
    [DataContract]
    public class LogMessage
    {
        private static readonly Random Rand = new Random();

        [DataMember]
        public Guid LogId { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime LogTime { get; set; }

        public LogMessage()
        {
            LogId = Guid.NewGuid();
            Message = "My Log Message " + Rand.Next();
            LogTime = DateTime.UtcNow;
        }
    }
}