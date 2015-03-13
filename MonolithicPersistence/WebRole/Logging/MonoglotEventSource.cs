using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics.Tracing;
namespace WebRole
{
    [EventSource(Name = "Monoglot")]
    public class MonoglotEventSource : EventSource
    {
        private static MonoglotEventSource _log = new MonoglotEventSource();
        private MonoglotEventSource() { }
        public static MonoglotEventSource Log { get { return _log; } }

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

        [Event(6, Message = "Write data start.")]
        internal void WriteDataStart()
        {
            this.WriteEvent(6);
        }

        [Event(7, Message = "Write data finish, elapsed time ={0}")]
        internal void WriteDataFinish(long elapsedtime)
        {
            this.WriteEvent(7, elapsedtime);
        }

    }
}