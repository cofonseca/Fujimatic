namespace ZoomPos
{
    partial class ZoomPosForm
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
			this.components = new System.ComponentModel.Container();
			this.ButtonExit = new System.Windows.Forms.Button();
			this.LabelZFL = new System.Windows.Forms.Label();
			this.LabelZFL_35mm = new System.Windows.Forms.Label();
			this.LabelCBC = new System.Windows.Forms.Label();
			this.ZoomFocalLength = new System.Windows.Forms.Label();
			this.ZoomFocalLength35 = new System.Windows.Forms.Label();
			this.BusyCount = new System.Windows.Forms.Label();
			this.TimerPoll = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.StateLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ButtonExit
			// 
			this.ButtonExit.Font = new System.Drawing.Font("MS UI Gothic", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.ButtonExit.Location = new System.Drawing.Point(307, 312);
			this.ButtonExit.Name = "ButtonExit";
			this.ButtonExit.Size = new System.Drawing.Size(123, 29);
			this.ButtonExit.TabIndex = 0;
			this.ButtonExit.Text = "Exit";
			this.ButtonExit.UseVisualStyleBackColor = true;
			this.ButtonExit.Click += new System.EventHandler(this.ButtonExit_Click);
			// 
			// LabelZFL
			// 
			this.LabelZFL.AutoSize = true;
			this.LabelZFL.Font = new System.Drawing.Font("MS UI Gothic", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.LabelZFL.Location = new System.Drawing.Point(29, 79);
			this.LabelZFL.Name = "LabelZFL";
			this.LabelZFL.Size = new System.Drawing.Size(178, 22);
			this.LabelZFL.TabIndex = 1;
			this.LabelZFL.Text = "Zoom focal length:";
			// 
			// LabelZFL_35mm
			// 
			this.LabelZFL_35mm.AutoSize = true;
			this.LabelZFL_35mm.Font = new System.Drawing.Font("MS UI Gothic", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.LabelZFL_35mm.Location = new System.Drawing.Point(29, 149);
			this.LabelZFL_35mm.Name = "LabelZFL_35mm";
			this.LabelZFL_35mm.Size = new System.Drawing.Size(343, 22);
			this.LabelZFL_35mm.TabIndex = 2;
			this.LabelZFL_35mm.Text = "Zoom focal length (in 35mm format):";
			// 
			// LabelCBC
			// 
			this.LabelCBC.AutoSize = true;
			this.LabelCBC.Font = new System.Drawing.Font("MS UI Gothic", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.LabelCBC.Location = new System.Drawing.Point(29, 231);
			this.LabelCBC.Name = "LabelCBC";
			this.LabelCBC.Size = new System.Drawing.Size(275, 22);
			this.LabelCBC.TabIndex = 3;
			this.LabelCBC.Text = "Communication BUSY count:";
			// 
			// ZoomFocalLength
			// 
			this.ZoomFocalLength.Font = new System.Drawing.Font("MS UI Gothic", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.ZoomFocalLength.Location = new System.Drawing.Point(252, 96);
			this.ZoomFocalLength.Name = "ZoomFocalLength";
			this.ZoomFocalLength.Size = new System.Drawing.Size(192, 22);
			this.ZoomFocalLength.TabIndex = 4;
			this.ZoomFocalLength.Text = "-- mm";
			this.ZoomFocalLength.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// ZoomFocalLength35
			// 
			this.ZoomFocalLength35.Font = new System.Drawing.Font("MS UI Gothic", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.ZoomFocalLength35.Location = new System.Drawing.Point(229, 195);
			this.ZoomFocalLength35.Name = "ZoomFocalLength35";
			this.ZoomFocalLength35.Size = new System.Drawing.Size(201, 22);
			this.ZoomFocalLength35.TabIndex = 5;
			this.ZoomFocalLength35.Text = "-- mm";
			this.ZoomFocalLength35.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// BusyCount
			// 
			this.BusyCount.Font = new System.Drawing.Font("MS UI Gothic", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.BusyCount.Location = new System.Drawing.Point(252, 267);
			this.BusyCount.Name = "BusyCount";
			this.BusyCount.Size = new System.Drawing.Size(178, 22);
			this.BusyCount.TabIndex = 6;
			this.BusyCount.Text = "0";
			this.BusyCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.SystemColors.InfoText;
			this.label1.Location = new System.Drawing.Point(-4, 54);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(497, 12);
			this.label1.TabIndex = 10;
			this.label1.Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
			// 
			// StateLabel
			// 
			this.StateLabel.AutoSize = true;
			this.StateLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.StateLabel.Font = new System.Drawing.Font("MS UI Gothic", 16F);
			this.StateLabel.Location = new System.Drawing.Point(33, 20);
			this.StateLabel.Name = "StateLabel";
			this.StateLabel.Size = new System.Drawing.Size(369, 24);
			this.StateLabel.TabIndex = 11;
			this.StateLabel.Text = " Waiting for a camera to be connected.";
			// 
			// ZoomPosForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(484, 361);
			this.Controls.Add(this.StateLabel);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.BusyCount);
			this.Controls.Add(this.ZoomFocalLength35);
			this.Controls.Add(this.ZoomFocalLength);
			this.Controls.Add(this.LabelCBC);
			this.Controls.Add(this.LabelZFL_35mm);
			this.Controls.Add(this.LabelZFL);
			this.Controls.Add(this.ButtonExit);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ZoomPosForm";
			this.Text = "ZoomPos";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ZoomPosForm_FormClosing);
			this.Load += new System.EventHandler(this.ZoomPosForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

        #endregion

        private System.Windows.Forms.Button ButtonExit;
        private System.Windows.Forms.Label LabelZFL;
        private System.Windows.Forms.Label LabelZFL_35mm;
        private System.Windows.Forms.Label LabelCBC;
        private System.Windows.Forms.Label ZoomFocalLength;
        private System.Windows.Forms.Label ZoomFocalLength35;
        private System.Windows.Forms.Label BusyCount;
        private System.Windows.Forms.Timer TimerPoll;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label StateLabel;
	}
}