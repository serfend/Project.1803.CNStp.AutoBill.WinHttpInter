using DotNet4.Utilities.UtilInput;
using DotNet4.Utilities.UtilReg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsHelp.API
{
	class TestMethod
	{
		private User.User user;
		public bool Login()
		{
			user = new User.User();
			user.InputUserByUser();
			return this.Login(user);
			
		}
		public bool Login(User.User user)
		{
			return user.Login();
			try
			{
				return user.Login();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return this.Login(user);
			}
		}
		public bool ModifyPhone()
		{
			if (user.Status!=User.User.BillStatus.Login) throw new UserNotLoginException();
			var userReg = new Reg().In("Setting").In("defaultUser");
			try
			{
				user.Info.Phone = InputBox.ShowInputBox("修改数据", "新的手机号", userReg.GetInfo("phone"), (x) => userReg.SetInfo("phone", x));
			}
			catch (Exception ex)
			{
				Console.WriteLine("修改用户手机号失败"+ex.Message);
				return false;
			}
			return true;
		}

		[Serializable]
		public class UserNotLoginException : Exception
		{
			public UserNotLoginException() { }
			public UserNotLoginException(string message) : base(message) { }
			public UserNotLoginException(string message, Exception inner) : base(message, inner) { }
			protected UserNotLoginException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
	}
}
