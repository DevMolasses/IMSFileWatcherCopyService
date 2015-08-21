using System;
using System.IO;

namespace IMSFileWatcherCopyService
{
    public static class Log
    {
        public static bool WriteErrorLog(Exception ex)
        {
            bool retVal;
            try
            {
                using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Logfile.txt", true))
                {
                    string now = DateTime.Now.ToString();
                    sw.WriteLine(string.Format("{0} : {1};{2}",now, ex.Source.ToString().Trim(), ex.Message.ToString().Trim()));
                    if (ex.InnerException.Message != null)
                        sw.WriteLine(string.Format("{0} : {1}", now, ex.InnerException.Message.ToString().Trim()));
                    sw.Flush();
                    sw.Close();
                    retVal = true;
                }
            }
            catch
            {
                retVal = false;
            }
            return retVal;
        }

        public static bool WriteErrorLog(string Message)
        {
            bool retVal;
            try
            {
                using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Logfile.txt", true))
                {
                    sw.WriteLine(DateTime.Now.ToString() + " : " + Message);
                    sw.Flush();
                    sw.Close();
                    retVal = true;
                }
            }
            catch
            {
                retVal = false;
            }
            return retVal;
        }
    }
}
