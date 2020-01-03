namespace mmhis_data_2_vs2012
{
	partial class enter_mmhis_year
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
			this.label1 = new System.Windows.Forms.Label();
			this.textBox_year = new System.Windows.Forms.TextBox();
			this.button_accept_year_number = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(206, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Please make changes to the year number.";
			// 
			// textBox_year
			// 
			this.textBox_year.Location = new System.Drawing.Point(16, 41);
			this.textBox_year.Name = "textBox_year";
			this.textBox_year.Size = new System.Drawing.Size(203, 20);
			this.textBox_year.TabIndex = 1;
			// 
			// button_accept_year_number
			// 
			this.button_accept_year_number.Location = new System.Drawing.Point(16, 88);
			this.button_accept_year_number.Name = "button_accept_year_number";
			this.button_accept_year_number.Size = new System.Drawing.Size(203, 23);
			this.button_accept_year_number.TabIndex = 2;
			this.button_accept_year_number.Text = "Use the above as Year";
			this.button_accept_year_number.UseVisualStyleBackColor = true;
			this.button_accept_year_number.Click += new System.EventHandler(this.button_accept_year_number_Click);
			// 
			// enter_mmhis_year
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(237, 131);
			this.Controls.Add(this.button_accept_year_number);
			this.Controls.Add(this.textBox_year);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "enter_mmhis_year";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Enter MMHIS Year";
			this.Load += new System.EventHandler(this.enter_mmhis_year_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_year;
		private System.Windows.Forms.Button button_accept_year_number;
	}
}