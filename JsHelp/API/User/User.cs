using DotNet4.Utilities.UtilCode;
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

		private UserInfomation info = new UserInfomation();

		private TimeSpan lastUse;
		private BillStatus status;
		private CancellationTokenSource loginCancel;
		/// <summary>
		/// 手动输入
		/// </summary>
		public User()
		{
			loginCancel = new CancellationTokenSource();
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
			}, loginCancel.Token);
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
				finally { loginCancel.Cancel(); }
				return null;
			});
			var GetUserInfo = GetLoginInit.ContinueWith((initInfo) =>
			{
				//TODO 如何在上一个Task取消后取消后面所有的Task
				if (initInfo.Result == null) return "获取数据失败";
				LoginHandle.GetUserInfo(this);
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
			Console.WriteLine(this.ToString() + "登录失败：" + reason);
		}
		public override string ToString()
		{
			return this.Username + "(" + this.Password + ")";
		}
	}
	class UserInfomation
	{
		private string phone;
		internal string LoginId;//依据验证码识别id
		public string Phone
		{
			get => phone;
			set
			{
				//TODO 用户登录状态时可通过用户信息页修改
				phone = value;
			}
		}
		public LoginForm LoginInfo { get; internal set; }
	}
}
