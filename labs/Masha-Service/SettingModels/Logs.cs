using System.Configuration;

using Config_Provider;
namespace STW_Service
{
    [Parsable("mainLogs")]
    class Logs
    {
        [Parsable("error")]
        public string ErrorLog { get; set; } = ConfigurationManager.AppSettings["ErrorLogPath"];
        [Parsable("actLog")]
        public string Log { get; set; } = ConfigurationManager.AppSettings["LogPath"];
        public Logs() { }
    }
}
