using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

namespace M_Service
{
    [RunInstaller(true)]
    public partial class Masha_Installer : Installer
    {
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;
        public Masha_Installer()
        {
            InitializeComponent();
            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "Masha-Service";
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
