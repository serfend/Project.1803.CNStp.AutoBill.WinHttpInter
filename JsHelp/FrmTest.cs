using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsHelp
{
	public partial class FrmTest : Form
	{
		public FrmTest()
		{
			InitializeComponent();
		}

		private void FrmTest_Load(object sender, EventArgs e)
		{
			new JsHelp.Unti.HttpClientTest().NormalHttp();
		}
	}
}
