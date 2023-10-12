using System;
using System.IO;
using System.Text;

namespace infrastructure.utils
{
    public class MyLogger
    {
        static object locker = new object();
        class Exceptions
        {
            public string Type { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
        }
        static readonly string LogPath;
        static readonly string TestPath;

        static MyLogger()
        {
            LogPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Logs");
            TestPath = Path.Combine(Directory.GetCurrentDirectory(), $"Logs/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}");//日志记录文件
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            
            if (!Directory.Exists(TestPath))
            {
                Directory.CreateDirectory(TestPath);
            }
        }

        /// <summary>
        /// 记录简单的信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public static void WriteMessage(string message, string param)
        {
            lock (locker)
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(message))
                {
                    sb.Append($"{message},");
                }
                sb.AppendLine();
                System.IO.File.AppendAllText(Path.Combine(TestPath, $"{DateTime.Now.ToString("yyyyMMdd")}_{param}.txt"), sb.ToString());
            }
        }

    }
}