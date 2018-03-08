using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilInput;
using DotNet4.Utilities.UtilReg;
using DotNet4.Utilities.UtilVerify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsHelp.API.User
{
	class User
	{
		private RuoKuaiHttp verifier = new RuoKuaiHttp();
		public enum BillStatus
		{
			NotLogin,Login,BeenPay
		}
		private string _JSESSIONID;

		private string encryptKey;

		private string username;
		private string password;
		
		private UserInfomation info=new UserInfomation();

		private TimeSpan lastUse;
		private BillStatus status;

		/// <summary>
		/// 手动输入
		/// </summary>
		public User()
		{

		}
		public User(string username, string password)
		{
			this.username = username;
			this.password = password;
		}

		public string Username { get => username; set => username = value; }
		public string Password { get => password; set => password = value; }
		public TimeSpan LastUse { get => lastUse;private set => lastUse = value; }
		internal BillStatus Status { get => status;private set => status = value; }
		public string JSESSIONID { get => _JSESSIONID;private set => _JSESSIONID = value; }
		public string LoginId { get => Info.LoginId; set => Info.LoginId = value; }
		public RuoKuaiHttp Verifier { get => verifier; set => verifier = value; }
		public string EncryptKey { get => encryptKey; set => encryptKey = value; }
		internal UserInfomation Info { get => info; set => info = value; }

		public void InputUserByUser()
		{
			var regUser = new Reg().In("Setting").In("defaultUser");
			this.username = InputBox.ShowInputBox("用户建立", "用户名", regUser.GetInfo("TestUsername"), (x) => regUser.SetInfo("TestUsername", x));
			this.password = InputBox.ShowInputBox("用户建立", "密码", regUser.GetInfo("TestPassword"),(x) => regUser.SetInfo("TestPassword", x));
		}
		public bool Login()
		{
			JSESSIONID = LoginHandle.JSESSIONID(out string lt,out string execution,out string actionUrl,out string responseContent);
			if (JSESSIONID == null) return false;
			var verifyImg = LoginHandle.GetVerifyImg(this);
			var verifyResult = Verifier.GetVer(RuoKuaiHttp.ver3040Config, verifyImg, out string verifyId);
			Info.LoginId = verifyId;
			var loginLocation = LoginHandle.GetLoginLocation(lt, execution, verifyResult, actionUrl, this);
			this.Status=BillStatus.Login;
			return true;
		}

		internal void LoginFailed(string reason)
		{
			Console.WriteLine(this.ToString() +"登录失败：" + reason);
		}
		public override string ToString()
		{
			return this.Username + "(" + this.Password + ")";
		}
	}
	class UserInfomation
	{
		private string phone;
		private string loginId;//依据验证码识别id
		public string Phone { get => phone;
			set {
				//TODO 用户登录状态时可通过用户信息页修改
				phone = value;
			}
		}

		public string LoginId { get => loginId; set => loginId = value; }
	}
}
