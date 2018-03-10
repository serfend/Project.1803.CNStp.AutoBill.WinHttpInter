using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet4.Utilities.UtilHttp
{
	public class HttpClientSocket
	{
		public void Open(string method, string url, bool syn)
		{

		}
		public void SetProxy(string host)
		{

		}
		public void SetRequestHeader(string name,string value)
		{

		}
		public void Send(string payload)
		{

		}
		public void Send()
		{

		}

		public byte[] ResponseBody
		{
			get => new byte[5];
		}
		public Action<int, string> OnError;
		public Action OnResponseFinished;
		
	}
}
