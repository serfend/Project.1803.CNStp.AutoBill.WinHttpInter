using DotNet4.Utilities.UtilCode;
using System.Collections.Generic;
using System.Text;

namespace JsHelp.API.User
{
	public class UserInfoForm
	{
		private Dictionary<string,string> fromInfos;
		public UserInfoForm(IList<string> fromInfos)
		{
			this.fromInfos = new Dictionary<string, string>(fromInfos.Count);
			foreach (var info in fromInfos)
			{
				var key = HttpUtil.GetElement(info, "name=\"", "\"");
				var value = HttpUtil.GetElement(info, "value=\"", "\"");
				if(key!=null)this.fromInfos[key] = value;
			}
		}
		public string this[string key]
		{
			get => fromInfos[key];
			set => fromInfos[key] = value;
		}
		public override string ToString()
		{
			var cstr = new StringBuilder();
			foreach(var info in fromInfos)
			{
				cstr.Append(info.Key).Append("=").Append(info.Value).Append("&");
			}
			return cstr.ToString(0, cstr.Length-1);
		}
	}
}