using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet4.Utilities.UtilHttp
{
	using System.Reflection;
	using System.Text.RegularExpressions;

	#region WinHttp 类
	#region WinHttp 类 使用说明


	#endregion
	public enum WinHttpRequestOption
	{
		WinHttpRequestOptionUserAgentString,
		WinHttpRequestOptionUrl,
		WinHttpRequestOptionUrlCodePage,
		WinHttpRequestOptionEscapePercentInUrl,
		WinHttpRequestOptionSslErrorIgnoreFlags,
		WinHttpRequestOptionSelectCertificate,
		WinHttpRequestOptionEnableRedirects,
		WinHttpRequestOptionUrlEscapeDisable,
		WinHttpRequestOptionUrlEscapeDisableQuery,
		WinHttpRequestOptionSecureProtocols,
		WinHttpRequestOptionEnableTracing,
		WinHttpRequestOptionRevertImpersonationOverSsl,
		WinHttpRequestOptionEnableHttpsToHttpRedirects,
		WinHttpRequestOptionEnablePassportAuthentication,
		WinHttpRequestOptionMaxAutomaticRedirects,
		WinHttpRequestOptionMaxResponseHeaderSize,
		WinHttpRequestOptionMaxResponseDrainSize,
		WinHttpRequestOptionEnableHttp11,
		WinHttpRequestOptionEnableCertificateRevocationCheck
	}
	/// <summary>
	/// WinHttpRequest.5.1类
	/// </summary>
	public class WinHttp
	{
		/// <summary>
		/// 自动管理Cookies 默认为 true
		/// </summary>
		bool AutoCookie { get; set; }

		/// <summary>
		/// 域名
		/// </summary>
		private string Domain { get; set; }
		/// <summary>
		/// 调用Com函数或属性的 Type对象
		/// </summary>
		private readonly Type _winhttp;
		//public Type TypeWinHttp { get { return _Winhttp; } }
		/// <summary>
		/// 动态的创建_WinHttpObj对象,通过该对象去访问Com组件的成员
		/// </summary>
		private readonly object _winHttpObj;
		/// <summary>
		/// 执行函数
		/// </summary>
		/// <param name="methodName">方法名</param>
		/// <param name="args">参数数组</param>
		/// <returns></returns>
		public object DoMethod(string methodName, object[] args)
		{
			return this._winhttp.InvokeMember(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static, null, this._winHttpObj, args);
		}

		public object DoMethod(string methodName, object[] args, System.Reflection.ParameterModifier[] paramMods)
		{
			return this._winhttp.InvokeMember(methodName, System.Reflection.BindingFlags.InvokeMethod, null, this._winHttpObj, args, paramMods, null, null);
		}

		/// <summary>
		/// 获取属性与设置属性
		/// </summary>
		/// <param name="propName">访问的属性名</param>
		/// <returns>属性值</returns>
		public object this[string propName]
		{
			get
			{
				return this._winhttp.InvokeMember(propName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.GetProperty, null, this._winHttpObj, null);
			}
			set
			{
				this._winhttp.InvokeMember(propName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.SetProperty, null, this._winHttpObj, new object[] { value });
			}
		}
		public WinHttp(string comName = "WinHttp.WinHttpRequest.5.1")
		{
			//创建 WinHttp对象
			this._winhttp = Type.GetTypeFromProgID(comName);
			if (this._winhttp == null)
				throw new Exception("指定的COM对象名称无效");
			this._winHttpObj = System.Activator.CreateInstance(this._winhttp);
		}
		/// <summary>
		/// 中止正在进行的异步操作
		/// </summary>
		public void StopAbort()
		{
			this.DoMethod("Abort", new object[] { });
		}

		public const int WinHttpRequestOptionEnableRedirects = 6;   //重定向常量值
																	/// <summary>
																	/// 是否禁止重定向
																	/// </summary>
																	/// <param name="value">为0 禁止</param>
		public void SetRedirect(int value = 0)
		{
			this._winhttp.InvokeMember("Option", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty, null, this._winHttpObj, new object[] { 6, 0, null });
			this._winhttp.InvokeMember("Option", System.Reflection.BindingFlags.GetProperty, null, this._winHttpObj, new object[] { 6 });

			//this.DoMethod("put_Option", new object[] { 6, false });
		}

		/// <summary>
		/// 指定是否自动发送凭据  设置当前自动登录策略
		/// </summary>
		/// <param name="autoLogonPolicy">整数型 </param>
		public void SetAutoLogoPolicy(int autoLogonPolicy)
		{
			this.DoMethod("SetAutoLogonPolicy", new object[] { autoLogonPolicy });
		}
		/// <summary>
		/// 指定一个客户端证书   选择一个客户端证书发送到一个安全的超文本传输协议（HTTPS）服务器。
		/// </summary>
		/// <param name="clientCertificate"></param>
		public void SetClientCertificate(string clientCertificate)
		{
			this.DoMethod("SetClientCertificate", new object[] { clientCertificate });
		}

		/// <summary>
		/// 获取 请求之后 响应 html文本 的内容
		/// </summary>
		public string ResponseBody()
		{
			//创建 Adodb.Stream对象 二进制数据流或文本流
			var adoStream = new WinHttp("Adodb.Stream");
			adoStream["Type"] = 1;
			adoStream["Mode"] = 3;
			adoStream.DoMethod("Open", new object[] { });
			adoStream.DoMethod("Write", new object[1] { this["ResponseBody"] });
			adoStream["Position"] = 0;
			adoStream["Type"] = 2;
			adoStream["Charset"] = "utf-8";//GB2312
			return adoStream["ReadText"].ToString();
		}
		/// <summary>
		/// 取状态码 从上次检索响应的HTTP状态代码
		/// </summary>
		/// <returns></returns>
		public int GetStatus()
		{
			var obj = this["Status"];
			return obj != null ? Convert.ToInt32(obj) : 0;
		}
		/// <summary>
		/// 获取状态文本  获取HTTP状态的文本
		/// </summary>
		/// <returns></returns>
		public string GetStatusText()
		{
			var obj = this["StatusText"];
			return obj != null ? obj.ToString() : "";
		}
		/// <summary>
		/// 指定代理服务器配置
		/// </summary>
		/// <param name="proxySetting">默认为 2 可空</param>
		/// <param name="proxyServer">例：192.68.1.1:8080</param>
		/// <param name="bypassList">127.0.0.1 可空</param>
		public void SetProxy(string proxyServer, int proxySetting = 2, string bypassList = "")
		{
			this.DoMethod("SetProxy", new object[] { proxySetting, proxyServer, bypassList });
		}
		/// <summary>
		/// 指定身体验证凭据        
		/// </summary>
		/// <param name="userName">用户名</param>
		/// <param name="password">密码</param>
		/// <param name="flags">标识 可空</param>
		public void SetCredentials(string userName, string password, int flags = 0)
		{
			this.DoMethod("SetCredentials", new object[] { userName, password, flags });
		}

		/// <summary>
		/// 设置 请求头 信息   添加 HTTP 协议头
		/// </summary>
		/// <param name="header">协议头</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public string SetRequestHeader(string header, object value)
		{
			var obj = this.DoMethod("SetRequestHeader", new object[] { header, value });
			return obj != null ? obj.ToString() : "True";
		}

		/// <summary>
		/// 访问HTTP
		/// </summary>
		/// <param name="openMethod">请求的方式 GET/POST</param>
		/// <param name="url">请求的网页地址</param>
		/// <param name="async">是否打开异步方式调用，异步方式不会阻塞</param>
		/// <returns></returns>
		public void Open(string url, string openMethod = "GET", bool async = false)
		{
			if (string.IsNullOrWhiteSpace(url))
				return;
			if (!url.Contains("http://") && !url.Contains("https://"))
				url = "http://" + url;
			// domain = Stringoperation.GetDomain(URL); 取域名 已 停止使用 请使用 Uri u = new Uri(url);
			this.DoMethod("Open", new object[] { openMethod, url, async });
		}

		/// <summary>
		/// 取指定 cookie值
		/// </summary>
		/// <param name="cookieName">cookie名称</param>
		/// <returns>失败返回 空</returns>
		public string GetCookie(string cookieName)
		{
			object obj = this.GetResponseHeader(cookieName);
			return string.IsNullOrWhiteSpace(obj.ToString()) ? "" : obj.ToString();
		}
		/// <summary>
		/// 取出所有Cookies
		/// </summary>
		/// <returns></returns>
		public string GetAllCookies()
		{
			var temp = this.GetAllResponseHeaders();
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

		/// <summary>
		/// 发送数据
		/// </summary>
		/// <param name="body">发送的数据 可空</param>
		/// <returns></returns>
		public void Send(string body = "")
		{
			//if (AutoCookie)
			//{
			//	_cookieManage.ObjectSetCookies(this, this.Domain);
			//}

			this.DoMethod("Send", new object[] { body });

			//if (AutoCookie)
			//{
			//	_cookieManage.ObjectSaveCookies(this, this.Domain);
			//}

		}
		///// <summary>
		///// 清空提交的POST数据
		///// </summary>
		//public void ClearPostData()
		//{
		//    this.PostDataList.Clear();
		//}
		///// <summary>
		///// 增加提交的POST数据
		///// </summary>
		///// <param name="FieldName">数据名称</param>
		///// <param name="Value">数据</param>
		//public void AddPostField(string FieldName, object Value)
		//{
		//    this.PostDataList.Add(FieldName + "=" + Value.ToString());
		//}
		///// <summary>
		///// 通过Post提交数据，就不需要再Send
		///// </summary>
		///// <returns></returns>
		//public string Post()
		//{
		//    if (!_Active)
		//        return "False";
		//    string st = string.Empty;
		//    for (int i = 0; i < this.PostDataCount; i++)
		//    {
		//        if (st != "")
		//            st = st + "&" + PostDataList[i].ToString();
		//        else
		//            st = PostDataList[i].ToString();
		//    }
		//    this.ContentLength = st.Length;
		//    return this.Send(st);    //直接发送出去，并返回
		//}
		/// <summary>
		/// 设置等待超时等 指定超时设置 （以毫秒为单位）
		/// </summary>
		/// <param name="resolveTimeout">解析超时，单位毫秒</param>
		/// <param name="connectTimeout">连接超时，单位毫秒</param>
		/// <param name="sendTimeout">发送超时，单位毫秒</param>
		/// <param name="receiveTimeout">接收超时，单位毫秒</param>
		/// <returns></returns>
		public void SetTimeouts(long resolveTimeout, long connectTimeout, long sendTimeout, long receiveTimeout)
		{
			this.DoMethod("SetTimeouts", new object[4] { resolveTimeout, connectTimeout, sendTimeout, receiveTimeout });
		}
		/// <summary>
		/// 等待数据提交完成 等待异步发送完成（以秒为单位)
		/// </summary>
		/// <param name="timeout"></param>
		/// <param name="succeeded"></param>
		/// <returns></returns>
		public string WaitForResponse(object timeout, out bool succeeded)
		{
			const bool succ = false;
			var paramesM = new System.Reflection.ParameterModifier[1];
			paramesM[0] = new System.Reflection.ParameterModifier(2); // 初始化为接口参数的个数
			paramesM[0][1] = true; // 设置第二个参数为返回参数

			//ParamesM[1] = true;
			var paramArray = new object[2] { timeout, succ };
			var obj = this.DoMethod("WaitForResponse", paramArray, paramesM);
			System.Windows.Forms.MessageBox.Show(paramArray[1].ToString());
			succeeded = bool.Parse(paramArray[1].ToString());
			//Succeeded = bool.Parse(ParamArray[1].ToString);
			return obj != null ? obj.ToString() : "";
		}
		/// <summary>
		/// 获取指定响应 头部信息
		/// </summary>
		/// <param name="header">指定头名称</param>
		/// <returns></returns>
		public string GetResponseHeader(string header)
		{
			var parames = new System.Reflection.ParameterModifier[1];
			parames[0] = new System.Reflection.ParameterModifier(2); // 初始化为接口参数的个数
			parames[0][1] = true;             // 设置第二个参数为返回参数
			var obj = this.DoMethod("GetResponseHeader", new object[] { header }, parames);
			return obj != null ? obj.ToString() : "";
		}
		/// <summary>
		/// 获取所有的头部属性
		/// </summary>
		/// <returns></returns>
		public string GetAllResponseHeaders()
		{
			var obj = this["GetAllResponseHeaders"];
			return obj != null ? obj.ToString() : "";
		}
	}
	#endregion
}