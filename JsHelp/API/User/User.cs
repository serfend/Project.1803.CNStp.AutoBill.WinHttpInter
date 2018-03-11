using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilHttp;
using DotNet4.Utilities.UtilInput;
using DotNet4.Utilities.UtilReg;
using DotNet4.Utilities.UtilVerify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static JsHelp.API.User.LoginHandle;

namespace JsHelp.API.User
{
	public class User
	{
		private RuoKuaiHttp verifier = new RuoKuaiHttp();
		public enum BillStatus
		{
			NotLogin, Login, BeenPay
		}
		private string _JSESSIONID;

		private string encryptKey;

		private string username;
		private string password;
		private string userId;

		private UserInfomation info ;

		private TimeSpan lastUse;
		private BillStatus status;
		/// <summary>
		/// 手动输入
		/// </summary>
		public User()
		{
			info = new UserInfomation(this);
		}
		public User(string username, string password):this()
		{
			this.username = username;
			this.password = password;
		}

		public string Username { get => username; set => username = value; }
		public string Password { get => password; set => password = value; }
		public TimeSpan LastUse { get => lastUse;  set => lastUse = value; }
		internal BillStatus Status { get => status;  set => status = value; }
		public string JSESSIONID { get => _JSESSIONID;  set => _JSESSIONID = value; }
		public string LoginId { get => Info.LoginId; set => Info.LoginId = value; }
		public RuoKuaiHttp Verifier { get => verifier; set => verifier = value; }
		public string EncryptKey { get => encryptKey; set => encryptKey = value; }
		internal UserInfomation Info { get => info; set => info = value; }
		public string UserId { get => userId; set => userId = value; }

		/// <summary>
		/// 登录状态下获取用户名
		/// </summary>
		/// <returns></returns>
		public void JSONPmemtitleaciton(Action<string>CallBack)
		{
			var http = new HttpClient();
			http.Item.Request.Cookies = this.JSESSIONID;
			http.GetHtml("http://jiyou.main.11185.cn/JSONPmemtitleaciton.html?jsoncallback=success_jsonpCallback&_1520750510311=", callBack: (x) => {
				CallBack?.Invoke(x.response.DataString());
			});
		}
		/// <summary>
		/// emp:{"msg":"18260633","ok":true}
		/// </summary>
		/// <param name="CallBack"></param>
		public void JSONGetUserDefaultAddressByUserID(Action<string> CallBack=null)
		{
			BillPageInfoGet("http://jiyou.retail.11185.cn/retail/JSONGetUserDefaultAddressByUserID.html", string.Format("buyer_user_id={0}", this.userId), (x) =>
			{
				CallBack?.Invoke(x);
			});
		}
		/// <summary>
		/// 获取用户已存地址 emp:[{"address":"北京北京市东城区二锅头街233号","city":"110100","contextName":"233","country":"I","defAddress":0,"district":"110101","id":18260633,"mobile":"13627271134","province":"110000","type":0,"userId":26663606,"zipcode":"110101"}]
		/// </summary>
		/// <param name="CallBack"></param>
		public void JSONGetUserAddressWithUserID(Action<string>CallBack=null)
		{
			BillPageInfoGet("http://jiyou.retail.11185.cn/retail/JSONGetUserAddressWithUserID.html", string.Format("buyer_user_id={0}", this.userId), (x) =>
			{
				this.info.Adress = x;
				CallBack?.Invoke(x);
			});
		}
		private void BillPageInfoGet(string url,string postData,Action<string>CallBack=null)
		{
			var item = new HttpItem()
			{
				Url = url,
				Referer = "http://jiyou.retail.11185.cn/retail/initPageForBuyNow.html",
				Method = "post",
				Request = new HttpContentItem()
				{
					PostData =postData
				},
			};
			item.Request.HeadersDic["Orgin"] = "http://jiyou.retail.11185.cn";
			item.Request.HeadersDic["X-Requested-With"] = "XMLHttpRequest";
			ByCASAuth(item, (x) => {
				CallBack?.Invoke(x);
			});
		}
		/// <summary>
		/// 在要求登录时使用此方法返回
		/// </summary>
		/// <param name="target"></param>
		/// <param name="CallBack"></param>
		public void ByCASAuth(HttpItem target, Action<string> CallBack) {
			var http = new HttpClient() {Item=target };
			
			http.GetHtml(cookies: this.JSESSIONID, callBack: (rawInfo) => {
				if (rawInfo.response.GetHeader("Location") != null)
				{
					http.GetHtml(string.Format("https://passport.11185.cn:8001/cas/tlogin?service={0}", target.Url), cookies: this.JSESSIONID, callBack: (x) => {
						var tickUrl = x.response.GetHeader("Location");
						http.GetHtml(tickUrl, cookies: this.JSESSIONID, callBack: (y) => {
							var resultUrl = x.response.GetHeader("Location");
							this.JSESSIONID += x.response.Cookies;
							http.GetHtml(resultUrl, cookies: this.JSESSIONID, callBack: (z) =>
							{
								CallBack?.Invoke(y.response.DataString());
							});
						});
					});
				}
				else
				{
					CallBack?.Invoke(rawInfo.response.DataString());
				}
			});
		}
		public void InputUserByUser()
		{
			var regUser = new Reg().In("Setting").In("defaultUser");
			this.username = InputBox.ShowInputBox("用户建立", "用户名", regUser.GetInfo("TestUsername"), (x) => regUser.SetInfo("TestUsername", x));
			this.password = InputBox.ShowInputBox("用户建立", "密码", regUser.GetInfo("TestPassword"), (x) => regUser.SetInfo("TestPassword", x));
		}
		public bool Login()
		{
			var GetJSessionId = new Task<string>(() =>
			{
				var result = LoginHandle.JSESSIONID(out string lt, out string execution, out string actionUrl);
				this.Info.LoginInfo = new LoginForm(lt, execution, actionUrl);
				return result;
			});
			var CheckJessionId=GetJSessionId.ContinueWith<bool>((x)=> {
				JSESSIONID = x.Result;
				return JSESSIONID != null;
			});
			var GetVerifyImgAndOutPutResult = CheckJessionId.ContinueWith<string>((x)=> {
				if (!x.Result) throw new LoginInitFailException("JSessionId获取失败");
				 var img= LoginHandle.GetVerifyImg(this);
				return Verifier.GetVer(RuoKuaiHttp.ver3040Config, img, out info.LoginId); 
			});
			var GetLoginInit = GetVerifyImgAndOutPutResult.ContinueWith<string>((verifyResult) => {
				try
				{
					var loginLocation = LoginHandle.GetLoginLocation(Info.LoginInfo.lt, Info.LoginInfo.execution, verifyResult.Result, Info.LoginInfo.actionUrl, this);
					this.Status = BillStatus.Login;
					return LoginHandle.InitUserInfo(loginLocation, this);
				}
				catch (VerifyFailedException ex)
				{
					this.LogInfo("登录失败"+ex.Message);
					new TestMethod(this).Login();
				}
				catch (UserPasswordFailedException)
				{
					System.Windows.Forms.MessageBox.Show("登录失败:密码错误");
				}
				return null;
			});
			var GetUserInfo = GetLoginInit.ContinueWith((initInfo) =>
			{
				//TODO 如何在上一个Task取消后取消后面所有的Task
				if (initInfo.Result == null) return "获取数据失败";
				this.JSONPmemtitleaciton((x) =>
				{
					this.UserId =Convert.ToInt32( initInfo.Result).ToString();
					JSONGetUserDefaultAddressByUserID((checkValidResult) => {

						JSONGetUserAddressWithUserID((address) => this.LogInfo("JSONGetUserAddressWithUserID:" + address));
					});
					
					System.Windows.Forms.MessageBox.Show("用户ID:" + initInfo.Result +",json:"+x, "登录成功");
				});
				
				return null;
			});
			GetJSessionId.Start();
			return true;
		}

		[Serializable]
		public class LoginInitFailException : Exception
		{
			public LoginInitFailException() { }
			public LoginInitFailException(string message) : base(message) { }
			public LoginInitFailException(string message, Exception inner) : base(message, inner) { }
			protected LoginInitFailException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
		internal void LoginFailed(string reason)
		{
			Logger.SysLog(this.ToString() + "登录失败：" + reason);
		}
		public override string ToString()
		{
			return this.Username + "(" + this.Password + ")";
		}

		internal void LogInfo(string info)
		{
			Logger.SysLog(this.Username + ":" + info);
		}
	}
	class UserInfomation
	{
		private string phone;
		private string adress;
		internal string LoginId;//依据验证码识别id
		private User parent;

		public UserInfomation(User parent)
		{
			this.parent = parent;
		}

		public string Phone
		{
			get => phone;
			set
			{
				var GetUserInfoForm = new Task<UserInfoForm>(() =>
				  {
					  var http = new HttpClient();
					  var infoPage = http.GetHtml("http://jiyou.main.11185.cn/u/modify_editInfo.html", cookies: parent.JSESSIONID).document.response.DataString(Encoding.UTF8);
					  return new UserInfoForm(infoPage, "modifyUser");
				  });
				var taskGetUserInfoForm = GetUserInfoForm.ContinueWith((formPayload) => {
						formPayload.Result["userInfo.mobilePhone"] = value;
						var http = new HttpClient();
						var response = http.GetHtml("http://jiyou.main.11185.cn/u/modifyuser.html", "post", formPayload.Result.ToString(), parent.JSESSIONID, referer: "http://jiyou.main.11185.cn/u/modify_editInfo.html").document.response;
						var info = response.DataString();
						if (info.Contains("您的信息已成功更新"))
						{
							parent.LogInfo("手机号码已修改为:" + value);
							phone = value;
						}
					});
				GetUserInfoForm.Start();
			}
		}
		public LoginForm LoginInfo { get; internal set; }
		public UserInfoForm UserInfo { get; internal set; }
		public string Adress { get => adress; set => adress = value; }
	}
}
