using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FujiSDK;
using CCameraControl;

namespace MultipleCameraControlSampleTool
{
	public partial class MultipleCameraControlSampleTool : Form
	{
		private long result;
		private CameraControl cameraControl;
		private long[] hCameras = new long[16];
		private IntPtr phCamera;

		public MultipleCameraControlSampleTool()
		{
			InitializeComponent();

			this.FormClosing += MultipleCameraControlSampleTool_FormClosing;
			Form parentForm = this;
			parentForm.StartPosition = FormStartPosition.CenterScreen;

			return;
		}

		private void MultipleCameraControlSampleTool_Load(object sender, EventArgs e)
		{
			result = CFujiSDK.XSDK_ERROR;

			cameraControl = new CameraControl();
			bool bLoad = cameraControl.LoadLibrary(); // Load XAPI.DLL.

			if (!bLoad)
			{
				MessageBox.Show("Failed to load XAPI.dll");
				Environment.Exit(0);
			}

			OpendList.OwnerDraw = true;
			DetectList.OwnerDraw = true;

			OpendList.DrawColumnHeader += List_DrawColumnHeader;
			OpendList.DrawSubItem += List_DrawSubItem;

			DetectList.DrawColumnHeader += List_DrawColumnHeader;
			DetectList.DrawSubItem += List_DrawSubItem;

			result = cameraControl.Init(); // Initialize SDK.

			if (result != CFujiSDK.XSDK_COMPLETE)
			{
				// err
				ErrDialog errDialog = new ErrDialog(cameraControl.sAPICode, cameraControl.sERRCode);
				errDialog.ShowDialog();

				Environment.Exit(0);
			}

			return;
		}

		private void DetectButtun_Click(object sender, EventArgs e)
		{
			result = CFujiSDK.XSDK_ERROR;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				int iInterface = 0;
				int iNumCameras = 0;
				string sInterface = "";
				string sDeviceName = "";

				if (USBTetherCheckBox.Checked)
				{
					iInterface |= CameraControl.XSDK_DSC_IF_USB;
				}
				if (IPTetherCheckBox.Checked)
				{
					iInterface |= CameraControl.XSDK_DSC_IF_WIFI_LOCAL;
				}

				DetectList.Items.Clear();

				result = cameraControl.Detect(iInterface, sInterface, sDeviceName, ref iNumCameras); // camera search.

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					ErrDialog errDialog = new ErrDialog(cameraControl.sAPICode, cameraControl.sERRCode);
					errDialog.ShowDialog();
					Cursor.Current = Cursors.Default;
					return;
				}

				if (iNumCameras > 0) // One or more cameras detected.
				{
					AppendButtun.Enabled = true;
					OpenButtun.Enabled = true;
					List<ListViewItem> items = new List<ListViewItem>();
					// Update List.
					for (int i = 0; i < iNumCameras; i++)
					{
						ListViewItem item = new ListViewItem(i.ToString());
						item.SubItems.Add("Valid");
						items.Add(item);
					}
					items.Sort((x, y) => int.Parse(x.Text).CompareTo(int.Parse(y.Text)));
					DetectList.Items.Clear();
					DetectList.Items.AddRange(items.ToArray());

					CameraIndex.Minimum = 0;
					CameraIndex.Maximum = iNumCameras - 1;
				}
				else
				{
					AppendButtun.Enabled = false;
					OpenButtun.Enabled = false;
					CameraIndex.Minimum = 0;
					CameraIndex.Maximum = 0;
				}

				CameraCount.Text = iNumCameras.ToString();
			}
			catch
			{
				// err
				MessageBox.Show("DetectButtun_Click err");
			}

			Cursor.Current = Cursors.Default;

			return;
		}

		private void AppendButtun_Click(object sender, EventArgs e)
		{
			result = CFujiSDK.XSDK_ERROR;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				int iInterface = 0;
				int iNumCameras = 0;
				string sInterface = "";
				List<XSDK_CameraList> cameraList = new List<XSDK_CameraList>(256);
				if (USBTetherCheckBox.Checked)
				{
					iInterface |= CameraControl.XSDK_DSC_IF_USB;
				}
				if (IPTetherCheckBox.Checked)
				{
					iInterface |= CameraControl.XSDK_DSC_IF_WIFI_LOCAL;
				}

				DetectList.Items.Clear();

				result = cameraControl.Append(iInterface, sInterface, null, ref iNumCameras, cameraList); // Camera list acquisition.

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					ErrDialog errDialog = new ErrDialog(cameraControl.sAPICode, cameraControl.sERRCode);
					errDialog.ShowDialog();
					Cursor.Current = Cursors.Default;
					return;
				}

				CameraCount.Text = iNumCameras.ToString();

				if (iNumCameras >= 0)
				{
					List<ListViewItem> items = new List<ListViewItem>();
					for (int i = 0; i < cameraList.Count; i++) // cameraList is a list of status and information of connected cameras.
					{
						// Update List.
						ListViewItem item = new ListViewItem(i.ToString());
						if (cameraList[i].bValid)
						{
							item.SubItems.Add("Valid");
						}
						else
						{
							item.SubItems.Add("Invalid");
						}

						item.SubItems.Add(cameraList[i].strProduct);

						if (cameraList[i].strFramework == "USB")
						{
							item.SubItems.Add(cameraList[i].strSerialNo);
						}
						else // Wi-Fi or Ethernet
						{
							item.SubItems.Add(cameraList[i].strIPAddress);
						}

						items.Add(item);
					}
					items.Sort((x, y) => int.Parse(x.Text).CompareTo(int.Parse(y.Text)));
					DetectList.Items.Clear();
					DetectList.Items.AddRange(items.ToArray());

					CameraIndex.Minimum = 0;
					CameraIndex.Maximum = iNumCameras - 1;
                    ListViewItem validItem = DetectList.Items[(int)CameraIndex.Value];
					string Valid = validItem.SubItems[1].Text;
					OpenButtun.Enabled = true;
					if (Valid == "Invalid")
					{
						OpenButtun.Enabled = false;
					}

					if(OpendList.Items.Count >= 1)
					{
						for(int i = 0; i < OpendList.Items.Count; i++)
						{
							ListViewItem openItem = OpendList.Items[i];
							string deviceIndex = openItem.SubItems[0].Text;
							if (deviceIndex == CameraIndex.Value.ToString())
							{
								OpenButtun.Enabled = false;
								break;
							}
						}
					}

				}
			}
			catch
			{
				// err
				MessageBox.Show("AppendButtun_Click err");
			}
			Cursor.Current = Cursors.Default;
			return;
		}

		private void OpenButtun_Click(object sender, EventArgs e)
		{
			try
			{
				result = CFujiSDK.XSDK_ERROR;
				string sDevice = "ENUM:";
				sDevice += CameraIndex.Value.ToString();
				phCamera = IntPtr.Zero;
				int iCameraMode = 0;

				result = cameraControl.Open(sDevice, ref phCamera, ref iCameraMode, (IntPtr)null); // Establish a session between the camera.

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					ErrDialog errDialog = new ErrDialog(cameraControl.sAPICode, cameraControl.sERRCode);
					errDialog.ShowDialog();
					return;
				}

				hCameras[(int)CameraIndex.Value] = phCamera.ToInt64();

				XSDK_DeviceInformation pDevInfo = new XSDK_DeviceInformation();
				result = cameraControl.GetDeviceInfo(phCamera, ref pDevInfo); // Get information about the connect camera.

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					result = cameraControl.Close(phCamera); // Disestablish a session between the camera.
					ErrDialog errDialog = new ErrDialog(cameraControl.sAPICode, cameraControl.sERRCode);
					errDialog.ShowDialog();

					return;
				}

				// Update List
				ListViewItem item = new ListViewItem(CameraIndex.Value.ToString());
				item.SubItems.Add(new string(pDevInfo.strVendor));
				item.SubItems.Add(new string(pDevInfo.strProduct));
				item.SubItems.Add(new string(pDevInfo.strFirmware));

				OpendList.Items.Add(item);
				List<ListViewItem> items = OpendList.Items.Cast<ListViewItem>().ToList();
				items.Sort((x, y) => int.Parse(x.Text).CompareTo(int.Parse(y.Text)));
				OpendList.Items.Clear();
				OpendList.Items.AddRange(items.ToArray());

				DetectButtun.Enabled = false;
				OpenButtun.Enabled = false;
				CloseButtun.Enabled = true;
				GetSensitivityButtun.Enabled = true;
			}
			catch
			{
				// err
				MessageBox.Show("OpenButtun_Click err");
			}

			return;
		}

		private void CloseButtun_Click(object sender, EventArgs e)
		{
			try
			{
				result = CFujiSDK.XSDK_ERROR;

				phCamera = new IntPtr(hCameras[(int)CameraIndex.Value]);

				result = cameraControl.Close(phCamera); // Disestablish a session between the camera.

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					ErrDialog errDialog = new ErrDialog(cameraControl.sAPICode, cameraControl.sERRCode);
					errDialog.ShowDialog();
					// Update List even if close failure.
				}

				for (int i = 0; i < OpendList.Items.Count; i++)
				{
					ListViewItem item = OpendList.Items[i];
					if (item.SubItems[0].Text == CameraIndex.Value.ToString())
					{
						// Update List
						OpendList.Items.RemoveAt(i);
					}
				}
				ListViewItem detectItem = DetectList.Items[(int)CameraIndex.Value];
				string Valid = detectItem.SubItems[1].Text;
				if (Valid == "Invalid")
				{
					OpenButtun.Enabled = false;
				}
				else
				{
					OpenButtun.Enabled = true;
				}
				CloseButtun.Enabled = false;
				GetSensitivityButtun.Enabled = false;

				if (OpendList.Items.Count == 0)
				{
					DetectButtun.Enabled = true;
				}

				phCamera = IntPtr.Zero;
				hCameras[(int)CameraIndex.Value] = 0;
			}
			catch
			{
				// err
				MessageBox.Show("CloseButtun_Click err");
			}

			return;
		}

		private void GetSensitivityButtun_Click(object sender, EventArgs e)
		{
			try
			{
				result = CFujiSDK.XSDK_ERROR;
				phCamera = new IntPtr(hCameras[(int)CameraIndex.Value]);
				int iSensitivity = 0;
				Sensitivity sensitivity;

				result = cameraControl.GetSensitivity(phCamera, ref iSensitivity); // Get the ISO sensitivity.

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					ErrDialog errDialog = new ErrDialog(cameraControl.sAPICode, cameraControl.sERRCode);
					errDialog.ShowDialog();

					return;
				}

				foreach (ListViewItem item in OpendList.Items)
				{
					if (item.SubItems[0].Text == CameraIndex.Value.ToString())
					{
						if (Enum.IsDefined(typeof(Sensitivity), iSensitivity))
						{
							sensitivity = (Sensitivity)iSensitivity;
							// Update List
							while (item.SubItems.Count > 4)
							{
								item.SubItems.RemoveAt(4);
							}
							item.SubItems.Add(sensitivity.ToString());
						}
						else
						{
							string hexValue = "0x" + iSensitivity.ToString("X8") ;
							// Update List
							while (item.SubItems.Count > 4)
							{
								item.SubItems.RemoveAt(4);
							}
							item.SubItems.Add(hexValue.ToString());
						}
					}
				}
			}
			catch
			{
				// err
				MessageBox.Show("GetSensitivityButtun_Click err");
			}

			return ;
		}

		private void MultipleCameraControlSampleTool_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				result = CFujiSDK.XSDK_ERROR;

				if (OpendList.Items.Count != 0)
				{
					// Disestablish session with all connected cameras.
					for (int i = 0; OpendList.Items.Count > i; i++)
					{
						ListViewItem item = OpendList.Items[i];
						phCamera = new IntPtr(hCameras[int.Parse(item.SubItems[0].Text)]);
						cameraControl.Close(phCamera);
					}
				}

				result = cameraControl.Exit(); // Terminate SDK.

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					ErrDialog errDialog = new ErrDialog(cameraControl.sAPICode, cameraControl.sERRCode);
					errDialog.ShowDialog();
				}
			}
			catch
			{
				// err
				MessageBox.Show("MultipleCameraControlSampleTool_FormClosing err");
			}
			return;
		}

		// Button control
		private void CameraIndex_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				int cameraIndex = (int)CameraIndex.Value;
                OpenButtun.Enabled = true;

                for (int i = 0; OpendList.Items.Count > i; i++)
				{
					if (OpendList.Items.Count > 0 && cameraIndex >= 0)
					{
						ListViewItem openItem = OpendList.Items[i];
						string deviceIndex = openItem.SubItems[0].Text;
						if (CameraIndex.Value.ToString() != deviceIndex)
						{
							OpenButtun.Enabled = true;
							CloseButtun.Enabled = false;
							GetSensitivityButtun.Enabled = false;
                        }
						else
						{
							OpenButtun.Enabled = false;
							CloseButtun.Enabled = true;
							GetSensitivityButtun.Enabled = true;
							break;
						}
					}
				}
                ListViewItem detectItem = DetectList.Items[cameraIndex];
                string Valid = detectItem.SubItems[1].Text;
                if (Valid == "Invalid")
                {
                    OpenButtun.Enabled = false;
                }
            }
			catch
			{
				// err
				MessageBox.Show("CameraIndex_ValueChanged err");
			}

			return;
		}

		// Centering of header.
		private void List_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = false;
			e.DrawBackground();
			TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, e.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}

		// Centering of subItem.
		private void List_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			SizeF textSize = e.Graphics.MeasureString(e.SubItem.Text, e.SubItem.Font);
			float x = e.Bounds.Left + (e.Bounds.Width - textSize.Width) / 2;
			float y = e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2;
			e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font, Brushes.Black, x, y);
		}
	}
}
