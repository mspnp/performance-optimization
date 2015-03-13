using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics.Tracing;

namespace WebRole
{
    [EventSource(Name = "PersistenceError")]
    public class PersistenceErrorEventSource : EventSource
    {
        private static PersistenceErrorEventSource _log = new PersistenceErrorEventSource();
        private PersistenceErrorEventSource() { }
        public static PersistenceErrorEventSource Log { get { return _log; } }

        [Event(65535, Message = "Application Failure: {0}")]
        internal void Failure(string message)
        {
            this.WriteEvent(65535, message);
        }
    }
}