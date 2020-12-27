using System;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

using AppInsightsAPI;
namespace DataAccessLayer
{
    public class DB_Interaction
    {
        readonly string connectionDBstring;
        readonly LoggerToDB logger;
        public DB_Interaction(string connectDBstr, string connectLogDBstr)
        {
            connectionDBstring = connectDBstr;
            logger = new LoggerToDB(connectLogDBstr);
        }
        public DataTable GetAllPersons()
        {
            DataTable persons = new DataTable("Person");
            string SP_Expression = "SP_GetPersons";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionDBstring))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(SP_Expression, connection);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    dataAdapter.Fill(persons);
                    if (persons.Rows.Count == 0)
                    { 
                        throw new Exception("Попытка вызова процедуры не вернула никаких значений.");
                    }

                    LogTableRow report = new LogTableRow("Данные успешно возвращены из SP_GetPersons.", DateTime.Now);
                    logger.LogToDBAsync(report);
                }
            }
            catch (Exception ex)
            {
                ErrorTableRow row = new ErrorTableRow(ex.GetType().FullName, ex.Message, ex.StackTrace, DateTime.Now);
                logger.LogToErrorDBAsync(row);
            }
            return persons;
        }
        public async Task<DataTable> GetAllPersonsAsync()
        {
            return await Task.Factory.StartNew(() => GetAllPersons());
        }
    }
}