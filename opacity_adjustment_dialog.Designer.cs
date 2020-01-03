namespace mmhis_data_2_vs2012
{
	partial class opacity_adjustment_dialog
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
			this.trackBar_opacity = new System.Windows.Forms.TrackBar();
			this.button_ok = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.trackBar_opacity)).BeginInit();
			this.SuspendLayout();
			// 
			// trackBar_opacity
			// 
			this.trackBar_opacity.AutoSize = false;
			this.trackBar_opacity.LargeChange = 10;
			this.trackBar_opacity.Location = new System.Drawing.Point(3, 1);
			this.trackBar_opacity.Margin = new System.Windows.Forms.Padding(0);
			this.trackBar_opacity.Maximum = 100;
			this.trackBar_opacity.Minimum = 10;
			this.trackBar_opacity.Name = "trackBar_opacity";
			this.trackBar_opacity.Size = new System.Drawing.Size(114, 20);
			this.trackBar_opacity.TabIndex = 0;
			this.trackBar_opacity.TickFrequency = 10;
			this.trackBar_opacity.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackBar_opacity.Value = 100;
			this.trackBar_opacity.ValueChanged += new System.EventHandler(this.trackBar_opacity_ValueChanged);
			// 
			// button_ok
			// 
			this.button_ok.Cursor = System.Windows.Forms.Cursors.Hand;
			this.button_ok.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_ok.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
			this.button_ok.FlatAppearance.BorderSize = 0;
			this.button_ok.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_ok.Location = new System.Drawing.Point(28, 24);
			this.button_ok.Name = "button_ok";
			this.button_ok.Size = new System.Drawing.Size(75, 23);
			this.button_ok.TabIndex = 1;
			this.button_ok.Text = "OK";
			this.button_ok.UseVisualStyleBackColor = true;
			this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
			// 
			// opacity_adjustment_dialog
			// 
			this.AcceptButton = this.button_ok;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button_ok;
			this.ClientSize = new System.Drawing.Size(123, 51);
			this.ControlBox = false;
			this.Controls.Add(this.button_ok);
			this.Controls.Add(this.trackBar_opacity);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "opacity_adjustment_dialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.trackBar_opacity)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button_ok;
		public System.Windows.Forms.TrackBar trackBar_opacity;
	}
}