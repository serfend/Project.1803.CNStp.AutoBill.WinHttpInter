using DotNet4.Utilities.UtilCode;
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
		public Action<HttpApiEvent.DataAvailable> OnDataAvailable;
		private List<byte> data;
		public HttpApi()
		{

			this.OnError += HttpApi_OnError;
			this.OnResponseDataAvailable += Http_OnResponseDataAvailable;
			this.OnResponseFinished += HttpApi_OnResponseFinished;
			this.OnResponseStart += HttpApi_OnResponseStart;
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
		public List<byte> Data { get => data; set => data = value; }

		private void HttpApi_OnError(int ErrorNumber, string ErrorDescription)
		{
			OnHttpError?.Invoke(new HttpApiEvent.ErrorEventDelegate(ErrorNumber, ErrorDescription));
		}

		private void HttpApi_OnResponseFinished()
		{
			OnDocumentReady?.Invoke(new HttpApiEvent.DocumentReady(Data));
		}

		private void HttpApi_OnResponseStart(int Status, string ContentType)
		{
			Data = new List<byte>(1024);
		}

		private void Http_OnResponseDataAvailable(ref Array Data)
		{
			foreach(var byteInfo in Data)
			{
				this.Data.Add((byte)byteInfo);
			}
			OnDataAvailable?.Invoke(new HttpApiEvent.DataAvailable(ref Data));
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
				item.Data = http.Data;
			}

			public HttpDocument(object responseBody, string headers, HttpContentItem item) : base(responseBody)
			{
				SetResponse(item, headers);
				var data =new List<byte>();
				foreach(var byt in this.Data)
				{
					data.Add(byt);
				}
				item.Data = data;
			}
			private void SetResponse(HttpContentItem item,string headers)
			{
				response = item;
				response.Cookies = HttpApi.GetAllCookies(headers);
			}
		}
		public class DataAvailable
		{
			private Array data;

			public DataAvailable(ref Array data)
			{
				this.data = data;
			}

			public Array Data { get => data; set => data = value; }
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
				this.Data = ObjectToBytes(responseBody);
			}
			public static byte[] ObjectToBytes(object obj)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					IFormatter formatter = new BinaryFormatter(); formatter.Serialize(ms, obj); return ms.GetBuffer();
				}
			}
		}
	}

	class HttpClient
	{
		public Action<HttpClientChild, HttpApiEvent.DocumentReady> DocumentReady;
		public Action<HttpClientChild, HttpApiEvent.ErrorEventDelegate> BadResponse;
		public Action<HttpClientChild, HttpApiEvent.DataAvailable> DataAvailable;
		private List<HttpClientChild> child = new List<HttpClientChild>();
		private HttpItem item;

		internal HttpItem Item { get => item; set => item = value; }

		public HttpClientChild GetHtml(string url=null,string method=null,string postData=null,string cookies=null,string userAgent=null,string host=null,string referer =null ,bool ifModifies=true,int timeOut=30000,string clientId =null,Action<HttpApiEvent.HttpDocument>callBack=null, HttpApiEvent.HttpDocument responseDocument=null)
		{
			var child = new HttpClientChild(this) {
				ID = clientId
			};
			this.child.Add(child);
			child.DocumentReady += (x,xx) =>
			{
				DocumentReady?.Invoke(x,xx);
				callBack?.Invoke(xx);
			};
			child.DataAvailable += (x, xx) =>
			{
				DataAvailable?.Invoke(x, xx);
			};
			child.BadResponse += (x, xx) =>
			{
				BadResponse?.Invoke(x, xx);
			};
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
			do
			{
				bool sendSuccess = false;
				if (callBack != null)
					sendSuccess=child.GetResponse(item, callBack);
				else
					sendSuccess=child.GetResponse(out responseDocument, item);
				if (sendSuccess) return child;
			} while (true);
		}

	}
	class HttpClientChild
	{
		public Action<HttpClientChild,HttpApiEvent.HttpDocument> DocumentReady;
		public Action<HttpClientChild,HttpApiEvent.ErrorEventDelegate> BadResponse;
		public Action<HttpClientChild,HttpApiEvent.DataAvailable> DataAvailable;
		private HttpClient parent;
		private HttpApi http;

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

		public HttpClientChild(HttpClient parent)
		{
			this.parent = parent;
			http = new HttpApi();
			http.OnHttpError += (x) =>
			{
				BadResponse?.Invoke(this, x);
			};
			http.OnDataAvailable += (x) =>
			{
				DataAvailable?.Invoke(this, x);
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
		public bool GetResponse(out HttpApiEvent.HttpDocument document, HttpItem item = null)
		{
			return GetResponse(out document, item,true);
		}
		/// <summary>
		/// 异步方法
		/// </summary>
		/// <param name="item"></param>
		/// <param name="CallBack"></param>
		/// <returns></returns>
		public bool GetResponse(HttpItem item = null, Action<HttpApiEvent.HttpDocument> CallBack = null)
		{
			return GetResponse(out HttpApiEvent.HttpDocument document, item,false, CallBack);
		}
		public bool GetResponse(out HttpApiEvent.HttpDocument document,HttpItem item=null,bool syn=false,Action<HttpApiEvent.HttpDocument> CallBack=null)
		{
			if (item != null)
			{
				Item = item;
			}
			proxyBeginTime = HttpUtil.TimeStamp;
			if (CallBack!=null)//当有回调时则触发自身的DocumentReady
				http.OnDocumentReady += (x) =>
				{
					CallBack.Invoke(SetDocument(x));
				};
			else
				http.OnDocumentReady += (x) =>
				{
					DocumentReady?.Invoke(this, SetDocument(x));
				};
			try
			{
				http.Open(Item.Method, Item.Url, Item.Asyn);
				//http.Option[WinHttpRequestOption.WinHttpRequestOptionEnableRedirects] =true;
				var cookies = Item.Request.Cookies;
				if(cookies!=null&&cookies.Length>0)http.SetRequestHeader("cookies", cookies);
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
					http.Send(Item.Request.PostData);
				}
				else
				{
					http.Send();
				}

				if (!syn) document = null; else
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
				return false;
			}
			return true;

		}
		private HttpApiEvent.HttpDocument SetDocument(HttpApiEvent.DocumentReady document)
		{
			return new HttpApiEvent.HttpDocument(ref document,ref this.http,Item.Response);
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
					return string.Format(UserAgentBase[random], UserAgentBase, randomValue);
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
	}
	public class HttpContentItem
	{
		private Dictionary<string,string> cookies;
		private Dictionary<string, string> headers;
		private Dictionary<string, string> postData;
		private List<byte> data;
		public HttpContentItem()
		{
			cookies = new Dictionary<string, string>();
			headers = new Dictionary<string, string>();
			postData = new Dictionary<string, string>();
		}

		public string DataString(Encoding coding)
		{
			return coding.GetString(data.ToArray());
		}
		public string Cookies
		{
			get => GetDic('=', ';', ref cookies);
			set => SetDic('=', ';', ref cookies, value);
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
			return headers[key];
		}
		public void SetHeaders(string key,string value)
		{
			headers[key] = value;
		}
		public List<byte> Data { get => data; set => data = value; }
		public Dictionary<string, string> HeadersDic { get => headers; set => headers = value; }

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
			string[] items = value.Split(split);
			foreach (var item in items)
			{
				string[] info = item.Split(end);
				dic[info[0]] = info[1];
			}
		}
	}
}
