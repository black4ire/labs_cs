using System.Configuration;

using Config_Provider;
namespace FileTransferService
{
    [Parsable("FTS-settings")]
    public class FileTransferSettings
    {
        [Parsable("PathToSourceFTP")]
        public string pathToSourceFTP { get; set; } = ConfigurationManager.AppSettings["DefFTPSourcePath"];
        [Parsable("PathToDestinationFTP")]
        public string pathToDestFTP { get; set; } = ConfigurationManager.AppSettings["DefFTPDestinationPath"];
        [Parsable("LoggerConnectionString")]
        public string logDBCS { get; set; } = ConfigurationManager.AppSettings["DefLogDBCS"];
    }
}
