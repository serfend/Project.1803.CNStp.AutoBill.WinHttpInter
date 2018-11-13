using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilHttp;
using DotNet4.Utilities.UtilInput;
using DotNet4.Utilities.UtilReg;
using DotNet4.Utilities.UtilVerify;
using JsHelp.API.Request;
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
		private Request.RequestBase userRequest;
		public enum BillStatus
		{
			NotLogin, Login,PasswordWrong, BeenPay
		}
		

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
			userRequest = new RequestBase(this);
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
		public string LoginId { get => Info.LoginId; set => Info.LoginId = value; }
		public RuoKuaiHttp Verifier { get => verifier; set => verifier = value; }
		public string EncryptKey { get => encryptKey; set => encryptKey = value; }
		internal UserInfomation Info { get => info; set => info = value; }
		public string UserId { get => userId; set => userId = value; }
		internal RequestBase UserRequest { get => userRequest; set => userRequest = value; }

		public void InputUserByUser()
		{
			var regUser = new Reg().In("Setting").In("defaultUser");
			this.username = InputBox.ShowInputBox("用户建立", "用户名", regUser.GetInfo("TestUsername"), (x) => regUser.SetInfo("TestUsername", x));
			this.password = InputBox.ShowInputBox("用户建立", "密码", regUser.GetInfo("TestPassword"), (x) => regUser.SetInfo("TestPassword", x));
		}
		public void Login(Action<Exception> CallBack)
		{
			this.Status = BillStatus.NotLogin;
			UserRequest.BuyCenter((lt,execution,actionUrl,cookies)=> {
				if (lt == null){CallBack.Invoke(new LoginInitFailException("初始化登录失败"));return;}
				this.Info.LoginInfo = new LoginForm(lt, execution, actionUrl);
				UserRequest.GetCaptcha((img) => {
					var verifierResult = Verifier.GetVer(RuoKuaiHttp.ver3040Config, img, out info.LoginId);
					UserRequest.GetLoginLocation(verifierResult,(result)=> {
						var resultInfo = result.DataString(Encoding.UTF8);
						if (resultInfo != null){
							if (resultInfo.Contains("用户名或密码错误")){
								LoginFailed("用户名或密码错误");
								Status = BillStatus.PasswordWrong;
								CallBack.BeginInvoke(new UserPasswordFailedException(), (obj) => { }, null);
								return;
							}
							else if (resultInfo.Contains("验证码错误")){
								LoginFailed("验证码错误");
								CallBack.BeginInvoke(new VerifyFailedException("报告验证码错误" + LoginId + "返回结果:" + Verifier.Report(RuoKuaiHttp.ver3040Config, LoginId)), (obj) => { },null);
								return;
							}
							else if (resultInfo.Contains("在试图完成你的请求时出错。请通知你的技术支持或重试。")){
								LoginFailed("CAS出错");
								CallBack.BeginInvoke(new WebCASNotSupportException(), (obj) => { }, null);
								return;
							}
							else if (resultInfo.Contains("输错密码次数太多，账号被锁定。"))
							{
								LoginFailed("账号被封禁");
								CallBack.BeginInvoke(new AccountAccessDenyException(), (obj) => { }, null);
								return;
							}
						}else throw new Exception("异常操作");
						var location = result.GetHeader("Location");//HttpUtil.GetElement(resultInfo, "<a href=\"", "\"").Replace("&#59;","?");
						this.Status = BillStatus.Login;
						UserRequest.InitLoginInfo(location,(initInfo)=> {
							UserRequest.JSONPmemtitleaciton((x) =>{
								if (x == null) Status = BillStatus.NotLogin;
								else
								{
									this.UserId = Convert.ToInt32(initInfo).ToString();
									Logger.SysLog("用户:" + UserId + "登录成功,json:" + x);
									
								}
								CallBack?.BeginInvoke(null, (obj) => { }, null);
							}, true);

							});
						});
					});
				});
		}

		[Serializable]
		public class AccountAccessDenyException : Exception
		{
			public AccountAccessDenyException() { }
			public AccountAccessDenyException(string message) : base(message) { }
			public AccountAccessDenyException(string message, Exception inner) : base(message, inner) { }
			protected AccountAccessDenyException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
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
					  parent.UserRequest.Http.ClearItem();
					  var infoPage = parent.UserRequest.Http.GetHtml("http://jiyou.main.11185.cn/u/modify_editInfo.html").document.response.DataString(Encoding.UTF8);
					  return new UserInfoForm(infoPage, "modifyUser");
				  });
				var taskGetUserInfoForm = GetUserInfoForm.ContinueWith((formPayload) => {
						formPayload.Result["userInfo.mobilePhone"] = value;
						var response = parent.UserRequest.Http.GetHtml("http://jiyou.main.11185.cn/u/modifyuser.html", "post", formPayload.Result.ToString(),referer: "http://jiyou.main.11185.cn/u/modify_editInfo.html").document.response;
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
