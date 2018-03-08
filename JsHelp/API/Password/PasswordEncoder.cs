using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsHelp.API.Password
{
	class PasswordEncoder:JShelp
	{
		private string exponent;
		private string modulus;

		public PasswordEncoder(string exponent, string modulus)
		{
			this.exponent = exponent;
			this.modulus = modulus;
		}
		public PasswordEncoder()
		{
			Exponent = "010001";
			Modulus = "00c7b96183ded273261211f29dfaf7f95f4138019971b9872238e8adb4f2d6dfabbf17baa558ba6fcec663ebc6ee40c6759e9734078b4b2e3f7535d21846da59ed798f5757b4a43606bee29bd82681bec97cac697a11c38105da52df1718b5deff139c8e1cab2452cfd2c7f0748ba82e7370b0da2de88b4dfd74e26a380612936d";
		}
		public string Exponent { get => exponent; set => exponent = value; }
		public string Modulus { get => modulus; set => modulus = value; }

		public string GetPasswordRAS(string raw)
		{
			Cmd("setMaxDigits(200);");
			Cmd("var key = getKeyPair('" + exponent + "', '', '" + modulus + "');");
			Cmd("var pwd = encodeURIComponent('" + raw + "');");
			Cmd("pwd = encryptedString(key, pwd);");
			Cmd("pwd");
			var result = ExcuteScript(new string[] { "password.js" });
			return result.ToString();
		}
	}
}
