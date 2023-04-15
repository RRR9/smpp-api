using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SmppServer
{
    public static class LogManager
    {
        private static string _path = @"E:\SMSLogs\" + DateTime.Now.ToString("ddMMyyyy");

        public static void Error(string Error)
        {
            using (StreamWriter sw = new StreamWriter(_path + "SMSErrorLogs.log", true))
            {
                sw.WriteLine(DateTime.Now.ToString("dd:MM:yyyy-HH:mm:ss - ") + Error);
            }
        }

        public static void Debug(string text)
        {
            using (StreamWriter sw = new StreamWriter(_path + "SMSDebugLogs.log", true))
            {
                sw.WriteLine(DateTime.Now.ToString("dd:MM:yyyy-HH:mm:ss - ") + text);
            }
        }
    }
}