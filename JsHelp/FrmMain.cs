using DotNet4.Utilities.UtilReg;
using DotNet4.Utilities.UtilVerify;
using JsHelp.API;
using JsHelp.API.Password;
using JsHelp.API.User;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DotNet4.Utilities.UtilInput;
using System.Threading;

namespace JsHelp
{
	public partial class FrmMain : Form
	{
		public FrmMain()
		{
			InitializeComponent();
			//重要！根据线程实际使用情况调整。
			//修改HTTP请求默认连接数，默认是2。
			System.Net.ServicePointManager.DefaultConnectionLimit = 256;
			//遇到417错误请使用以下代码。
			//System.Net.ServicePointManager.Expect100Continue = false;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			var setting = new Reg().In("Setting");
			var rkSetting = setting.In("rk");
			ipVerifyUsername.Text = rkSetting.GetInfo("username");
			ipVerifyPassword.Text = rkSetting.GetInfo("password");
			ipVerifyUsername.TextChanged += (x, xx) =>
			{
				rkSetting.SetInfo("username", ipVerifyUsername.Text);
				SynVerifier();
			};
			ipVerifyPassword.TextChanged += (x, xx) =>
			{
				rkSetting.SetInfo("password", ipVerifyPassword.Text);
				SynVerifier();
			};
			var formSetting = setting.In("Main");
			BtnTestStampInfo.Enabled = Convert.ToBoolean(formSetting.GetInfo("BtnTestStampInfo.Enabled","false"));

			lstUser.Columns.Add(new ColumnHeader("userAccess") { Text="状态"});
			lstUser.HoverSelection = true;
			lstUser.FullRowSelect = true;

			for(int i = 0; i < UserList.Count; i++)
			{
				var user = UserList.GetUser(i);
				var item=lstUser.Items.Add(user.Username,user.Username,0);
				item.SubItems.Add(user.Password);
				item.SubItems.Add(user.Status.ToString());
			}
			lstUser.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			var updateLst = new Action<int>((num) =>
			 {
				 for (int i = 0; i < num; i++)
				 {
					 //Thread.Sleep(1500);
					 new TestMethod(UserList.GetUser(i)).Login((user) =>
					 {
						 lstUser.BeginInvoke(new Action(() => { lstUser.Items[user.Username].SubItems[2].Text = user.Status.ToString(); }));
					 });
				 }
			 });
			btnTestLogin.Click += (x, xx) => {
				var i = Convert.ToInt32(InputBox.ShowInputBox("用户序号", ""));
				new TestMethod(UserList.GetUser(i)).Login((user) =>
				{
					lstUser.BeginInvoke(new Action(() => { lstUser.Items[user.Username].SubItems[2].Text = user.Status.ToString(); }));
				});
			} ;
			btnTestLoginMuti.Click += (x, xx) => {
				var num = InputBox.ShowInputBox("用户数量", "", UserList.Count.ToString());
				updateLst.BeginInvoke(Convert.ToInt32(num), (obj) => { }, null);
			};
			btnTestModifyPhone.Click += (x, xx) => TestModule.ModifyPhone();
			BtnTestStampInfo.Click += (x, xx) => { if (((Button)x).Enabled) TestModule.SynStampInfo(); };
			btnTestSynBillInfo.Click += (x, xx) => TestModule.SynBillInfo();
			btnTestGetPhoneVerifyCode.Click += (x, xx) => TestModule.GetPhoneVerifyCode();
			btnTestGetImgVerifyCode.Click += (x, xx) => TestModule.GetImgVerifyCode();
			btnTestSubmitBill.Click += (x, xx) => TestModule.TestSubmitBill();
			Logger.OnLog += (logSender, logInfo) => {
				OpLog.Invoke(new Action( ()=> {
					OpLog.AppendText(logInfo.LogBase);
					OpLog.AppendText(".");
					OpLog.AppendText(logInfo.LogInfo);
					OpLog.AppendText("\n");
					OpLog.ScrollToCaret();
				}));
			};
			
			SynVerifier();
		}
		private void SynVerifier()
		{
			RuoKuaiHttp.Init(ipVerifyUsername.Text, ipVerifyPassword.Text);
		}

		#region 测试模块
		private TestMethod TestModule = new TestMethod();
		#endregion

		private void btnTestLogin_Click(object sender, EventArgs e)
		{

		}
	}
}
