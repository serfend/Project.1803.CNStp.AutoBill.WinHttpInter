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
		private bool CheckStampStatus()
		{
			if (stamp == null) { Logger.SysLog("CheckStampStatus().StampNotInitialize");return false; }
			return true;
		}
		public TestMethod(User.User user)
		{
			this.user = user;
		}
		public bool Login()
		{
			DotNet4.Utilities.UtilHttp.HttpItem.UsedFidder = true;
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
		public bool SynStampInfo(bool showMsg=true)
		{
			if (!CheckUserStatus()) return false;
			var stampReg = new Reg().In("Setting").In("defaultUser");
			try
			{
				stamp = new StampTarget.Stamp(user,
					InputBox.ShowInputBox("邮票", "邮票的id", stampReg.GetInfo("stampId"), (x) => stampReg.SetInfo("stampId", x)),
					InputBox.ShowInputBox("邮票", "购买数量", stampReg.GetInfo("stampBuyNum"), (x) => stampReg.SetInfo("stampBuyNum", x)),
					(loadedStamp) => {
						if (showMsg)
						{
							loadedStamp.ShowOut();
						}
					});
			}
			catch (Exception ex)
			{
				Logger.SysLog("获取邮票数据失败:" + ex.Message);
			}
			return true;
		}
		public bool SynBillInfo()
		{
			if (!CheckUserStatus()) return false;
			var task = new Task(() => SynStampInfo(false));
			var synBiill = task.ContinueWith((x) => {
				do
				{
					Task.Delay(500);//等待邮票数据初始化
				} while (!stamp.SynBillInfo(user));
			});
			task.Start();
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
