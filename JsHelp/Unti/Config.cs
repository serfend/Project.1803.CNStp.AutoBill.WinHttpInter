using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace tools
{
    [Serializable]
    public class Config
    {
        public Config() {
        
        }
        public string domain = "";
        public int port = 80;
        public int reTry = 1;
        public int maxTime = 10;//延时注入判断阀值
        public int timeOut = 10;//秒
        public string encoding = "UTF-8";
        public string request = "";
        public bool is_foward_302 = true;
        public bool useSSL = false;//ssl
    }
}
