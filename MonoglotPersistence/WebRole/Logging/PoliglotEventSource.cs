using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics.Tracing;

namespace WebRole.Logging
{

    [EventSource(Name = "Poliglot")]
    public class PoliglotEventSource : EventSource
    {
        private static PoliglotEventSource _log = new PoliglotEventSource();
        private PoliglotEventSource() { }
        public static PoliglotEventSource Log { get { return _log; } }

        [Event(1, Message = "Starting up.")]
        internal void Startup()
        {
            this.WriteEvent(1);
        }

        [Event(2, Message = "loading page {1} activityID={0}")]
        internal void PageStart(int ID, string url)
        {
            this.WriteEvent(2, ID, url);
        }

        [Event(3, Message = "Read data start.")]
        internal void ReadDataStart()
        {
            this.WriteEvent(3);
        }

        [Event(4, Message = "Read data finish, elapsed time ={0}")]
        internal void ReadDataFinish(long elapsedtime)
        {
            this.WriteEvent(4, elapsedtime);
        }

        [Event(5, Message = "PageEnd")]
        internal void PageEnd()
        {
            this.WriteEvent(5);
        }
    }
}