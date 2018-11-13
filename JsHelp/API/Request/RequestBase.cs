using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilHttp;
using DotNet4.Utilities.UtilReg;
using JsHelp.API.StampTarget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JsHelp.API.Request
{
	class RequestBase
	{
		HttpClient http;
		private User.User user;

		internal HttpClient Http { get => http; set => http = value; }

		public RequestBase( User.User user)
		{
			this.user = user;
			Http = new HttpClient() { AlwaysClearItem=true };
		}
		public void GetBillInfo(BillBuild bill,Action<string> CallBack = null)
		{
			Http.GetHtml("http://jiyou.retail.11185.cn/retail/initPageForBuyNow.html", "post", string.Format("buyGoodsNowBean.goods_id={0}&buy_type={1}&buyGoodsNowBean.goods_attr_id={2}&buyGoodsNowBean.goods_num={3}&goodsTicketAttr={4}",
				bill.Goods.Goods_id,
				bill.Buy_type,
				bill.Goods.Goods_attr_id,
				bill.Goods.Goods_num,
				bill.GoodsTicketAttr
		), callBack: (x) => CallBack.Invoke(x.response.DataString()));
		}

		/// <summary>
		/// CAS登录重认证流程
		/// </summary>
		/// <returns></returns>
		public void JSONPmemtitleaciton( Action<string> CallBack, bool isBillPage = false)
		{
			Http.ClearItem();
			var targetUrl = isBillPage ? "http://jiyou.main.11185.cn/JSONPmemtitleaciton.html?jsoncallback=success_jsonpCallback&_1520750510311=" : "http://jiyou.retail.11185.cn/JSONPmemtitleaciton.html?jsoncallback=success_jsonpCallback&_1521283411279=";
			Http.GetHtml(targetUrl, callBack: (x) => {
				var location = x.response.GetHeader("Location");
				if (location != null)
				{
					Logger.SysLog(user.Username + "进行重新认证");
					Http.Item.Referer = "http://jiyou.retail.11185.cn/memtitle.html";
					Http.GetHtml(location, callBack: (casLocation) => {
						var callBackLocation = casLocation.response.GetHeader("Location");
						if (callBackLocation == null) return;
						Http.GetHtml(callBackLocation, callBack: (loginLocation) => {
							var successLocation = loginLocation.response.GetHeader("Location");
							if (successLocation == null)
							{
								Logger.SysLog("JSONPmemtitleaciton失败!");
							}
							else
							{
								Http.GetHtml(successLocation, callBack: (newWebInfoGet) => {
									var info = newWebInfoGet.response.DataString();
									if (info.Contains("execution"))
									{
										Logger.SysLog(user.Username + "要求重新登录");
										CallBack?.Invoke(null);
									}
									else
										CallBack?.Invoke(info);
								});
							}
						});
					});
				}
				else
				{
					var info = x.response.DataString();
					CallBack?.Invoke(x.response.DataString());
				}

			});
		}

		#region infomationHandle

		#endregion

		#region CASHandle
		public void JSONGetUserInfoByUserId(Action<string>CallBack,Stamp stamp)
		{

		}
		#endregion
		#region loginHandleq=
		public void BuyCenter(Action<string,string,string,string>CallBack)
		{
			
			Http.GetHtml("http://jiyou.main.11185.cn/u/buyerCenter.html",callBack:(x)=> {
				var newUrl = x.response.GetHeader("Location");
				if (newUrl == null)
				{
					Logger.SysLog("登录初始化失败");
					CallBack.BeginInvoke(null, null,null,null,(obj)=> { },null);
					return;
				}
				var loginPage = Http.GetHtml(newUrl,clientId:user.Username);
				var responseContent = loginPage.document.response.DataString(Encoding.UTF8);
				var responseHeaders = loginPage.document.response.HeadersDic;
				var actionUrl = "https://passport.11185.cn:8001" + HttpUtil.GetElement(responseContent, "action=\"", "\"");
				var lt = HttpUtil.GetElement(responseContent, "name=\"lt\" value=\"", "\"");
				var excution = HttpUtil.GetElement(responseContent, "name=\"execution\" value=\"", "\"");
				var cookies= loginPage.document.response.Cookies;
				CallBack.BeginInvoke(lt, excution, actionUrl, cookies,(obj)=> { },null);
			},clientId:user.Username);
		}
		public void GetCaptcha(Action<byte[]> CallBack)
		{
			var captcha = Http.GetHtml("https://passport.11185.cn:8001/cas/captcha/captcha", host: "passport.11185.cn:8001", referer: "https://passport.11185.cn:8001/cas/tlogin?service=http://jiyou.11185.cn/index.html", clientId: user.Username);
			var encrypt = Http.GetHtml("https://passport.11185.cn:8001/cas/account/encryptKey", host: "passport.11185.cn:8001", referer: "https://passport.11185.cn:8001/cas/tlogin?service=http://jiyou.11185.cn/index.html");
			user.EncryptKey = encrypt.document.response.DataString(Encoding.UTF8);
			CallBack.BeginInvoke(captcha.document.Data,(obj)=> { },null);
		}
		public void GetLoginLocation(string code, Action<HttpContentItem> CallBack)
		{
			var modulus = HttpUtil.GetElement(user.EncryptKey, "modul\":\"", "\"");
			var exponent = HttpUtil.GetElement(user.EncryptKey, "exponent\":\"", "\"");
			var encodedPassworder = new Password.PasswordEncoder(exponent, modulus);
			//var encodedPassworder = new Password.PasswordEncoder();
			var encodedPassword = encodedPassworder.GetPasswordRAS(user.Password);
			Http.Item.Request.HeadersDic["Origin"] = "https://passport.11185.cn:8001";
			var document =Http.GetHtml(user.Info.LoginInfo.actionUrl, host: "passport.11185.cn:8001", referer: "https://passport.11185.cn:8001/cas/tlogin?service=http://jiyou.11185.cn/index.html", method: "post", postData: string.Format("username={0}&password={1}&code={2}&lt={3}&execution={4}&_eventId=submit", user.Username, encodedPassword, code, user.Info.LoginInfo.lt, user.Info.LoginInfo.execution), clientId: user.Username);
			CallBack?.BeginInvoke(document.document.response, (obj) => { }, null);
		}
		public void InitLoginInfo(string loginLocation, Action<string> CallBack)
		{
			if (loginLocation == null)
				return;
			var userInfo =Http.GetHtml(loginLocation).document.response;
			var newUrl = userInfo.GetHeader("Location");
			var userInfoDoc = Http.GetHtml(loginLocation).document.response.DataString(Encoding.UTF8);
			CallBack.BeginInvoke( HttpUtil.GetElement(userInfoDoc, "<font color=\"red\">", "</font>"),(obj)=> { },null);
		}
		#endregion
	}

}
