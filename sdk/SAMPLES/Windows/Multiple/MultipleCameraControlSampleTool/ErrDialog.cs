using System;
using System.Windows.Forms;
using CCameraControl;

namespace MultipleCameraControlSampleTool
{
	public partial class ErrDialog : Form
	{
		public ErrDialog(string APICode,string ERRCode)
		{
			InitializeComponent();
			Form parentForm = this;
			parentForm.StartPosition = FormStartPosition.CenterScreen;
			APICodeTextBox.Text = APICode.ToString();
			ERRCodeTextBox.Text = ERRCode.ToString();
		}

		private void OKButtun_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
