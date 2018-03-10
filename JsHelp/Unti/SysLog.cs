using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;
using System.Security.Cryptography;


namespace DotNet4.Utilities.UtilReg
{
    class Logger
    {
		public static string path = "log";

        public static void SysLog(string log)
        {
            AppendLogToFile(string.Format("{0}/{1}.log", path, DateTime.Now.ToLongDateString()),string.Format("{0}:{1}", DateTime.Now.ToLongDateString() , log));
        }

        public static object c = "";
        public static void AppendLogToFile(string path, string log)
        {
            //锁住，防止多线程引发错误
            lock (c)
            {
                List<string> list = new List<string>();
                FileStream fs_dir = null;
                StreamWriter sw = null;
                try
                {
                    fs_dir = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "/" + path, FileMode.Append, FileAccess.Write);

                    sw = new StreamWriter(fs_dir);

                    sw.WriteLine(log);

                    sw.Close();

                    fs_dir.Close();

                }
                catch (Exception e)
                {
					Logger.SysLog("FileTools-AppendLogToFile-error:读取文件内容发生错误！" + e.Message);
                }
                finally
                {
                    if (sw != null)
                    {
                        sw.Close();
                    }
                    if (fs_dir != null)
                    {
                        fs_dir.Close();
                    }
                }
            }

        }
     
    }
}
