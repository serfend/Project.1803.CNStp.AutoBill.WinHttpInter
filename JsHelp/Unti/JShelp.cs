using MSScriptControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsHelp
{
	class JShelp
	{
		StringBuilder cmdInfo=new StringBuilder();

		public void Cmd(string expressionLine)
		{
			cmdInfo.Append(expressionLine).Append("\r\n");
		}
		public void Clear()
		{
			cmdInfo.Clear();
		}
		public object ExcuteScript(string[] jsPath)
		{
			return ExcuteScript(cmdInfo.ToString(), jsPath);
		}
		public object ExcuteScript(string expression,  string[] jsPath)
		{
			var cstr = new StringBuilder();
			foreach (var jsFile in jsPath)
			{
				string js = System.IO.File.ReadAllText(jsFile);
				cstr.Append(js).Append("\r\n");
			}
			object o = ExcuteScript(expression, cstr.ToString());
			return o;
		}
		public object ExcuteScript(string sExpression, string sCode)
		{
			var scriptControl = new ScriptControl
			{
				UseSafeSubset = true,
				Language = "JavaScript"
			};
			scriptControl.AddCode(sCode);
			try
			{
				return scriptControl.Eval(sExpression);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return null;
		}
	}
}
