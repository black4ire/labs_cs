using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace DataManagerService
{
    public partial class DataManager : ServiceBase
    {
        GeneratingTimer genTimer;
        public DataManager()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            genTimer = new GeneratingTimer();
            Thread ManagerThread = new Thread(new ThreadStart(genTimer.Start));
            ManagerThread.Start();
        }

        protected override void OnStop()
        {
            genTimer.Stop();
            Thread.Sleep(1000);
        }
        public void OnDebug()
        {
            OnStart(null);
        }
    }
}
