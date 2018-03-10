using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilHttp.HttpApiEvent;
using DotNet4.Utilities.UtilHttp.HttpException;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinHttp;
namespace DotNet4.Utilities.UtilHttp
{
	public class HttpApi: WinHttpRequestClass
	{
		public Action<HttpApiEvent.ErrorEventDelegate> OnHttpError;
		public Action<HttpApiEvent.DocumentReady> OnDocumentReady;
		public HttpApi()
		{
			OnError += HttpApi_OnError;
			OnResponseFinished += HttpApi_OnResponseFinished;
			
		}
		public HttpApi(bool UsedFidder):this()
		{
			if(UsedFidder)this.SetProxy(2, "127.0.0.1:8888");
		}
		private void HttpApi_OnError(int ErrorNumber, string ErrorDescription)
		{
			OnHttpError?.Invoke(new HttpApiEvent.ErrorEventDelegate(ErrorNumber, ErrorDescription));
		}

		/// <summary>
		/// 取出所有Cookies
		/// </summary>
		/// <returns></returns>
		public static string GetAllCookies(string temp)
		{
			if (string.IsNullOrWhiteSpace(temp))
				return "";
			//匹配出 cookie名称 和 cookie值
			var matches = System.Text.RegularExpressions.Regex.Matches(temp, "Set-Cookie: ?(.*?)=(.*?);");
			if (matches.Count <= 0)
				return "";
			var cookies = string.Empty;
			foreach (var item in matches.Cast<Match>().Where(item => item.Success))
			{
				cookies = item.Groups[1].Value + ":" + item.Groups[2].Value + " ";
			}
			return cookies;
		}

		private void HttpApi_OnResponseFinished()
		{
			OnDocumentReady?.Invoke(new HttpApiEvent.DocumentReady(this.ResponseBody));
		}



	}
	namespace HttpApiEvent
	{
		public class HttpDocument : DocumentReady
		{
			public HttpContentItem response;

			public HttpDocument(ref DocumentReady document,ref HttpApi http, HttpContentItem item) :base(document.Data)
			{
				SetResponse(item, http.GetAllResponseHeaders());
				item.Data = this.Data;
			}

			public HttpDocument(object responseBody, string headers, HttpContentItem item) : base(responseBody)
			{
				SetResponse(item, headers);
				item.Data = this.Data;
			}
			private void SetResponse(HttpContentItem item,string headers)
			{
				response = item;
				response.Cookies = HttpApi.GetAllCookies(headers);
			}
		}
		public class ErrorEventDelegate
		{
			private int errNumber;
			private string errDescription;

			public ErrorEventDelegate(int errNumber, string errDescription)
			{
				this.ErrNumber = errNumber;
				this.ErrDescription = errDescription;
			}

			public int ErrNumber { get => errNumber; set => errNumber = value; }
			public string ErrDescription { get => errDescription; set => errDescription = value; }
		}
		public class DocumentReady
		{
			public byte[] Data;
			public DocumentReady(ref byte[] data)
			{
				this.Data = data;
			}

			public DocumentReady(object responseBody)
			{
				this.Data = (byte[])responseBody;
			}
		}
	}

	class HttpClient
	{
		public Action<HttpClientChild, HttpApiEvent.DocumentReady> DocumentReady;
		public Action<HttpClientChild, HttpApiEvent.ErrorEventDelegate> BadResponse;
		private List<HttpClientChild> child = new List<HttpClientChild>();
		private HttpItem item;

		internal HttpItem Item { get => item; set => item = value; }

		public HttpClientChild GetHtml(string url=null,string method=null,string postData=null,string cookies=null,string userAgent=null,string host=null,string referer =null ,bool ifModifies=true,int timeOut=30000,string clientId =null,Action<HttpApiEvent.HttpDocument>callBack=null)
		{

			item = new HttpItem()
			{
				Host=host,
				Url=url,
				Method=method,
				Referer=referer,
				IfModified=ifModifies,
				UserAgent=userAgent
			};
			item.Request.PostData = postData;
			item.Request.Cookies = cookies;
			var child = new HttpClientChild(this, Item.UsedFidder)
			{
				ID = clientId
			};
			this.child.Add(child);
			child.DocumentReady += (x, xx) =>
			{
				DocumentReady?.Invoke(x, xx);
				callBack?.Invoke(xx);
			};
			child.BadResponse += (x, xx) =>
			{
				BadResponse?.Invoke(x, xx);
			};
			do
			{
				bool sendSuccess = false;
				if (callBack != null)
					sendSuccess=child.GetResponse(item, callBack);
				else
					sendSuccess=child.GetResponse(item);
				if (sendSuccess) return child;
			} while (true);
		}

	}
	class HttpClientChild
	{
		public Action<HttpClientChild,HttpApiEvent.HttpDocument> DocumentReady;
		public Action<HttpClientChild,HttpApiEvent.ErrorEventDelegate> BadResponse;
		private HttpClient parent;
		private HttpApi http;
		public HttpDocument document = null;

		private HttpItem item;
		private long proxyBeginTime, proxyEndTime;
		private bool documentLoaded;
		private string lastUserAgent;
		/// <summary>
		/// 获取响应时间/ms
		/// </summary>
		public long ProxyTime
		{
			get=> documentLoaded?proxyEndTime - proxyBeginTime: HttpUtil.TimeStamp;
				
		}
		public string LastUserAgent { get => lastUserAgent; set => lastUserAgent = value; }
		public HttpItem Item { get => item; set => item = value; }
		public string ID { get; internal set; }

		public HttpClientChild(HttpClient parent,bool UsedFidder=false)
		{
			this.parent = parent;
			http = new HttpApi(UsedFidder);
			http.OnHttpError += (x) =>
			{
				BadResponse?.Invoke(this, x);
			};
			http.OnDocumentReady += (x) =>
			{
				proxyEndTime = HttpUtil.TimeStamp;
				documentLoaded = true;
			};
		}
		public enum Status
		{
			NoElement,NotBuild,NotReady,Ready
		}
		/// <summary>
		/// 同步方法
		/// </summary>
		/// <param name="document"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool GetResponse( HttpItem item = null)
		{
			return GetResponse(item,false);
		}
		/// <summary>
		/// 异步方法
		/// </summary>
		/// <param name="item"></param>
		/// <param name="CallBack"></param>
		/// <returns></returns>
		public bool GetResponse(HttpItem item = null, Action<HttpApiEvent.HttpDocument> CallBack = null)
		{
			return GetResponse(item,false, CallBack);
		}
		public bool GetResponse(HttpItem item=null,bool syn=false,Action<HttpApiEvent.HttpDocument> CallBack=null)
		{
			if (item != null)
			{
				Item = item;
			}
			proxyBeginTime = HttpUtil.TimeStamp;


			http.OnDocumentReady += (x) =>
			{
				var document = SetDocument(x);
				DocumentReady?.Invoke(this, document);
			};
			http.OnError += (x, xx) =>
			{

			};
			try
			{
				if (Item.Url == null || Item.Url.Length < 5)
				{
					throw new UrlInvalidException();
				}
				http.Option[WinHttpRequestOption.WinHttpRequestOption_EnableHttpsToHttpRedirects] = false;
				http.Option[WinHttpRequestOption.WinHttpRequestOption_SslErrorIgnoreFlags] = 13056;
				http.Open(Item.Method, Item.Url, syn);
				http.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
				var cookies = Item.Request.Cookies;
				if(cookies!=null&&cookies.Length>0)http.SetRequestHeader("Cookie", cookies);
				if(Item.Referer!=null)if(Item.Referer!=null)http.SetRequestHeader("Referer", Item.Referer);
				if (Item.IfModified) http.SetRequestHeader("IfModified","0");
				http.SetRequestHeader("UserAgent", Item.UserAgent);
				if (Item.Host != null) http.SetRequestHeader("Host", Item.Host);
				if (Item.AcceptLanguage != null) http.SetRequestHeader("AcceptLanguage", Item.AcceptLanguage);
				if (Item.Request.HeadersDic != null&& Item.Request.HeadersDic.Count>0)
				{
					foreach (var header in Item.Request.HeadersDic)
					{
						http.SetRequestHeader(header.Key, header.Value);
					}
				}
				if(Item.Request.PostData!="")
					http.Send(Item.Request.PostData);
				else
				{
					http.Send();
				}

				if (syn) document = null; else
				{
					var doc = http.ResponseBody;
					var headers = http.GetAllResponseHeaders();
					document = new HttpApiEvent.HttpDocument(doc, headers,Item.Response);
					
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + '\n' + ex.StackTrace);
				document = null;
				return true;
			}
			return true;

		}
		private HttpApiEvent.HttpDocument SetDocument(HttpApiEvent.DocumentReady document)
		{
			return new HttpApiEvent.HttpDocument(ref document,ref this.http,Item.Response);
		}


	}
	namespace HttpException
	{

		[Serializable]
		public class UrlInvalidException : Exception
		{
			public UrlInvalidException() { }
			public UrlInvalidException(string message) : base(message) { }
			public UrlInvalidException(string message, Exception inner) : base(message, inner) { }
			protected UrlInvalidException(
			  SerializationInfo info,
			  StreamingContext context) : base(info, context) { }
		}
	}
	class HttpItem
	{
		private static List<string> UserAgentBase=new List<string>() {
			"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36 {0}",
			"Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_8; en-us) AppleWebKit/534.50 (KHTML, like Gecko) Version/5.1 Safari/534.50 {0}",
			"Mozilla/5.0 (Windows; U; Windows NT 6.1; en-us) AppleWebKit/534.50 (KHTML, like Gecko) Version/5.1 Safari/534.50$Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0 {0}",
		};
		private bool asyn=true;//默认异步
		private string url;
		private string userAgent;
		private bool useRandomAgent=true;
		private string method;
		private string host;
		private string referer;
		private HttpContentItem request;
		private HttpContentItem response;
		private string acceptLanguage;
		private bool ifModified;
		public HttpItem()
		{
			request = new HttpContentItem();
			response = new HttpContentItem();
		}
		/// <summary>
		/// 当存在时会将收到的数据保存到文件
		/// </summary>
		private FileInfo targetFile;

		public string Url { get => url; set => url = value; }
		public string UserAgent { get {
				if (UseRandomAgent)
				{
					var rand = new Random();
					int random = rand.Next(UserAgentBase.Count);
					int randomValue = rand.Next(999);
					return string.Format(UserAgentBase[random], randomValue);
				}
				else
				{
					return userAgent;
				}
			}
			set => userAgent = value; }
		public bool UseRandomAgent { get => useRandomAgent; set => useRandomAgent = value; }
		public string Method { get => method??"get"; set => method = value; }
		public string Host { get => host; set => host = value; }
		public string Referer { get => referer; set => referer = value; }
		public string AcceptLanguage { get => acceptLanguage; set => acceptLanguage = value; }
		public bool IfModified { get => ifModified; set => ifModified = value; }
		public FileInfo TargetFile { get => targetFile; set => targetFile = value; }
		public bool Asyn { get => asyn; set => asyn = value; }
		internal HttpContentItem Request { get => request; set => request = value; }
		internal HttpContentItem Response { get => response; set => response = value; }
		public bool UsedFidder { get;  set; }
	}
	public class HttpContentItem
	{
		private Dictionary<string,string> cookies;
		private Dictionary<string, string> headers;
		private Dictionary<string, string> postData;
		private byte[] data;
		public HttpContentItem()
		{
			cookies = new Dictionary<string, string>();
			headers = new Dictionary<string, string>();
			postData = new Dictionary<string, string>();
		}

		public string DataString(Encoding coding)
		{
			return coding.GetString(Data.ToArray());
		}
		public string Cookies
		{
			get => GetDic('=', ';', ref cookies);
			set => SetDic('=', ';', ref cookies, value?.Replace(':','='));
		}
		public string PostData
		{
			
			get
			{
				var tmp = GetDic('=', '&', ref postData);
				return tmp.Length > 0 ? tmp.Substring(0, tmp.Length - 1) : tmp;
			}
			set => SetDic('=', '&', ref postData, value);
		}
		public string Headers
		{
			get => GetDic(':', '\n', ref headers);
			set => SetDic(':', '\n', ref headers, value);
		}
		public string GetHeaders(string key)
		{
			return headers.ContainsKey(key) ?
			headers[key] : null;
		}
		public void SetHeaders(string key,string value)
		{
			headers[key] = value;
		}
		public Dictionary<string, string> HeadersDic { get => headers; set => headers = value; }
		public byte[] Data { get => data; set => data = value; }

		public HttpContentItem AddPostData(string key,string value)
		{
			postData.Add(key, value);
			return this;
		}
		public HttpContentItem RemoveAllPostData()
		{
			postData.Clear();
			return this;
		}
		private string GetDic(char split, char end,ref Dictionary<string,string> dic)
		{
			if (dic == null) return null;
			var cstr = new StringBuilder();
			foreach (var item in dic)
			{
				cstr.Append(item.Key).Append(split).Append(item.Value).Append(end);
			}
			return cstr.ToString();
		}
		private void SetDic(char split, char end,ref Dictionary<string,string> dic,string value)
		{
			if (value == null) return;
			value=value.Replace(' ', end);
			string[] items = value.Split(end);
			foreach (var item in items)
			{
				if (item.Length < 2) continue;
				string[] info = item.Split(split);
				dic[info[0]] = info[1];
			}
		}
	}
}
