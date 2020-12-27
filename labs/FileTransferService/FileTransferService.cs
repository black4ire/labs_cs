using System.ServiceProcess;
using System.Threading;

namespace FileTransferService
{
    public partial class FileTransferService : ServiceBase
    {
        XXFTP_Watcher watcher;
        public FileTransferService()
        {
            InitializeComponent();
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            watcher = new XXFTP_Watcher();
            Thread watcherThread = new Thread(new ThreadStart(watcher.Start));
            watcherThread.Start();
        }

        protected override void OnStop()
        {
            watcher.Stop();
            Thread.Sleep(1000);
        }

        public void OnDebug()
        {
            OnStart(null);
        }
    }
}
