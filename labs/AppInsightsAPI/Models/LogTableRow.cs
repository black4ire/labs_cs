using System;

namespace AppInsightsAPI
{
    public class LogTableRow
    {
        public string message;
        public DateTime exactTime;
        public LogTableRow(string m, DateTime time)
        {
            message = m;
            exactTime = time;
        }
    }
}
