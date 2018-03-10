using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace tools
{
    public class ServerInfo
    {
        public string host = "";//host主机头
        public string url = "";//pathAndQuery
        public int port = 80;
        public string request = "";
        public string encoding = "";
        public string header = "";
        public string body = "";
        public string reuqestBody = "";
        public string reuqestHeader = "";
        public Dictionary<string, string> headers = new Dictionary<string, string>();
        public string response = "";
        public string gzip = "";
        public int length = 0;
        public int code = 0;
        public int location = 0;
        public int runTime = 0;//获取网页消耗时间，毫秒
        public int sleepTime = 0;//休息时间
        public string cookies = "";
        public bool timeout = false;
    }
}
