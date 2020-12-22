using System.ServiceProcess;
using System.Threading;

namespace M_Service
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
#if DEBUG
            STW_Service service = new STW_Service();
            service.OnDebug();
#else


            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Masha_Service()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
