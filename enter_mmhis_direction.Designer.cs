namespace mmhis_data_2_vs2012
{
	partial class enter_mmhis_direction
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
			this.listBox_direction = new System.Windows.Forms.ListBox();
			this.button_use_selected_direction = new System.Windows.Forms.Button();
			this.button_no_direction = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(48, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(198, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Select MMHIS Direction for This Section";
			// 
			// listBox_direction
			// 
			this.listBox_direction.FormattingEnabled = true;
			this.listBox_direction.Items.AddRange(new object[] {
            "East",
            "West",
            "North",
            "South"});
			this.listBox_direction.Location = new System.Drawing.Point(51, 58);
			this.listBox_direction.Name = "listBox_direction";
			this.listBox_direction.Size = new System.Drawing.Size(195, 121);
			this.listBox_direction.TabIndex = 1;
			// 
			// button_use_selected_direction
			// 
			this.button_use_selected_direction.Location = new System.Drawing.Point(51, 199);
			this.button_use_selected_direction.Name = "button_use_selected_direction";
			this.button_use_selected_direction.Size = new System.Drawing.Size(195, 23);
			this.button_use_selected_direction.TabIndex = 2;
			this.button_use_selected_direction.Text = "Use Selected Direction";
			this.button_use_selected_direction.UseVisualStyleBackColor = true;
			this.button_use_selected_direction.Click += new System.EventHandler(this.button_use_selected_direction_Click);
			// 
			// button_no_direction
			// 
			this.button_no_direction.Location = new System.Drawing.Point(51, 240);
			this.button_no_direction.Name = "button_no_direction";
			this.button_no_direction.Size = new System.Drawing.Size(195, 23);
			this.button_no_direction.TabIndex = 3;
			this.button_no_direction.Text = "Use No Direction";
			this.button_no_direction.UseVisualStyleBackColor = true;
			this.button_no_direction.Click += new System.EventHandler(this.button_no_direction_Click);
			// 
			// enter_mmhis_direction
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 284);
			this.Controls.Add(this.button_no_direction);
			this.Controls.Add(this.button_use_selected_direction);
			this.Controls.Add(this.listBox_direction);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "enter_mmhis_direction";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Enter Direction";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button_use_selected_direction;
		private System.Windows.Forms.Button button_no_direction;
		private System.Windows.Forms.ListBox listBox_direction;
	}
}