using AppInsightsAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer
{
    class FileGenerator
    {
        LoggerToDB logger;
        public FileGenerator(string logDBstr) 
        {
            logger = new LoggerToDB(logDBstr);
        }
        public void Generate(string path, string text, string extension)
        {
            string fullpath = Path.Combine(path, DateTime.Now.ToString("dd-MMM-yyyy HH-mm-ss") + extension);
            try
            {
                using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
                logger.LogToDB(new LogTableRow($"Успешная запись данных в файл {Path.GetFileName(fullpath)}.", DateTime.Now));
            }
            catch (Exception ex)
            {
                logger.LogToErrorDBAsync(new ErrorTableRow(ex.GetType().FullName, ex.Message, ex.StackTrace, DateTime.Now));
            }
        }
        public async void GenerateAsync(string path, string text, string extension)
        {
            await Task.Run(() => Generate(path, text, extension));
        }
    }
}
