using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNet4.Utilities.UtilHttp;
namespace JsHelp.Unti
{
	class HttpClientTest
	{
		private HttpClient http;
		public void NormalHttp()
		{
			http = new HttpClient();
			http.GetHtml("http://www.baidu.com",callBack:(x)=>
			{
				Console.WriteLine(x.response.DataString(Encoding.UTF8));
			});
		}
	}
}
