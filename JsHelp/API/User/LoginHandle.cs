using DotNet4.Utilities.UtilCode;
using DotNet4.Utilities.UtilVerify;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNet4.Utilities.UtilHttp;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using DotNet4.Utilities.UtilHttp.HttpApiEvent;
using DotNet4.Utilities.UtilReg;

namespace JsHelp.API.User
{
	public class LoginHandle
	{
		[Serializable]
		public class WebCASNotSupportException : Exception
		{
			public WebCASNotSupportException() { }
			public WebCASNotSupportException(string message) : base(message) { }
			public WebCASNotSupportException(string message, Exception inner) : base(message, inner) { }
			protected WebCASNotSupportException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
		[Serializable]
		public class UserPasswordFailedException : Exception
		{
			public UserPasswordFailedException() { }
			public UserPasswordFailedException(string message) : base(message) { }
			public UserPasswordFailedException(string message, Exception inner) : base(message, inner) { }
			protected UserPasswordFailedException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
		[Serializable]
		public class VerifyFailedException : Exception
		{
			public VerifyFailedException() { }
			public VerifyFailedException(string message) : base(message) { }
			public VerifyFailedException(string message, Exception inner) : base(message, inner) { }
			protected VerifyFailedException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
	}
}
