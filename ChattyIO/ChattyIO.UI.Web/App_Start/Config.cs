namespace ChattyIO.UI.Web.App_Start
{
    using System.Configuration;

    public static class Config
    {
        static Config()
        {
            ApiEndPointBaseAddress = ConfigurationManager.AppSettings["apibaseaddress"];
        }
        public static string ApiEndPointBaseAddress { get; set; }
    }
}