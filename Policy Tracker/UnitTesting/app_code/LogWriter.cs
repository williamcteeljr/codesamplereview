using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace UnitTesting.app_code
{
    class LogWriter
    {
        private string m_exePath = string.Empty;
        public LogWriter(string logMessage)
        {
            LogWrite(logMessage);
        }
        public void LogWrite(string logMessage)
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {

                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {

                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        public void Log(string msg, TextWriter txtWriter)
        {
            try
            {
                txtWriter.WriteLine("{0} {1} : {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), msg);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}
