using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

namespace STW_Service
{
    [RunInstaller(true)]
    public partial class STW_Installer : Installer
    {
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;
        public STW_Installer()
        {
            InitializeComponent();
            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "STW-Service";
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
