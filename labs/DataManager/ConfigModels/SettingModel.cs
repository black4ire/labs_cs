using Config_Provider;
using System;
using System.Configuration;

namespace DataManagerService
{
    [Parsable("ModelGenService")]
    public class SettingModel
    {
        [Parsable("DBconnetion")]
        public string connectionDBString { get; set; } = ConfigurationManager.AppSettings["DefaultDBconnection"];
        [Parsable("FTP_path")]
        public string pathToFTP { get; set; } = ConfigurationManager.AppSettings["DefaultFTPpath"];
        [Parsable("LogDBconntection")]
        public string connectionLogDBString { get; set; } = ConfigurationManager.AppSettings["DefaultLogDBconntection"];
        private int timeoutInSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultTimeout"]);
        [Parsable("Seconds")]
        public int TimeoutInSeconds
        {
            set 
            {
                if (value >= 5 && value <= 20)
                    timeoutInSeconds = value;
            }
            get
            {
                return timeoutInSeconds;
            }
        }
    }
}
