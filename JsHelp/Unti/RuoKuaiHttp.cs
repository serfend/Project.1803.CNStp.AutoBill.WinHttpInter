using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilReg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace DotNet4.Utilities.UtilVerify
{
    public class RuoKuaiHttp
    {
		private static bool init=false;
		public static void Init(string username,string password)
		{
			ver3040Config = new Config(username, password) { Typeid="3040"};
			ver6114Config = new Config(username, password) { Typeid="6114"};
			init = true;
		}
		public static Config ver3040Config;//字母验证码
		public static Config ver6114Config ;//8图坐标验证码
		public string GetVer(Config config,string picPath,out string picId)
		{
			return GetVer(config, File.ReadAllBytes(picPath),out picId);	
		}
		public string GetVer(Config config,byte[] picData,out string picId)
		{
			if (!init) throw new Exception("未初始化");
			Console.WriteLine("验证码识别请求开始");
			var begin = HttpUtil.TimeStamp;
			string result= Post(@"http://api.ruokuai.com/create.xml", config.ToDictionary(), picData);
			picId = HttpUtil.GetElementInItem(result, "Id");
			result = HttpUtil.GetElementInItem(result, "Result");
			
			var wasteTime = HttpUtil.TimeStamp - begin;
			Console.WriteLine(string.Format("验证码识别{0}格式返回结果:{1},耗时:{2}ms", config.Typeid, result,wasteTime));
			return result;
		}

		public string Report(Config config,string picId)
		{
			if (!init) throw new Exception("未初始化");
			var dic = config.ToReportDictionary(picId);
			return Report(dic);
		}
		private string Report(IDictionary<object, object> param)
		{
			string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
			byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

			HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://api.ruokuai.com/reporterror.json");
			wr.ContentType = "multipart/form-data; boundary=" + boundary;
			wr.UserAgent = "RK_C# 1.2";
			wr.Method = "POST";
			Stream rs = null;
			try
			{
				rs = wr.GetRequestStream();
			}
			catch { return "无法连接.请检查网络."; }
			string responseStr = null;
			string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
			foreach (string key in param.Keys)
			{
				rs.Write(boundarybytes, 0, boundarybytes.Length);
				string formitem = string.Format(formdataTemplate, key, param[key]);
				byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
				rs.Write(formitembytes, 0, formitembytes.Length);
			}
			rs.Write(boundarybytes, 0, boundarybytes.Length);
			rs.Close();
			WebResponse wresp = null;
			try
			{
				wresp = wr.GetResponse();

				Stream stream2 = wresp.GetResponseStream();
				StreamReader reader2 = new StreamReader(stream2);
				responseStr = reader2.ReadToEnd();

			}
			catch
			{
				//throw;
			}
			finally
			{
				if (wresp != null)
				{
					wresp.Close();
					wresp = null;
				}
				wr.Abort();
				wr = null;

			}
			return responseStr;
		}
        #region Post With Pic
        /// <summary>
        /// HTTP POST方式请求数据(带图片)
        /// </summary>
        /// <param name="url">URL</param>        
        /// <param name="param">POST的数据</param>
        /// <param name="fileByte">图片Byte</param>
        /// <returns></returns>
        private  string Post(string url, IDictionary<object, object> param, byte[] fileByte)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.UserAgent = "RK_C# 1.2";
            wr.Method = "POST";

            //wr.Timeout = 150000;
            //wr.KeepAlive = true;

            //wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
            Stream rs = null;
            try
            {
                rs = wr.GetRequestStream();
            }
            catch { return "无法连接.请检查网络."; }
            string responseStr = null;

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in param.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, param[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, "image", "i.gif", "image/gif");//image/jpeg
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            rs.Write(fileByte, 0, fileByte.Length);

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();

                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                responseStr = reader2.ReadToEnd();

            }
            catch
            {
                //throw;
            }
            finally
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                wr.Abort();
                wr = null;

            }
            return responseStr;
        }
		#endregion

		#region Post
		private string Post(string url, Dictionary<object, object> param)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "RK_C# 1.1";
            //request.Timeout = 30000;

            #region POST方法

            //如果需要POST数据  
            if (!(param == null || param.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in param.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, param[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, param[key]);
                    }
                    i++;
                }

                byte[] data = System.Text.Encoding.UTF8.GetBytes(buffer.ToString());
                try
                {
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch {
                    return "无法连接.请检查网络.";
                }

            }

            #endregion

            WebResponse response = null;
            string responseStr = string.Empty;
            try
            {
                response = request.GetResponse();

                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception)
            {
                //throw;
            }
            finally
            {
                request = null;
                response = null;
            }

            return responseStr;

        }
        #endregion
    }
	public class Config
	{
		private string username;
		private string password;
		public Config(string username,string password)
		{
			this.username = username;
			this.password = password;
		}
		private string typeid= "3040";//4位英文数字混合
		private string timeout="90";
		private string softid = "97651";
		private string softkey = "627f1c6f783741808067317b0c61cc6c";

		public string Username { get => username; set => username = value; }
		public string Password { get => password; set => password = value; }
		public string Typeid { get => typeid; set => typeid = value; }
		public string Timeout { get => timeout; set => timeout = value; }
		public string Softid { get => softid; set => softid = value; }
		public string Softkey { get => softkey; set => softkey = value; }

		public Dictionary<object, object> ToDictionary()
		{
			return  new Dictionary<object, object>
				{
					{"username",username},
					{"password",password},
					{"typeid",typeid},
					{"timeout",timeout},
					{"softid",softid},
					{"softkey",softkey}
				};
		}
		public Dictionary<object,object>ToReportDictionary(string picId)
		{
			return new Dictionary<object, object>
			{
				{"username",username},
				{"password",password},
				{"softid",softid},
				{"softkey",softkey},
				{"id",picId}
			};
		}
	}
}
