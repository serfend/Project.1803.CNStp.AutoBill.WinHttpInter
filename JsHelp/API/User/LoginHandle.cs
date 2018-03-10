using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilVerify;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNet4.Utilities.UtilHttp;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using DotNet4.Utilities.UtilHttp.HttpApiEvent;

namespace JsHelp.API.User
{
	class LoginHandle
	{
		private static HttpClient http = new HttpClient();
		public static string JSESSIONID(out string lt, out string excution, out string actionUrl)
		{
			var jsessuibid=http.GetHtml("http://jiyou.main.11185.cn/u/buyerCenter.html");
			var newUrl = jsessuibid.document.response.GetHeaders("Location");
			jsessuibid=http.GetHtml(newUrl);
			var responseContent = jsessuibid.document.response.DataString(Encoding.UTF8);
			var responseHeaders = jsessuibid.document.response.HeadersDic;
			actionUrl = "https://passport.11185.cn:8001" + HttpUtil.GetElement(responseContent, "action=\"", "\"");
			lt = HttpUtil.GetElement(responseContent, "name=\"lt\" value=\"", "\"");
			excution = HttpUtil.GetElement(responseContent, "name=\"execution\" value=\"", "\"");
			return jsessuibid.document.response.Cookies;


		}
		public static byte[] GetVerifyImg(User user)
		{
			var http = new HttpClient();
			var captcha = http.GetHtml("https://passport.11185.cn:8001/cas/captcha/captcha", cookies: user.JSESSIONID, host: "passport.11185.cn:8001", referer: "https://passport.11185.cn:8001/cas/tlogin?service=http://jiyou.11185.cn/index.html");
			var encrypt=http.GetHtml("https://passport.11185.cn:8001/cas/account/encryptKey", cookies: user.JSESSIONID, host: "passport.11185.cn:8001", referer: "https://passport.11185.cn:8001/cas/tlogin?service=http://jiyou.11185.cn/index.html");
			user.EncryptKey = encrypt.document.response.DataString(Encoding.UTF8);
			return captcha.document.Data;
		}
		public static string GetLoginLocation(string lt, string execution, string code, string targetUrl, User user)
		{
			var modulus = HttpUtil.GetElement(user.EncryptKey, "modul\":\"", "\"");
			var exponent = HttpUtil.GetElement(user.EncryptKey, "exponent\":\"", "\"");
			var encodedPassworder = new Password.PasswordEncoder(exponent, modulus);
			//var encodedPassworder = new Password.PasswordEncoder();
			var encodedPassword = encodedPassworder.GetPasswordRAS(user.Password);
			http.Item.Request.HeadersDic["Origin"]= "https://passport.11185.cn:8001";
			var document = http.GetHtml(targetUrl, host: "passport.11185.cn:8001", referer: "https://passport.11185.cn:8001/cas/tlogin?service=http://jiyou.11185.cn/index.html", cookies: user.JSESSIONID, method: "post", postData: string.Format("username={0}&password={1}&code={2}&lt={3}&execution={4}&_eventId=submit", user.Username, encodedPassword, code, lt, execution));

			

			
			var resultInfo = document.document?.response.DataString(Encoding.UTF8);
			if(resultInfo!=null)
			{
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
			}else
			{
				throw new Exception("异常操作");
			}
			user.JSESSIONID = document.document.response.Cookies;
			var location = document.document.response.GetHeaders("Location") ;//HttpUtil.GetElement(resultInfo, "<a href=\"", "\"").Replace("&#59;","?");
			return location;
		}

		internal static string InitUserInfo(string loginLocation, User user)
		{
			var userInfo=http.GetHtml(loginLocation, cookies: user.JSESSIONID).document.response;
			var newUrl = userInfo.GetHeaders("Location");
			var tmpItem = new HttpContentItem() { Cookies =   user.JSESSIONID+ userInfo.Cookies };
			user.JSESSIONID = tmpItem.Cookies;
			var userInfoDoc = http.GetHtml(loginLocation, cookies: user.JSESSIONID).document.response.DataString(Encoding.UTF8);
			return HttpUtil.GetElement(userInfoDoc, "<font color=\"red\">", "</font>");
		}
		public static void GetUserInfo(User user)
		{
			var doc=http.GetHtml("http://jiyou.main.11185.cn/u/buyerCenter.html", cookies: user.JSESSIONID);
			Console.WriteLine(doc.document.response.DataString(Encoding.UTF8));
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
