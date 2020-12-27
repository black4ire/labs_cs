using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Configuration;

using Config_Provider;
using LoggerToTXT;
namespace STW_Service
{
    public class Watcher
    {
        LoggerToTxt logger;
        FileSystemWatcher SW { get; set; } //source watcher
        FileSystemWatcher TW { get; set; } //target watcher
        bool enabled;
        ServerFolders FolderSet { get; set; }
        Logs LogSet { get; set; }
        readonly string pathToConfig = ConfigurationManager.AppSettings["PathToConfig"];
        public Watcher()
        {
            FolderSet = new ServerFolders();
            LogSet = new Logs();
            logger = new LoggerToTxt();
            try
            {
                ConfigManager configManager = new ConfigManager(pathToConfig);
                FolderSet = configManager.GetOptions<ServerFolders>();
                LogSet = configManager.GetOptions<Logs>();
            }
            catch (Exception ex)
            {

                string errorLogPath = LogSet.ErrorLog;
                logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }

            SW = new FileSystemWatcher(FolderSet.SourcePath);
            TW = new FileSystemWatcher(FolderSet.TargetPath);

            SW.IncludeSubdirectories = TW.IncludeSubdirectories = true;
        }
        public void Start()
        {
            SW.Deleted += Deleted;
            TW.Deleted += Deleted;
            SW.Created += Created;
            TW.Created += Created;
            SW.Renamed += Renamed;
            TW.Renamed += Renamed;
            SW.EnableRaisingEvents = TW.EnableRaisingEvents = true;
            enabled = true;
            while (enabled)
            {
                Thread.Sleep(500);
            }
        }
        public void Stop()
        {
            SW.EnableRaisingEvents = TW.EnableRaisingEvents = false;
            enabled = false;
        }
        void Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileEvent = "удален";
                string filePath = e.FullPath;
                logger.LogToAsync(LogSet.Log, string.Format("{0} файл {1} был {2}",
                                     DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), filePath, fileEvent));
            }
            catch (Exception ex)
            {
                string errorLogPath = LogSet.ErrorLog;
                logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
        void Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileEvent = "создан";
                string filePath = e.FullPath;
                logger.LogToAsync(LogSet.Log, string.Format("{0} файл {1} был {2}",
                                     DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), filePath, fileEvent));
                if (Path.GetDirectoryName(e.FullPath) == SW.Path)
                {
                    Task.Run(() => (new Archivator()).Archivate(e.FullPath, FolderSet.ArcPath));
                }
            }
            catch (Exception ex)
            {
                string errorLogPath = LogSet.ErrorLog;
                logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
        void Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                if (Path.GetFileNameWithoutExtension(e.FullPath).Contains("$$$") &&
                    Path.GetDirectoryName(e.FullPath) == FolderSet.ArcPath)
                {
                    Task.Run(() =>
                    {
                        (new Archivator()).Dearchivate(e.FullPath, FolderSet.DearcPath);
                        Thread.Sleep(100);
                        File.Delete(e.FullPath);
                    });
                }
                else
                {
                    string fileEvent = "переименован в " + e.FullPath;
                    string filePath = e.OldFullPath;
                    logger.LogToAsync(LogSet.Log, string.Format("{0} файл {1} был {2}",
                                     DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), filePath, fileEvent));
                }
            }
            catch (Exception ex)
            {
                string errorLogPath = LogSet.ErrorLog;
                logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
