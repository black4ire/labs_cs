using System;
using XmlXsdGenerator;
using AppInsightsAPI;
using System.Data;
using DataAccessLayer;
using System.Threading.Tasks;

namespace ServiceLayer
{
    public class DALService: IDALService
    {
        LoggerToDB logger;
        XXGenerator generator;
        FileGenerator fileGenerator;
        readonly string connectDBstring;
        readonly string connectLogDBstring;
        public DALService(string DBstr,string logDBstr) 
        {
            logger = new LoggerToDB(logDBstr);
            generator = new XXGenerator(logDBstr);
            fileGenerator = new FileGenerator(logDBstr);
            connectLogDBstring = logDBstr;
            connectDBstring = DBstr;
        }
        public void MakeJob(string pathToFTP)
        {
            try
            {
                DataSet ds = new DataSet("Persons");
                DB_Interaction DBInt = new DB_Interaction(connectDBstring, connectLogDBstring);
                var tableTask = DBInt.GetAllPersonsAsync();

                IXmlGenerator xmlGenerator = generator;
                DataTable table = tableTask.Result;
                ds.Tables.Add(table);
                var xmlTask = xmlGenerator.GenerateXmlAsync(ds);
                //здесь возможно будет что-то ещё
                string xmlText = xmlTask.Result;
                fileGenerator.GenerateAsync(pathToFTP, xmlText, ".xml");

                IXsdGenerator xsdGenerator = generator;
                var xsdTask = xsdGenerator.GenerateXsdAsync(ds);
                //здесь возможно будет что-то ещё
                string xsdText = xsdTask.Result;
                fileGenerator.GenerateAsync(pathToFTP, xsdText, ".xsd");
            }
            catch (Exception ex)
            {
                ErrorTableRow errorRow = new ErrorTableRow(ex.GetType().FullName, ex.Message, ex.StackTrace, DateTime.Now);
                logger.LogToErrorDBAsync(errorRow);
            }
        }
        public async void MakeJobAsync(string pathToFTP)
        {
            await Task.Run(() => MakeJob(pathToFTP));
        }
    }
}
