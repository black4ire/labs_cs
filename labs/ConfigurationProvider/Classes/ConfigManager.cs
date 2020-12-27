using System;
using System.Configuration;
using System.IO;

using LoggerToTXT;
namespace Config_Provider
{
    public class ConfigManager
    {
        IConfigurationProvider ConfigProvider { get; }
        readonly string filexmlpath = null;
        readonly string filejsonpath = null;
        readonly string filexsdpath = null;
        LoggerToTxt logger = new LoggerToTxt();
        public ConfigManager(string configDir)
        {
            try
            {
                string[] configFiles = Directory.GetFiles(configDir);
                foreach (var file in configFiles)
                {
                    if (filexmlpath == null && Path.GetExtension(file) == ".xml")
                    {
                        filexmlpath = file;
                    }
                    else if (filejsonpath == null && Path.GetExtension(file) == ".json")
                    {
                        filejsonpath = file;
                    }
                    else if (filexsdpath == null && Path.GetExtension(file) == ".xsd")
                    {
                        filexsdpath = file;
                    }
                }

                if (filexmlpath != null && File.Exists(filexmlpath))
                {
                    ConfigProvider = new XmlParser(filexmlpath, filexsdpath);
                }
                else if (filejsonpath != null && File.Exists(filejsonpath))
                {
                    ConfigProvider = new JsonParser(filejsonpath);
                }
                if (ConfigProvider == null)
                {
                    throw new Exception("Невозможно установить поставщика конфигурации.\nВыбран поставщик по умолчанию (App.config)," +
                        " возможны непредвиденные ошибки.");
                }
            }
            catch (Exception ex)
            {

                string errorLogPath = ConfigurationManager.AppSettings["ConfigErrorLogPath"];
                logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
        public T GetOptions<T>() where T : new()
        {
            T objParsed = new T();
            try
            {
                if (ConfigProvider != null)
                    objParsed = ConfigProvider.Parse<T>();
            }
            catch (Exception ex)
            {

                string errorLogPath = ConfigurationManager.AppSettings["ConfigErrorLogPath"];
                logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
            return objParsed;
        }

    }
}
