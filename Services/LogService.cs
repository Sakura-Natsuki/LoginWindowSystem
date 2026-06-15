using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginWindowSystem.Services
{
    public class LogService
    {
        private static volatile LogService _instance;

        private static readonly object _lock = new object();

        private readonly string _logPath;

        private LogService()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _logPath = Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd}.log");

            WriteRaw($"{new string('-', 40)}");
            WriteRaw($"日志服务启动[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]");
        }

        public static LogService Instacne
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LogService();
                        }
                    }
                }

                return _instance;
            }
        }

        public void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public void Warn(string message)
        {
            WriteLog("WARN", message);
        }

        public void Error(string message)
        {
            WriteLog("ERROR", message);
        }

        private void WriteLog(string level, string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            WriteRaw(line);
        }

        private void WriteRaw(string line)
        {
            lock (_lock)
            {
                File.AppendAllText(_logPath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
