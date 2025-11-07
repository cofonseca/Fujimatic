using System;
using System.Windows.Forms;
using CCameraControl;
using FujiSDK;

namespace ZoomPos
{
	public partial class ZoomPosForm : Form
	{
		private string state = "DisConnect";
		private System.Windows.Forms.Timer timer;
		private ZoomPosControl zoomPosControl;

		public ZoomPosForm()
		{
			InitializeComponent();

			return;
		}

		private void ZoomPosForm_Load(object sender, EventArgs e)
		{
			zoomPosControl = new ZoomPosControl();

			bool bLoad = zoomPosControl.LoadLibrary();

			if (!bLoad)
			{
				// err
				MessageBox.Show("Failed to load XAPI.dll");
				Environment.Exit(0);
				return;
			}

			zoomPosControl.SetDetectOption(CameraControl.XSDK_DSC_IF_USB, null);
			zoomPosControl.BeginConnectThread();

			SetTimer();

			return;
		}

		// Exit Application
		private void ButtonExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void SetTimer()
		{
			timer = new System.Windows.Forms.Timer();
			timer.Interval = 500;
			timer.Tick += ConnectCheck;
			timer.Start();
		}

		private void ConnectCheck(object sender, EventArgs e)
		{
			switch (state)
			{
				case "DisConnect":
					if (zoomPosControl != null)
					{
						// Connection Check
						if (zoomPosControl.IsConnectThread() == ZoomPosControl.FALSE)
						{
							zoomPosControl.EndConnectThread();
							zoomPosControl.BeginPollThread();

							state = "Connect";
							StateLabel.Text = " Connect";
						}
					}
					break;
				case "Connect":
					if (zoomPosControl != null)
					{
						// Connection Check
						if (zoomPosControl.IsPollThread() != ZoomPosControl.FALSE)
						{
							long lFocalLength;
							int busyCount;

							lFocalLength = zoomPosControl.GetFocalLength();
							if (lFocalLength > 0)
							{
								ZoomFocalLength.Text = (lFocalLength / 100).ToString() + "." + (lFocalLength % 100).ToString() + " mm";
							}
							else
							{
								ZoomFocalLength.Text = "- - mm";
							}

							lFocalLength = zoomPosControl.Get35mmFocalLength();
							if (lFocalLength > 0)
							{
								ZoomFocalLength35.Text = (lFocalLength / 100).ToString() + "." + (lFocalLength % 100).ToString() + " mm";
							}
							else
							{
								ZoomFocalLength35.Text = "- - mm";
							}

							busyCount = zoomPosControl.GetBusyCount();
							BusyCount.Text = busyCount.ToString();
						}
						else
						{
							zoomPosControl.EndPollThread();
							zoomPosControl.TerminateConnection();
							zoomPosControl.SetDetectOption(CameraControl.XSDK_DSC_IF_USB, null);
							zoomPosControl.BeginConnectThread();

							state = "DisConnect";
							StateLabel.Text = " Waiting for a camera to be connected.";
							ZoomFocalLength.Text = "- - mm";
							ZoomFocalLength35.Text = "- - mm";
							BusyCount.Text = "0";
						}
					}
					break;
			}
		}

		// Exit Application
		private void ZoomPosForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (timer != null)
			{
				timer.Stop();
				timer.Dispose();
				timer = null;
			}
			zoomPosControl.TerminateConnection();	
		}
	}
}
