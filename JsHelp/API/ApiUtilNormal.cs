
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Drawing;
using System.Threading.Tasks;
using DotNet4.Utilities.UtilCode;
using JsHelp.API.User;

namespace JsHelp.API
{
	class UtilNormal:IDisposable
	{
		HttpClient http;
		public UtilNormal()
		{
			http = new HttpClient();
		}

		public void Dispose()
		{
			((IDisposable)http).Dispose();
		}

		public string Send(string url)
		{
			//TODO GetHtml
			return null;
		}

	}
}