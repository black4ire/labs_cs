using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.ServiceProcess;
using System.Configuration;

namespace M_Service
{
    public class Watcher
    {
        FileSystemWatcher SW { get; set; } 
        FileSystemWatcher TW { get; set; }
        readonly object obect = new object(); 
        bool on;
        ServerFolders FolderSet { get; set; }
        Logs LogSet { get; set; }
        public Watcher()
        {
            FolderSet = new ServerFolders();
            LogSet = new Logs();
            try
            {
                Manager configManager = new Manager();
                FolderSet = configManager.GetOptions<ServerFolders>();
                LogSet = configManager.GetOptions<Logs>();
            }
            catch (Exception ex)
            {
                lock (obect)
                {
                    string errorLogPath = LogSet.ErrorLog;
                    if (File.Exists(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }

            SW = new FileSystemWatcher(FolderSet.SourcePath);
            TW = new FileSystemWatcher(FolderSet.TargetPath);
            
            SW.IncludeSubdirectories = TW.IncludeSubdirectories = true;
        }
        public void Start()
        {
            SW.Deleted += Delete;
            TW.Deleted += Delete;
            SW.Created += Create;
            TW.Created += Create;
            SW.Renamed += Rename;
            TW.Renamed += Rename;
            SW.EnableRaisingEvents = TW.EnableRaisingEvents = true;
            on = true;
            while (on)
            {
                Thread.Sleep(500);
            }
        }
        public void Stop()
        {
            SW.EnableRaisingEvents = TW.EnableRaisingEvents = false;
            on = false;
        }
        private void WriteEntry(string fileEvent, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogSet.Log, true))
                {
                    writer.WriteLine(string.Format("{0} файл {1} был {2}",
                                     DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), filePath, fileEvent));
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                string errorLogPath = LogSet.ErrorLog;
                if (File.Exists(errorLogPath))
                {
                    File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
        void Delete(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileEvent = "удален";
                string filePath = e.FullPath;
                lock (obect)
                {
                    WriteEntry(fileEvent, filePath);
                }
            }
            catch (Exception ex)
            {
                lock (obect)
                {
                    string errorLogPath = LogSet.ErrorLog;
                    if (File.Exists(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
        }
        void Create(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileEvent = "создан";
                string filePath = e.FullPath;
                lock (obect)
                {
                    WriteEntry(fileEvent, filePath);
                }
                if (Path.GetDirectoryName(e.FullPath) == SW.Path)
                {
                    Task.Run(() => (new Archivator()).Archivate(e.FullPath, FolderSet.ArcPath));
                }
            }
            catch (Exception ex)
            {
                lock (obect)
                {
                    string errorLogPath = LogSet.ErrorLog;
                    if (File.Exists(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
        }
        void Rename(object sender, RenamedEventArgs e)
        {
            try
            {
                if (Path.GetFileNameWithoutExtension(e.FullPath).Contains("%%") &&
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
                    lock (obect)
                    {
                        WriteEntry(fileEvent, filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                lock (obect)
                {
                    string errorLogPath = LogSet.ErrorLog;
                    if (File.Exists(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
        }
    }
}
