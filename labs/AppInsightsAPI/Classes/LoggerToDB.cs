using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppInsightsAPI
{
    public class LoggerToDB
    {
        string connectionDBstring { get; }
        public LoggerToDB(string DBCS)
        {
            connectionDBstring = DBCS;
        }
        public void LogToDB(LogTableRow row)
        {
            using (var connection = new SqlConnection(connectionDBstring))
            {
                connection.Open();
                LogToDBRoutine(connection, row);
            }
        }
        public async void LogToDBAsync(LogTableRow row)
        {
            await Task.Run(() => LogToDB(row));
        }
        public void LogToErrorDB(ErrorTableRow row)
        {
            using (var connection = new SqlConnection(connectionDBstring))
            {
                connection.Open();
                LogToErrorDBRoutine(connection, row);
            }
        }
        public async void LogToErrorDBAsync(ErrorTableRow row)
        {
            await Task.Run(() => LogToErrorDB(row));
        }
        private void LogToDBRoutine(SqlConnection openedConnection, LogTableRow row)
        {
            string SP_Expression = "SP_WriteToLog";
            SqlTransaction transaction = openedConnection.BeginTransaction();
            try
            {
                SqlCommand command = new SqlCommand(SP_Expression, openedConnection);
                command.Transaction = transaction;
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter message = new SqlParameter
                {
                    ParameterName = "@message",
                    Value = row.message
                };
                SqlParameter exacttime = new SqlParameter
                {
                    ParameterName = "@exacttime",
                    SqlDbType = SqlDbType.DateTime,
                    Value = row.exactTime
                };
                command.Parameters.AddRange(new[] { message, exacttime });

                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                ErrorTableRow errorRow = new ErrorTableRow(ex.GetType().FullName, ex.Message, ex.StackTrace, DateTime.Now);
                LogToErrorDBAsync(errorRow);
                transaction.Rollback();
            }
        }
        private void LogToErrorDBRoutine(SqlConnection openedConnection, ErrorTableRow row)
        {
            string SP_Expression = "SP_WriteToErrorLog";
            SqlTransaction transaction = openedConnection.BeginTransaction();
            try
            {
                SqlCommand command = new SqlCommand(SP_Expression, openedConnection);
                command.Transaction = transaction;
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter type = new SqlParameter
                {
                    ParameterName = "@type",
                    Value = row.type
                };
                SqlParameter message = new SqlParameter
                {
                    ParameterName = "@message",
                    Value = row.message
                };
                SqlParameter stacktrace = new SqlParameter
                {
                    ParameterName = "@stacktrace",
                    Value = row.stacktrace
                };
                SqlParameter exacttime = new SqlParameter
                {
                    ParameterName = "@exacttime",
                    SqlDbType = SqlDbType.DateTime,
                    Value = row.exactTime
                };
                command.Parameters.AddRange(new[] { type, message, stacktrace,exacttime });

                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Ошибка при попытке записи в лог ошибок! Недопустимо!\nСообщение: " + ex.Message);
            }
        }
    }
}
