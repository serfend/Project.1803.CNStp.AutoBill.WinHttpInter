using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilVerify;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNet4.Utilities.UtilHttp;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web;
using System.IO;


using WinHttp;
namespace JsHelp.API.User
{
	class LoginHandle
	{

		public static string JSESSIONID(out string lt, out string excution, out string actionUrl, out string responseContent)
		{
			
			var http = new HttpHelper();
			var item = new HttpItem() { URL = "http://jiyou.main.11185.cn/u/buyerCenter.html" };

			var responseInfo = http.GetHtml(item);
			var responseHeaders = responseInfo.Header;
			responseContent = responseInfo.Html;
			actionUrl = "https://passport.11185.cn:8001" + HttpUtil.GetElement(responseContent, "action=\"", "\"");
			lt = HttpUtil.GetElement(responseContent, "name=\"lt\" value=\"", "\"");
			excution = HttpUtil.GetElement(responseContent, "name=\"execution\" value=\"", "\"");
			return responseInfo.Cookie;


		}
		public static byte[] GetVerifyImg(User user)
		{
			var http = new HttpHelper();
			var item = new HttpItem()
			{
				Cookie = user.JSESSIONID,
				Host = "passport.11185.cn:8001",
				Referer = "https://passport.11185.cn:8001/cas/tlogin?service=http://jiyou.11185.cn/index.html",
				URL = "https://passport.11185.cn:8001/cas/captcha/captcha",
				ResultType = ResultType.Byte
			};
			var response = http.GetHtml(item);
			item.ResultType = ResultType.String;
			item.URL = "https://passport.11185.cn:8001/cas/account/encryptKey";
			user.EncryptKey = http.GetHtml(item).Html;
			return response.ResultByte;

		}
		public static string GetLoginLocation(string lt, string execution, string code, string targetUrl, User user)
		{
			var modulus = HttpUtil.GetElement(user.EncryptKey, "modul\":\"", "\"");
			var exponent = HttpUtil.GetElement(user.EncryptKey, "exponent\":\"", "\"");
			var encodedPassworder = new Password.PasswordEncoder(exponent, modulus);
			//var encodedPassworder = new Password.PasswordEncoder();
			var encodedPassword = encodedPassworder.GetPasswordRAS(user.Password);

			var handler = new HttpClientHandler()
			{
				AllowAutoRedirect = true,
			};
			using (var client = new HttpClient())
			{
				//client.DefaultRequestHeaders.Add("Cookies", user.JSESSIONID);
				//client.DefaultRequestHeaders.Add("Host", "passport.11185.cn:8001");
				//client.DefaultRequestHeaders.Add("Referer", "https://passport.11185.cn:8001/cas/tlogin?service=http://jiyou.11185.cn/index.html");
				//client.DefaultRequestHeaders.Add("Accept", "*/*");
				//client.DefaultRequestHeaders.Add("Origin", "https://passport.11185.cn:8001");
				//var data = new Dictionary<string, string>
				//{
				//	["username"] = user.Username,
				//	["password"] = encodedPassword,
				//	["code"] = code,
				//	["lt"] = lt,
				//	["execution"] = execution,
				//	["_eventId"] = "submit",
				//};

				//var content = new FormUrlEncodedContent(data);
				//var http = client.PostAsync(targetUrl, content).Result;

				var request = WebRequest.Create(targetUrl);
				request.ContentType = "application/x-www-form-urlencoded";
				request.Method = "Post";
				var data = Encoding.GetEncoding("UTF-8").GetBytes(string.Format("username={0}&password={1}&code={2}&lt={3}&execution={4}&_eventId=submit", user.Username, encodedPassword, code, lt, execution));
				request.ContentLength = data.Length;
				using (var stream = request.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}
				HttpWebResponse response = request.GetResponse() as HttpWebResponse;
				Stream responseStream = response.GetResponseStream();
				string resultInfo = null;
				using (StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
				{
					resultInfo = reader.ReadToEnd();
				}
				responseStream.Close();

				Console.WriteLine("准备完成,请求登录");


				//resultInfo = ZipHelper.GZipDecompressString(resultInfo);
				if (resultInfo.Contains("用户名或密码错误"))
				{
					user.LoginFailed("用户名或密码错误");
					throw new UserPasswordFailedException();
				}
				else if (resultInfo.Contains("验证码错误"))
				{
					user.LoginFailed("验证码错误");
					throw new VerifyFailedException("报告验证码错误" + user.LoginId + "返回结果:" + user.Verifier.Report(RuoKuaiHttp.ver3040Config, user.LoginId));
				}
				else if (resultInfo.Contains("在试图完成你的请求时出错。请通知你的技术支持或重试。"))
				{
					user.LoginFailed("CAS出错");
					throw new WebCASNotSupportException();
				}
				//var location = http.Headers.GetValues("Location");

				return null;
			}
		}

		[Serializable]
		public class WebCASNotSupportException : Exception
		{
			public WebCASNotSupportException() { }
			public WebCASNotSupportException(string message) : base(message) { }
			public WebCASNotSupportException(string message, Exception inner) : base(message, inner) { }
			protected WebCASNotSupportException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
		[Serializable]
		public class UserPasswordFailedException : Exception
		{
			public UserPasswordFailedException() { }
			public UserPasswordFailedException(string message) : base(message) { }
			public UserPasswordFailedException(string message, Exception inner) : base(message, inner) { }
			protected UserPasswordFailedException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
		[Serializable]
		public class VerifyFailedException : Exception
		{
			public VerifyFailedException() { }
			public VerifyFailedException(string message) : base(message) { }
			public VerifyFailedException(string message, Exception inner) : base(message, inner) { }
			protected VerifyFailedException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
	}
}
