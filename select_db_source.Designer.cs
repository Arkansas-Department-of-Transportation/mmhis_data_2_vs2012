namespace mmhis_data_2_vs2012
{
	partial class select_db_source
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
			this.button_access = new System.Windows.Forms.Button();
			this.button_sql_server = new System.Windows.Forms.Button();
			this.button_sqlite = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(22, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(166, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Please Select MMHIS DB Source";
			// 
			// button_access
			// 
			this.button_access.Location = new System.Drawing.Point(25, 52);
			this.button_access.Name = "button_access";
			this.button_access.Size = new System.Drawing.Size(200, 23);
			this.button_access.TabIndex = 1;
			this.button_access.Text = "Microsoft Access Database";
			this.button_access.UseVisualStyleBackColor = true;
			this.button_access.Click += new System.EventHandler(this.button_access_Click);
			// 
			// button_sql_server
			// 
			this.button_sql_server.Location = new System.Drawing.Point(25, 93);
			this.button_sql_server.Name = "button_sql_server";
			this.button_sql_server.Size = new System.Drawing.Size(200, 23);
			this.button_sql_server.TabIndex = 2;
			this.button_sql_server.Text = "Microsoft SQL Server";
			this.button_sql_server.UseVisualStyleBackColor = true;
			this.button_sql_server.Click += new System.EventHandler(this.button_sql_server_Click);
			// 
			// button_sqlite
			// 
			this.button_sqlite.Location = new System.Drawing.Point(25, 139);
			this.button_sqlite.Name = "button_sqlite";
			this.button_sqlite.Size = new System.Drawing.Size(200, 23);
			this.button_sqlite.TabIndex = 3;
			this.button_sqlite.Text = "SQLite Database";
			this.button_sqlite.UseVisualStyleBackColor = true;
			this.button_sqlite.Click += new System.EventHandler(this.button_sqlite_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// select_db_source
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(251, 186);
			this.Controls.Add(this.button_sqlite);
			this.Controls.Add(this.button_sql_server);
			this.Controls.Add(this.button_access);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "select_db_source";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select DB Source";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button_access;
		private System.Windows.Forms.Button button_sql_server;
		private System.Windows.Forms.Button button_sqlite;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
	}
}