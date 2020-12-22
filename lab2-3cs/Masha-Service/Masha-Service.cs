using System.Threading;
using System.ServiceProcess;

namespace M_Service
{
    public partial class Masha_Service : ServiceBase
    {
        Watcher watcher;
        public Masha_Service()
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
