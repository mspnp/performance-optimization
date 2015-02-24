using System.Diagnostics.Tracing;
[EventSource(Name = "MyCompany")]
public class MyCompanyEventSource : EventSource
{
    private static MyCompanyEventSource _log = new MyCompanyEventSource();
    private MyCompanyEventSource() { }
    public static MyCompanyEventSource Log { get { return _log; } }

    [Event(1, Message = "Application Failure: {0}")]
    internal void Failure(string message)
    {
        this.WriteEvent(1, message);
    }

    [Event(2, Message = "Starting up.")]
    internal void Startup()
    {
        this.WriteEvent(2);
    }


    [Event(3, Message = "loading page {1} activityID={0}")]
    internal void PageStart(int ID, string url)
    {
        this.WriteEvent(3, ID, url);
    }

    [Event(4, Message = "Read data start.")]
    public void ReadDataStart()
    {
        this.WriteEvent(4);
    }

    [Event(5, Message = "Read data finish.")]
    public void ReadDataFinish()
    {
        this.WriteEvent(5);
    }
}
