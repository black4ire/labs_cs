using System;
using System.Configuration;

using Config_Provider;
using AppInsightsAPI;
using System.IO;
using System.Threading;

namespace FileTransferService
{
    class XXFTP_Watcher
    {
        FileTransferSettings settings;
        FileSystemWatcher watcherXML;
        FileSystemWatcher watcherXSD;
        LoggerToDB logger;
        bool enabled;
        public void Start()
        {
            watcherXML.IncludeSubdirectories = true;
            watcherXSD.IncludeSubdirectories = true;
            watcherXML.Created += Created;
            watcherXSD.Created += Created;
            watcherXML.Filter = "*.xml";
            watcherXSD.Filter = "*.xsd";
            watcherXML.EnableRaisingEvents = true;
            watcherXSD.EnableRaisingEvents = true;

            enabled = true;
            while (enabled)
            {
                Thread.Sleep(500);
            }
        }
        public void Stop()
        {
            watcherXML.EnableRaisingEvents = false;
            watcherXSD.EnableRaisingEvents = false;
            enabled = false;
        }
        public XXFTP_Watcher()
        {
            settings = new FileTransferSettings();
            try
            {
                string pathToConfig = ConfigurationManager.AppSettings["PathToConfig"];
                ConfigManager manager = new ConfigManager(pathToConfig);
                settings = manager.GetOptions<FileTransferSettings>();
                logger = new LoggerToDB(settings.logDBCS);
                watcherXML = new FileSystemWatcher(settings.pathToSourceFTP);
                watcherXSD = new FileSystemWatcher(settings.pathToSourceFTP);
            }
            catch (Exception ex)
            {
                ErrorTableRow errorRow = new ErrorTableRow(ex.GetType().FullName, ex.Message, ex.StackTrace, DateTime.Now);
                logger.LogToErrorDBAsync(errorRow);
                settings = new FileTransferSettings();
                watcherXML = new FileSystemWatcher(settings.pathToSourceFTP);
                watcherXSD = new FileSystemWatcher(settings.pathToSourceFTP);
            }
        }
        private void Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                Thread.Sleep(1000);
                while(!IsFileReady(e.FullPath)) ;
                File.Move(e.FullPath, Path.Combine(settings.pathToDestFTP, Path.GetFileName(e.FullPath)));
                logger.LogToDBAsync(new LogTableRow($"Перемещение файла {e.Name} прошло успешно.", DateTime.Now));
            }
            catch (Exception ex)
            {
                ErrorTableRow errorRow = new ErrorTableRow(ex.GetType().FullName, ex.Message, ex.StackTrace, DateTime.Now);
                logger.LogToErrorDBAsync(errorRow);
            }
        }
        private bool IsFileReady(string filename)
        {
            bool ready;
            while (true)
            {
                try
                {
                    var fs = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.None);
                    fs.Close();
                    ready = true;
                    break;
                }
                catch
                {
                    continue;
                }
            }
            return ready;
        }
    }
}
