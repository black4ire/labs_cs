using System;
using System.Configuration;
using System.IO;

namespace M_Service
{
    public class Manager
    {
        IConfigurationProvider ConfigProvider { get; }
        readonly object obect = new object(); 

        public Manager()
        {
            string fileXMLPath = ConfigurationManager.AppSettings["PathToXml"];
            string fileJsonPath = ConfigurationManager.AppSettings["PathToJson"];

            try
            {
                if (File.Exists(fileXMLPath))
                    ConfigProvider = new XmlParser();
                else if (File.Exists(fileJsonPath))
                    ConfigProvider = new JsonParser();
                if (ConfigProvider == null)
                {
                    throw new Exception("Невозможно установить поставщика конфигурации.\nВыбран поставщик по умолчанию (App.config)");
                }
            }
            catch (Exception ex)
            {
                lock (obect)
                {
                    string errorLogPath = ConfigurationManager.AppSettings["ErrorLogPath"];
                    if (File.Exists(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, "\n"+ DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") 
                            + " " + ex.Message);
                    }
                }
            }
        }

        public T GetOptions<T>() where T : new()
        {
            T obectParsed = new T();
            try
            {
                if (ConfigProvider != null)
                    obectParsed = ConfigProvider.Parse<T>();
            }
            catch (Exception ex)
            {
                lock (obect)
                {
                    string errorLogPath = ConfigurationManager.AppSettings["ErrorLogPath"];
                    if (File.Exists(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
            return obectParsed;
        }
    }
}
