namespace JsHelp
{
	partial class FrmMain
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.ipUserPassword = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ipUserName = new System.Windows.Forms.TextBox();
			this.btnAddUser = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lstUser = new System.Windows.Forms.ListView();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btnTestSubmitBill = new System.Windows.Forms.Button();
			this.btnTestSynBillInfo = new System.Windows.Forms.Button();
			this.btnTestGetImgVerifyCode = new System.Windows.Forms.Button();
			this.btnTestGetPhoneVerifyCode = new System.Windows.Forms.Button();
			this.btnTestModifyPhone = new System.Windows.Forms.Button();
			this.btnTestLogin = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.ipVerifyPassword = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.ipVerifyUsername = new System.Windows.Forms.TextBox();
			this.BtnTestStampInfo = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.ipUserPassword);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.ipUserName);
			this.groupBox1.Controls.Add(this.btnAddUser);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(294, 122);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "用户";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(29, 12);
			this.label2.TabIndex = 4;
			this.label2.Text = "密码";
			// 
			// ipUserPassword
			// 
			this.ipUserPassword.Location = new System.Drawing.Point(53, 51);
			this.ipUserPassword.Name = "ipUserPassword";
			this.ipUserPassword.Size = new System.Drawing.Size(227, 21);
			this.ipUserPassword.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 29);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(41, 12);
			this.label1.TabIndex = 2;
			this.label1.Text = "用户名";
			// 
			// ipUserName
			// 
			this.ipUserName.Location = new System.Drawing.Point(53, 24);
			this.ipUserName.Name = "ipUserName";
			this.ipUserName.Size = new System.Drawing.Size(227, 21);
			this.ipUserName.TabIndex = 1;
			// 
			// btnAddUser
			// 
			this.btnAddUser.Location = new System.Drawing.Point(203, 78);
			this.btnAddUser.Name = "btnAddUser";
			this.btnAddUser.Size = new System.Drawing.Size(77, 28);
			this.btnAddUser.TabIndex = 0;
			this.btnAddUser.Text = "添加";
			this.btnAddUser.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lstUser);
			this.groupBox2.Location = new System.Drawing.Point(312, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(673, 892);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "用户列表";
			// 
			// lstUser
			// 
			this.lstUser.Location = new System.Drawing.Point(6, 20);
			this.lstUser.Name = "lstUser";
			this.lstUser.Size = new System.Drawing.Size(661, 866);
			this.lstUser.TabIndex = 0;
			this.lstUser.UseCompatibleStateImageBehavior = false;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.BtnTestStampInfo);
			this.groupBox3.Controls.Add(this.btnTestSubmitBill);
			this.groupBox3.Controls.Add(this.btnTestSynBillInfo);
			this.groupBox3.Controls.Add(this.btnTestGetImgVerifyCode);
			this.groupBox3.Controls.Add(this.btnTestGetPhoneVerifyCode);
			this.groupBox3.Controls.Add(this.btnTestModifyPhone);
			this.groupBox3.Controls.Add(this.btnTestLogin);
			this.groupBox3.Location = new System.Drawing.Point(12, 218);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(292, 296);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "测试";
			// 
			// btnTestSubmitBill
			// 
			this.btnTestSubmitBill.Location = new System.Drawing.Point(8, 242);
			this.btnTestSubmitBill.Name = "btnTestSubmitBill";
			this.btnTestSubmitBill.Size = new System.Drawing.Size(278, 33);
			this.btnTestSubmitBill.TabIndex = 5;
			this.btnTestSubmitBill.Text = "提交订单";
			this.btnTestSubmitBill.UseVisualStyleBackColor = true;
			// 
			// btnTestSynBillInfo
			// 
			this.btnTestSynBillInfo.Location = new System.Drawing.Point(8, 131);
			this.btnTestSynBillInfo.Name = "btnTestSynBillInfo";
			this.btnTestSynBillInfo.Size = new System.Drawing.Size(278, 33);
			this.btnTestSynBillInfo.TabIndex = 4;
			this.btnTestSynBillInfo.Text = "获取订单信息";
			this.btnTestSynBillInfo.UseVisualStyleBackColor = true;
			// 
			// btnTestGetImgVerifyCode
			// 
			this.btnTestGetImgVerifyCode.Location = new System.Drawing.Point(8, 205);
			this.btnTestGetImgVerifyCode.Name = "btnTestGetImgVerifyCode";
			this.btnTestGetImgVerifyCode.Size = new System.Drawing.Size(278, 33);
			this.btnTestGetImgVerifyCode.TabIndex = 3;
			this.btnTestGetImgVerifyCode.Text = "获取图像验证码";
			this.btnTestGetImgVerifyCode.UseVisualStyleBackColor = true;
			// 
			// btnTestGetPhoneVerifyCode
			// 
			this.btnTestGetPhoneVerifyCode.Location = new System.Drawing.Point(8, 168);
			this.btnTestGetPhoneVerifyCode.Name = "btnTestGetPhoneVerifyCode";
			this.btnTestGetPhoneVerifyCode.Size = new System.Drawing.Size(278, 33);
			this.btnTestGetPhoneVerifyCode.TabIndex = 2;
			this.btnTestGetPhoneVerifyCode.Text = "获取手机验证码";
			this.btnTestGetPhoneVerifyCode.UseVisualStyleBackColor = true;
			// 
			// btnTestModifyPhone
			// 
			this.btnTestModifyPhone.Location = new System.Drawing.Point(8, 57);
			this.btnTestModifyPhone.Name = "btnTestModifyPhone";
			this.btnTestModifyPhone.Size = new System.Drawing.Size(278, 33);
			this.btnTestModifyPhone.TabIndex = 1;
			this.btnTestModifyPhone.Text = "修改手机号";
			this.btnTestModifyPhone.UseVisualStyleBackColor = true;
			// 
			// btnTestLogin
			// 
			this.btnTestLogin.Location = new System.Drawing.Point(8, 20);
			this.btnTestLogin.Name = "btnTestLogin";
			this.btnTestLogin.Size = new System.Drawing.Size(278, 33);
			this.btnTestLogin.TabIndex = 0;
			this.btnTestLogin.Text = "登录";
			this.btnTestLogin.UseVisualStyleBackColor = true;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.label3);
			this.groupBox4.Controls.Add(this.ipVerifyPassword);
			this.groupBox4.Controls.Add(this.label4);
			this.groupBox4.Controls.Add(this.ipVerifyUsername);
			this.groupBox4.Location = new System.Drawing.Point(12, 140);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(294, 72);
			this.groupBox4.TabIndex = 3;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "若快打码平台";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 50);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(29, 12);
			this.label3.TabIndex = 4;
			this.label3.Text = "密码";
			// 
			// ipVerifyPassword
			// 
			this.ipVerifyPassword.Location = new System.Drawing.Point(53, 45);
			this.ipVerifyPassword.Name = "ipVerifyPassword";
			this.ipVerifyPassword.Size = new System.Drawing.Size(227, 21);
			this.ipVerifyPassword.TabIndex = 3;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 23);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(41, 12);
			this.label4.TabIndex = 2;
			this.label4.Text = "用户名";
			// 
			// ipVerifyUsername
			// 
			this.ipVerifyUsername.Location = new System.Drawing.Point(53, 18);
			this.ipVerifyUsername.Name = "ipVerifyUsername";
			this.ipVerifyUsername.Size = new System.Drawing.Size(227, 21);
			this.ipVerifyUsername.TabIndex = 1;
			// 
			// BtnTestStampInfo
			// 
			this.BtnTestStampInfo.Location = new System.Drawing.Point(8, 94);
			this.BtnTestStampInfo.Name = "BtnTestStampInfo";
			this.BtnTestStampInfo.Size = new System.Drawing.Size(278, 33);
			this.BtnTestStampInfo.TabIndex = 6;
			this.BtnTestStampInfo.Text = "获取邮票信息";
			this.BtnTestStampInfo.UseVisualStyleBackColor = true;
			// 
			// FrmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(997, 916);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "FrmMain";
			this.Text = "11185邮票采集";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox ipUserPassword;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox ipUserName;
		private System.Windows.Forms.Button btnAddUser;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ListView lstUser;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button btnTestSubmitBill;
		private System.Windows.Forms.Button btnTestSynBillInfo;
		private System.Windows.Forms.Button btnTestGetImgVerifyCode;
		private System.Windows.Forms.Button btnTestGetPhoneVerifyCode;
		private System.Windows.Forms.Button btnTestModifyPhone;
		private System.Windows.Forms.Button btnTestLogin;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox ipVerifyPassword;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox ipVerifyUsername;
		private System.Windows.Forms.Button BtnTestStampInfo;
	}
}

