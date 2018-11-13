using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsHelp.API.User
{
	static class UserList
	{
		private static List<User> list;
		static UserList()
		{
			list = new List<User>();
			var userAccount = File.ReadAllLines("账号.txt");
			foreach(var account in userAccount)
			{
				var info = account.Split('$');
				var user = new User(info[0], info[1]);
				list.Add(user);
			}
		}
		public static User GetUser(int index)
		{
			return list[index];
		}
		public static User GetUser(string userName)
		{
			return list.Find((s)=>  s.Username.Equals(userName));
		}
		public static int Count { get => list.Count; }
	}
}
