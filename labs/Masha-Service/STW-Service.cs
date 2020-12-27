using System.Threading;
using System.ServiceProcess;

namespace STW_Service
{
    public partial class STW_Service : ServiceBase
    {
        Watcher watcher;
        public STW_Service()
        {
            InitializeComponent();
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            watcher = new Watcher();
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
