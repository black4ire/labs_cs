using System;
using System.Threading;
using System.Configuration;

using Config_Provider;
using AppInsightsAPI;
using ServiceLayer;
namespace DataManagerService
{
    class GeneratingTimer
    {
        bool enabled;
        Timer timer;
        readonly SettingModel settings = default;
        readonly string pathToConfig = ConfigurationManager.AppSettings["PathToConfig"];
        LoggerToDB logger = new LoggerToDB(ConfigurationManager.AppSettings["DefaultLogDBconntection"]);
        public GeneratingTimer()
        {
            try
            {
                ConfigManager manager = new ConfigManager(pathToConfig);
                settings = manager.GetOptions<SettingModel>();
            }
            catch (Exception ex)
            {
                ErrorTableRow errorRow = new ErrorTableRow(ex.GetType().FullName, ex.Message, ex.StackTrace, DateTime.Now);
                logger.LogToErrorDB(errorRow);
            }
        }
        public void Start() 
        {
            TimerCallback callback = new TimerCallback(DoOnTick);
            timer = new Timer(callback, null, 3000, settings.TimeoutInSeconds * 1000); // timer starts working after 3 seconds
            enabled = true;
            while (enabled)
            {
                Thread.Sleep(500);
            }
        }
        public void Stop() 
        {
            enabled = false;
            timer.Dispose();
        }
        public void DoOnTick(object sender)
        {
            IDALService service = new DALService(settings.connectionDBString, settings.connectionLogDBString);
            service.MakeJobAsync(settings.pathToFTP);
        }
    }
}
