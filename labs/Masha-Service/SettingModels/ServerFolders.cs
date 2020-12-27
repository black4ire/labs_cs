using System.Configuration;

using Config_Provider;
namespace STW_Service
{
    [Parsable("mainFolders")]
    class ServerFolders
    {
        [Parsable("source")]
        public string SourcePath { get; set; } = ConfigurationManager.AppSettings["SourcePath"];

        [Parsable("target")]
        public string TargetPath { get; set; } = ConfigurationManager.AppSettings["TargetPath"];

        [Parsable("dearchivated")]
        public string DearcPath { get; set; } = ConfigurationManager.AppSettings["DearchivationPath"];

        [Parsable("archivated")]
        public string ArcPath { get; set; } = ConfigurationManager.AppSettings["ArchivationPath"];
        public ServerFolders() { }
    }
}
