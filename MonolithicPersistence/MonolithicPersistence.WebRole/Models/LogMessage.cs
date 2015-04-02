// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MonolithicPersistence.WebRole.Models
{
    public class LogMessage
    {
        private static readonly Random Rand = new Random();

        public Guid LogId { get; set; }
        public string Message { get; set; }
        public DateTime LogTime { get; set; }

        public LogMessage()
        {
            LogId = Guid.NewGuid();
            Message = "My Log Message " + Rand.Next();
            LogTime = DateTime.UtcNow;
        }
    }
}