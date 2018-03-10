namespace JsHelp.API.User
{
	public class LoginForm
	{
		public string lt;
		public string execution;
		public string actionUrl;
		public string LoginId;
		public LoginForm(string lt, string execution, string actionUrl)
		{
			this.lt = lt;
			this.execution = execution;
			this.actionUrl = actionUrl;
		}
	}
}