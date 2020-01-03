namespace mmhis_data_2_vs2012
{
	partial class choose_field_specifier
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
			this.listBox_field_specifier = new System.Windows.Forms.ListBox();
			this.button_ok = new System.Windows.Forms.Button();
			this.textBox_field_specifier = new System.Windows.Forms.TextBox();
			this.checkBox_append = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// listBox_field_specifier
			// 
			this.listBox_field_specifier.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listBox_field_specifier.FormattingEnabled = true;
			this.listBox_field_specifier.IntegralHeight = false;
			this.listBox_field_specifier.Location = new System.Drawing.Point(12, 12);
			this.listBox_field_specifier.Name = "listBox_field_specifier";
			this.listBox_field_specifier.Size = new System.Drawing.Size(239, 348);
			this.listBox_field_specifier.TabIndex = 0;
			this.listBox_field_specifier.SelectedIndexChanged += new System.EventHandler(this.listBox_field_specifier_SelectedIndexChanged);
			// 
			// button_ok
			// 
			this.button_ok.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.button_ok.Enabled = false;
			this.button_ok.Location = new System.Drawing.Point(176, 398);
			this.button_ok.Name = "button_ok";
			this.button_ok.Size = new System.Drawing.Size(75, 23);
			this.button_ok.TabIndex = 1;
			this.button_ok.Text = "OK";
			this.button_ok.UseVisualStyleBackColor = true;
			this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
			// 
			// textBox_field_specifier
			// 
			this.textBox_field_specifier.Location = new System.Drawing.Point(12, 367);
			this.textBox_field_specifier.Name = "textBox_field_specifier";
			this.textBox_field_specifier.Size = new System.Drawing.Size(239, 20);
			this.textBox_field_specifier.TabIndex = 2;
			this.textBox_field_specifier.TextChanged += new System.EventHandler(this.textBox_field_specifier_TextChanged);
			// 
			// checkBox_append
			// 
			this.checkBox_append.AutoSize = true;
			this.checkBox_append.Location = new System.Drawing.Point(12, 401);
			this.checkBox_append.Name = "checkBox_append";
			this.checkBox_append.Size = new System.Drawing.Size(63, 17);
			this.checkBox_append.TabIndex = 3;
			this.checkBox_append.Text = "Append";
			this.checkBox_append.UseVisualStyleBackColor = true;
			// 
			// choose_field_specifier
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(264, 429);
			this.Controls.Add(this.checkBox_append);
			this.Controls.Add(this.textBox_field_specifier);
			this.Controls.Add(this.button_ok);
			this.Controls.Add(this.listBox_field_specifier);
			this.Name = "choose_field_specifier";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Choose Field Specifier";
			this.Load += new System.EventHandler(this.choose_field_specifier_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox listBox_field_specifier;
		private System.Windows.Forms.Button button_ok;
		private System.Windows.Forms.TextBox textBox_field_specifier;
		private System.Windows.Forms.CheckBox checkBox_append;
	}
}