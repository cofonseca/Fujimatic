namespace MultipleCameraControlSampleTool
{
	partial class ErrDialog
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OKButtun = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.APICodeTextBox = new System.Windows.Forms.TextBox();
			this.ERRCodeTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// OKButtun
			// 
			this.OKButtun.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.OKButtun.Location = new System.Drawing.Point(113, 140);
			this.OKButtun.Name = "OKButtun";
			this.OKButtun.Size = new System.Drawing.Size(75, 23);
			this.OKButtun.TabIndex = 0;
			this.OKButtun.Text = "OK";
			this.OKButtun.UseVisualStyleBackColor = true;
			this.OKButtun.Click += new System.EventHandler(this.OKButtun_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(48, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(49, 12);
			this.label1.TabIndex = 1;
			this.label1.Text = "APICode";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 12);
			this.label2.TabIndex = 2;
			this.label2.Text = "GetErrNumber()";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(43, 94);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(54, 12);
			this.label3.TabIndex = 3;
			this.label3.Text = "ERRCode";
			// 
			// APICodeTextBox
			// 
			this.APICodeTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.APICodeTextBox.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
			this.APICodeTextBox.Location = new System.Drawing.Point(113, 45);
			this.APICodeTextBox.Name = "APICodeTextBox";
			this.APICodeTextBox.ReadOnly = true;
			this.APICodeTextBox.Size = new System.Drawing.Size(237, 19);
			this.APICodeTextBox.TabIndex = 4;
			this.APICodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// ERRCodeTextBox
			// 
			this.ERRCodeTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.ERRCodeTextBox.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
			this.ERRCodeTextBox.Location = new System.Drawing.Point(113, 87);
			this.ERRCodeTextBox.Name = "ERRCodeTextBox";
			this.ERRCodeTextBox.ReadOnly = true;
			this.ERRCodeTextBox.Size = new System.Drawing.Size(237, 19);
			this.ERRCodeTextBox.TabIndex = 5;
			this.ERRCodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// ErrDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(362, 175);
			this.Controls.Add(this.ERRCodeTextBox);
			this.Controls.Add(this.APICodeTextBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.OKButtun);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ErrDialog";
			this.Text = "XSDK";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button OKButtun;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox APICodeTextBox;
		private System.Windows.Forms.TextBox ERRCodeTextBox;
	}
}