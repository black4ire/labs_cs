using System;

namespace AppInsightsAPI
{
    public class ErrorTableRow
    {
        public string type;
        public string message;
        public string stacktrace;
        public DateTime exactTime;
        public ErrorTableRow(string t, string m, string stt, DateTime time)
        {
            type = t;
            message = m;
            stacktrace = stt;
            exactTime = time;
        }
    }
}
