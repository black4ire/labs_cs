using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using AppInsightsAPI;
namespace XmlXsdGenerator
{
    public class XXGenerator : IXmlGenerator, IXsdGenerator
    {
        LoggerToDB logger;
        public XXGenerator(string connectLogDBstr)
        {
            logger = new LoggerToDB(connectLogDBstr); 
        }
        public string GenerateXml(DataSet ds)
        {
            string xmlRes = "";
            try
            {
                using (var strWriter = new StringWriter())
                {
                    ds.WriteXml(strWriter);
                    xmlRes = strWriter.ToString();
                }
                logger.LogToDBAsync(new LogTableRow("Успешно сгенерирована строка XML.", DateTime.Now));
            }
            catch (Exception ex)
            {
                ErrorTableRow errorRow = new ErrorTableRow(ex.GetType().FullName, ex.Message, ex.StackTrace, DateTime.Now);
                logger.LogToErrorDBAsync(errorRow);
            }
            return xmlRes;
        }
        public async Task<string> GenerateXmlAsync(DataSet ds)
        {
            return await Task.Factory.StartNew(() => GenerateXml(ds));
        }
        public string GenerateXsd(DataSet ds)
        {
            string xsdRes = "";
            try
            {
                using (var strWriter = new StringWriter())
                {
                    ds.WriteXmlSchema(strWriter);
                    xsdRes = strWriter.ToString();
                }
                logger.LogToDBAsync(new LogTableRow("Успешно сгенерирована XSD схема.", DateTime.Now));
            }
            catch (Exception ex)
            {
                ErrorTableRow errorRow = new ErrorTableRow(ex.GetType().FullName, ex.Message, ex.StackTrace, DateTime.Now);
                logger.LogToErrorDBAsync(errorRow);
            }
            return xsdRes;
        }
        public async Task<string> GenerateXsdAsync(DataSet ds)
        {
            return await Task.Factory.StartNew(() => GenerateXsd(ds));
        }
    }
}
