using System.Windows.Forms;

namespace MultipleCameraControlSampleTool
{
	partial class MultipleCameraControlSampleTool
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify.
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AppendButtun = new System.Windows.Forms.Button();
			this.DetectButtun = new System.Windows.Forms.Button();
			this.DetectorAppend = new System.Windows.Forms.GroupBox();
			this.DetectList = new System.Windows.Forms.ListView();
			this.index1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.Valid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.strProduct = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SerialNoIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label4 = new System.Windows.Forms.Label();
			this.CameraCount = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.Interface = new System.Windows.Forms.GroupBox();
			this.IPTetherCheckBox = new System.Windows.Forms.CheckBox();
			this.USBTetherCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.OpendList = new System.Windows.Forms.ListView();
			this.index2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.strVendor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.strProduct2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.strFirmware = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.Sensitivity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label6 = new System.Windows.Forms.Label();
			this.GetSensitivityButtun = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.CameraIndex = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.OpenButtun = new System.Windows.Forms.Button();
			this.CloseButtun = new System.Windows.Forms.Button();
			this.DetectorAppend.SuspendLayout();
			this.Interface.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CameraIndex)).BeginInit();
			this.SuspendLayout();
			// 
			// AppendButtun
			// 
			this.AppendButtun.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.AppendButtun.Enabled = false;
			this.AppendButtun.Location = new System.Drawing.Point(470, 18);
			this.AppendButtun.Name = "AppendButtun";
			this.AppendButtun.Size = new System.Drawing.Size(75, 23);
			this.AppendButtun.TabIndex = 1;
			this.AppendButtun.Text = "Append";
			this.AppendButtun.UseVisualStyleBackColor = false;
			this.AppendButtun.Click += new System.EventHandler(this.AppendButtun_Click);
			// 
			// DetectButtun
			// 
			this.DetectButtun.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.DetectButtun.Location = new System.Drawing.Point(389, 18);
			this.DetectButtun.Name = "DetectButtun";
			this.DetectButtun.Size = new System.Drawing.Size(75, 23);
			this.DetectButtun.TabIndex = 0;
			this.DetectButtun.Text = "Detect";
			this.DetectButtun.UseVisualStyleBackColor = false;
			this.DetectButtun.Click += new System.EventHandler(this.DetectButtun_Click);
			// 
			// DetectorAppend
			// 
			this.DetectorAppend.Controls.Add(this.DetectList);
			this.DetectorAppend.Controls.Add(this.label4);
			this.DetectorAppend.Controls.Add(this.CameraCount);
			this.DetectorAppend.Controls.Add(this.label1);
			this.DetectorAppend.Controls.Add(this.Interface);
			this.DetectorAppend.Controls.Add(this.DetectButtun);
			this.DetectorAppend.Controls.Add(this.AppendButtun);
			this.DetectorAppend.Location = new System.Drawing.Point(12, 12);
			this.DetectorAppend.Name = "DetectorAppend";
			this.DetectorAppend.Size = new System.Drawing.Size(560, 275);
			this.DetectorAppend.TabIndex = 2;
			this.DetectorAppend.TabStop = false;
			this.DetectorAppend.Text = "Detect/Append";
			// 
			// DetectList
			// 
			this.DetectList.BackColor = System.Drawing.SystemColors.ScrollBar;
			this.DetectList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.index1,
            this.Valid,
            this.strProduct,
            this.SerialNoIP});
			this.DetectList.FullRowSelect = true;
			this.DetectList.GridLines = true;
			this.DetectList.HideSelection = false;
			this.DetectList.Location = new System.Drawing.Point(6, 100);
			this.DetectList.Name = "DetectList";
			this.DetectList.Size = new System.Drawing.Size(548, 175);
			this.DetectList.TabIndex = 8;
			this.DetectList.UseCompatibleStateImageBehavior = false;
			this.DetectList.View = System.Windows.Forms.View.Details;
			// 
			// index1
			// 
			this.index1.Text = "index";
			this.index1.Width = 99;
			// 
			// Valid
			// 
			this.Valid.Text = "bValid";
			this.Valid.Width = 105;
			// 
			// strProduct
			// 
			this.strProduct.Text = "strProduct";
			this.strProduct.Width = 109;
			// 
			// SerialNoIP
			// 
			this.SerialNoIP.Text = "strSerialNo/strIPAddress";
			this.SerialNoIP.Width = 231;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(10, 77);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 12);
			this.label4.TabIndex = 5;
			this.label4.Text = "Detected cameras:";
			// 
			// CameraCount
			// 
			this.CameraCount.BackColor = System.Drawing.SystemColors.ScrollBar;
			this.CameraCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CameraCount.Location = new System.Drawing.Point(512, 49);
			this.CameraCount.Name = "CameraCount";
			this.CameraCount.Size = new System.Drawing.Size(33, 20);
			this.CameraCount.TabIndex = 4;
			this.CameraCount.Text = "0";
			this.CameraCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(422, 53);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 12);
			this.label1.TabIndex = 3;
			this.label1.Text = "# of cameras:";
			// 
			// Interface
			// 
			this.Interface.Controls.Add(this.IPTetherCheckBox);
			this.Interface.Controls.Add(this.USBTetherCheckBox);
			this.Interface.Location = new System.Drawing.Point(6, 18);
			this.Interface.Name = "Interface";
			this.Interface.Size = new System.Drawing.Size(203, 47);
			this.Interface.TabIndex = 2;
			this.Interface.TabStop = false;
			this.Interface.Text = "Interface";
			// 
			// IPTetherCheckBox
			// 
			this.IPTetherCheckBox.AutoSize = true;
			this.IPTetherCheckBox.Location = new System.Drawing.Point(68, 18);
			this.IPTetherCheckBox.Name = "IPTetherCheckBox";
			this.IPTetherCheckBox.Size = new System.Drawing.Size(113, 16);
			this.IPTetherCheckBox.TabIndex = 1;
			this.IPTetherCheckBox.Text = "IP(Local network)";
			this.IPTetherCheckBox.UseVisualStyleBackColor = true;
			// 
			// USBTetherCheckBox
			// 
			this.USBTetherCheckBox.AutoSize = true;
			this.USBTetherCheckBox.Checked = true;
			this.USBTetherCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.USBTetherCheckBox.Location = new System.Drawing.Point(15, 18);
			this.USBTetherCheckBox.Name = "USBTetherCheckBox";
			this.USBTetherCheckBox.Size = new System.Drawing.Size(47, 16);
			this.USBTetherCheckBox.TabIndex = 0;
			this.USBTetherCheckBox.Text = "USB";
			this.USBTetherCheckBox.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.OpendList);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.GetSensitivityButtun);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.CameraIndex);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.OpenButtun);
			this.groupBox2.Controls.Add(this.CloseButtun);
			this.groupBox2.Location = new System.Drawing.Point(12, 304);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(560, 317);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Individual camera control";
			// 
			// OpendList
			// 
			this.OpendList.BackColor = System.Drawing.SystemColors.ScrollBar;
			this.OpendList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.index2,
            this.strVendor,
            this.strProduct2,
            this.strFirmware,
            this.Sensitivity});
			this.OpendList.FullRowSelect = true;
			this.OpendList.GridLines = true;
			this.OpendList.HideSelection = false;
			this.OpendList.Location = new System.Drawing.Point(6, 135);
			this.OpendList.Name = "OpendList";
			this.OpendList.Size = new System.Drawing.Size(548, 175);
			this.OpendList.TabIndex = 7;
			this.OpendList.UseCompatibleStateImageBehavior = false;
			this.OpendList.View = System.Windows.Forms.View.Details;
			// 
			// index2
			// 
			this.index2.Text = "index";
			this.index2.Width = 91;
			// 
			// strVendor
			// 
			this.strVendor.Text = "strVendor";
			this.strVendor.Width = 112;
			// 
			// strProduct2
			// 
			this.strProduct2.Text = "strProduct";
			this.strProduct2.Width = 113;
			// 
			// strFirmware
			// 
			this.strFirmware.Text = "strFirmware";
			this.strFirmware.Width = 107;
			// 
			// Sensitivity
			// 
			this.Sensitivity.Text = "Sensitivity";
			this.Sensitivity.Width = 121;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 120);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(88, 12);
			this.label6.TabIndex = 6;
			this.label6.Text = "Opened devices:";
			// 
			// GetSensitivityButtun
			// 
			this.GetSensitivityButtun.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.GetSensitivityButtun.Enabled = false;
			this.GetSensitivityButtun.Location = new System.Drawing.Point(8, 80);
			this.GetSensitivityButtun.Name = "GetSensitivityButtun";
			this.GetSensitivityButtun.Size = new System.Drawing.Size(546, 23);
			this.GetSensitivityButtun.TabIndex = 5;
			this.GetSensitivityButtun.Text = "Getsensitivity";
			this.GetSensitivityButtun.UseVisualStyleBackColor = false;
			this.GetSensitivityButtun.Click += new System.EventHandler(this.GetSensitivityButtun_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 65);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(107, 12);
			this.label5.TabIndex = 4;
			this.label5.Text = "Camera control API:";
			// 
			// CameraIndex
			// 
			this.CameraIndex.Location = new System.Drawing.Point(59, 29);
			this.CameraIndex.Name = "CameraIndex";
			this.CameraIndex.Size = new System.Drawing.Size(39, 19);
			this.CameraIndex.TabIndex = 3;
			this.CameraIndex.ValueChanged += new System.EventHandler(this.CameraIndex_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(19, 31);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(34, 12);
			this.label3.TabIndex = 2;
			this.label3.Text = "index:";
			// 
			// OpenButtun
			// 
			this.OpenButtun.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.OpenButtun.Enabled = false;
			this.OpenButtun.Location = new System.Drawing.Point(112, 26);
			this.OpenButtun.Name = "OpenButtun";
			this.OpenButtun.Size = new System.Drawing.Size(75, 23);
			this.OpenButtun.TabIndex = 0;
			this.OpenButtun.Text = "Open";
			this.OpenButtun.UseVisualStyleBackColor = false;
			this.OpenButtun.Click += new System.EventHandler(this.OpenButtun_Click);
			// 
			// CloseButtun
			// 
			this.CloseButtun.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CloseButtun.Enabled = false;
			this.CloseButtun.Location = new System.Drawing.Point(193, 26);
			this.CloseButtun.Name = "CloseButtun";
			this.CloseButtun.Size = new System.Drawing.Size(75, 23);
			this.CloseButtun.TabIndex = 1;
			this.CloseButtun.Text = "Close";
			this.CloseButtun.UseVisualStyleBackColor = false;
			this.CloseButtun.Click += new System.EventHandler(this.CloseButtun_Click);
			// 
			// MultipleCameraControlSampleTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(579, 631);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.DetectorAppend);
			this.Name = "MultipleCameraControlSampleTool";
			this.Text = "MultipleCameraControlSampleTool";
			this.Load += new System.EventHandler(this.MultipleCameraControlSampleTool_Load);
			this.DetectorAppend.ResumeLayout(false);
			this.DetectorAppend.PerformLayout();
			this.Interface.ResumeLayout(false);
			this.Interface.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CameraIndex)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button AppendButtun;
		private System.Windows.Forms.Button DetectButtun;
		private System.Windows.Forms.GroupBox DetectorAppend;
		private System.Windows.Forms.GroupBox Interface;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button OpenButtun;
		private System.Windows.Forms.Button CloseButtun;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label CameraCount;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown CameraIndex;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button GetSensitivityButtun;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ListView OpendList;
		private ColumnHeader index2;
		private ColumnHeader strVendor;
		private ColumnHeader strProduct2;
		private ColumnHeader strFirmware;
		private ListView DetectList;
		private ColumnHeader index1;
		private ColumnHeader Valid;
		private ColumnHeader strProduct;
		private ColumnHeader SerialNoIP;
		private CheckBox IPTetherCheckBox;
		private CheckBox USBTetherCheckBox;
		private ColumnHeader Sensitivity;
	}
}

