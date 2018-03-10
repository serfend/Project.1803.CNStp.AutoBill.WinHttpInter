using DotNet4.Utilities.UtilInput;
using DotNet4.Utilities.UtilReg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsHelp.API
{
	class TestMethod
	{
		private User.User user;
		private StampTarget.Stamp stamp;
		public TestMethod() {}
		private bool CheckUserStatus()
		{
			if (user == null) { Logger.SysLog("CheckUserStatus().UserNotInitialize");return false; }
			if (user.Status != User.User.BillStatus.Login) { Logger.SysLog("CheckUserStatus().UserNotLogin." + user.Username);return false; }
			return true;
		}
		public TestMethod(User.User user)
		{
			this.user = user;
		}
		public bool Login()
		{
			if (user == null)
			{
				user = new User.User();
				user.InputUserByUser();
			}
			return user.Login();
		}
		public bool ModifyPhone()
		{
			if (!CheckUserStatus())return false;
			var userReg = new Reg().In("Setting").In("defaultUser");
			try
			{
				user.Info.Phone = InputBox.ShowInputBox("修改数据", "新的手机号", userReg.GetInfo("phone"), (x) => userReg.SetInfo("phone", x));
			}
			catch (Exception ex)
			{
				Logger.SysLog("修改用户手机号失败:" + ex.Message);
				return false;
			}
			return true;
		}
		public void SynStampInfo()
		{
			throw new NotImplementedException();
		}
		public bool SynBillInfo()
		{
			if (!CheckUserStatus()) return false;

			return true;
		}
		public bool GetPhoneVerifyCode()
		{
			if (!CheckUserStatus()) return false;
			throw new NotImplementedException();
		}

		public bool GetImgVerifyCode()
		{
			if (!CheckUserStatus()) return false;
			throw new NotImplementedException();
			return true;
		}



		public bool TestSubmitBill()
		{
			if (!CheckUserStatus()) return false;
			throw new NotImplementedException();
			return true;
		}

	}
}
