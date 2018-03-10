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
	class User
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
				catch (VerifyFailedException)
				{
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
				System.Windows.Forms.MessageBox.Show("用户ID:"+initInfo.Result,"登录成功");
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
	}
}
