using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace DataManagerService
{
    [RunInstaller(true)]
    public partial class DataManagerInstaller : Installer
    {
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;
        public DataManagerInstaller()
        {
            InitializeComponent();
            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "DataManager";
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
