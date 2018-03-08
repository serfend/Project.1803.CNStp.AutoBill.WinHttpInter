using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
			private byte[] data;
			private object responseBody;

			public DocumentReady(ref byte[] data)
			{
				this.Data = data;
			}

			public DocumentReady(object responseBody)
			{
				this.ResponseBody = responseBody;
			}

			public byte[] Data { get => data; set => data = value; }
			public object ResponseBody { get => responseBody; set => responseBody = value; }
		}
	}

	class HttpClient
	{
		public Action<HttpClientChild, HttpApiEvent.DocumentReady> DocumentReady;
		public Action<HttpClientChild, HttpApiEvent.ErrorEventDelegate> BadResponse;
		public Action<HttpClientChild, HttpApiEvent.DataAvailable> DataAvailable;
		private List<HttpClientChild> child = new List<HttpClientChild>();
		public HttpClientChild GetHtml()
		{
			var child = new HttpClientChild(this);
			this.child.Add(child);
			child.DocumentReady += (x,xx) =>
			{
				DocumentReady?.Invoke(x,xx);
			};
			child.DataAvailable += (x, xx) =>
			{
				DataAvailable?.Invoke(x, xx);
			};
			child.BadResponse += (x, xx) =>
			{
				BadResponse?.Invoke(x, xx);
			};
			return child;
		}

	}
	class HttpClientChild
	{
		public Action<HttpClientChild,HttpApiEvent.DocumentReady> DocumentReady;
		public Action<HttpClientChild,HttpApiEvent.ErrorEventDelegate> BadResponse;
		public Action<HttpClientChild,HttpApiEvent.DataAvailable> DataAvailable;
		private HttpClient parent;
		private HttpApi http;
		private HttpItem item;
		public HttpClientChild(HttpClient parent)
		{
			this.parent = parent;
			http = new HttpApi();
			http.OnDocumentReady += (x) =>
			{
				DocumentReady?.Invoke(this, x);
			};
			http.OnHttpError += (x) =>
			{
				BadResponse?.Invoke(this, x);
			};
			http.OnDataAvailable += (x) =>
			{
				DataAvailable?.Invoke(this, x);
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
		public bool GetResponse(out HttpApiEvent.DocumentReady document, HttpItem item = null)
		{
			return GetResponse(out document, item,true);
		}
		/// <summary>
		/// 异步方法
		/// </summary>
		/// <param name="item"></param>
		/// <param name="CallBack"></param>
		/// <returns></returns>
		public bool GetResponse(HttpItem item = null, Action<HttpApiEvent.DocumentReady> CallBack = null)
		{
			return GetResponse(out HttpApiEvent.DocumentReady document, item,false, CallBack);
		}
		public bool GetResponse(out HttpApiEvent.DocumentReady document,HttpItem item=null,bool syn=false,Action<HttpApiEvent.DocumentReady> CallBack=null)
		{
			http.OnDocumentReady += (x) =>
			{
				CallBack?.Invoke(x);
			};
			
			try
			{
				http.Open(item.Method, item.Url, item.Asyn);
				//http.Option[WinHttpRequestOption.WinHttpRequestOptionEnableRedirects] =true;
				http.SetRequestHeader("cookies",item.Request.Cookies);
				if(item.Referer!=null)http.SetRequestHeader("Referer", item.Referer);
				if (item.IfModified) http.SetRequestHeader("IfModified","0");
				http.SetRequestHeader("UserAgent", item.UserAgent);
				if (item.Host != null) http.SetRequestHeader("Host", item.Host);
				if (item.AcceptLanguage != null) http.SetRequestHeader("AcceptLanguage", item.AcceptLanguage);
				foreach(var header in item.Request.HeadersDic)
				{
					http.SetRequestHeader(header.Key, header.Value);
				}
				http.Send(item.Request.PostData);
				if (!syn) document = null; else document = new HttpApiEvent.DocumentReady(http.Data);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + '\n' + ex.StackTrace);
				document = null;
				return false;
			}
			return true;

		}

		
	}
	class HttpItem
	{
		private static List<string> UserAgentBase=new List<string>() {
			"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36 {0}",
			"Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_8; en-us) AppleWebKit/534.50 (KHTML, like Gecko) Version/5.1 Safari/534.50 {0}",
			"Mozilla/5.0 (Windows; U; Windows NT 6.1; en-us) AppleWebKit/534.50 (KHTML, like Gecko) Version/5.1 Safari/534.50$Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0 {0}",
		};
		private bool asyn=false;//默认异步
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
		public string Method { get => method; set => method = value; }
		public string Host { get => host; set => host = value; }
		public string Referer { get => referer; set => referer = value; }
		public string AcceptLanguage { get => acceptLanguage; set => acceptLanguage = value; }
		public bool IfModified { get => ifModified; set => ifModified = value; }
		public FileInfo TargetFile { get => targetFile; set => targetFile = value; }
		public bool Asyn { get => asyn; set => asyn = value; }
		internal HttpContentItem Request { get => request; set => request = value; }
		internal HttpContentItem Response { get => response; set => response = value; }
	}
	class HttpContentItem
	{
		private Dictionary<string,string> cookies;
		private Dictionary<string, string> headers;
		private Dictionary<string, string> postData;
		private List<byte> data;
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
			var cstr = new StringBuilder();
			foreach (var item in dic)
			{
				cstr.Append(item.Key).Append(split).Append(item.Value).Append(end);
			}
			return cstr.ToString();
		}
		private void SetDic(char split, char end,ref Dictionary<string,string> dic,string value)
		{
			string[] items = value.Split(split);
			foreach (var item in items)
			{
				string[] info = item.Split(end);
				dic[info[0]] = info[1];
			}
		}
	}
}
