//using DotNet4.Utilities.UtilReg;
//using DotNet4.Utilities.UtilVerify;
//using JsHelp.API;
//using JsHelp.API.Password;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Windows.Forms;

//namespace JsHelp
//{
//	public partial class FrmMain : Form
//	{
//		public FrmMain()
//		{
//			InitializeComponent();
//			//重要！根据线程实际使用情况调整。
//			//修改HTTP请求默认连接数，默认是2。
//			System.Net.ServicePointManager.DefaultConnectionLimit = 128;
//			//遇到417错误请使用以下代码。
//			//System.Net.ServicePointManager.Expect100Continue = false;
//		}

//		private void Form1_Load(object sender, EventArgs e)
//		{
//			//Console.WriteLine( new PasswordEncoder().GetPasswordRAS("shileizuim"));
//			var rkReg = new Reg().In("Setting").In("rk");
//			ipVerifyUsername.Text = rkReg.GetInfo("username");
//			ipVerifyPassword.Text = rkReg.GetInfo("password");
//			ipVerifyUsername.TextChanged += (x,xx) =>
//			{
//				rkReg.SetInfo("username", ipVerifyUsername.Text);
//				SynVerifier();
//			};
//			ipVerifyPassword.TextChanged += (x, xx) =>
//			{
//				rkReg.SetInfo("password", ipVerifyPassword.Text);
//				SynVerifier();
//			};
//			btnTestLogin.Click += (x,xx)=>  TestModule.Login(); ;
//			btnTestModifyPhone.Click += (x, xx) => TestModule.ModifyPhone();
//			SynVerifier();
//		}

//		private void SynVerifier()
//		{
//			RuoKuaiHttp.Init(ipVerifyUsername.Text, ipVerifyPassword.Text);
//		}

//		#region 测试模块
//		private TestMethod TestModule=new TestMethod();
//		#endregion

//		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
//		{

//		}

//		private void btnTestLogin_Click(object sender, EventArgs e)
//		{

//		}
//	}
//}
