namespace mmhis_data_2_vs2012
{
	partial class data_table_view
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
			this.objectListView1 = new BrightIdeasSoftware.ObjectListView();
			this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
			this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
			this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
			((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
			this.SuspendLayout();
			// 
			// objectListView1
			// 
			this.objectListView1.AllColumns.Add(this.olvColumn1);
			this.objectListView1.AllColumns.Add(this.olvColumn2);
			this.objectListView1.AllColumns.Add(this.olvColumn3);
			this.objectListView1.AlternateRowBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
			this.objectListView1.CellEditUseWholeCell = false;
			this.objectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn2,
            this.olvColumn3});
			this.objectListView1.Cursor = System.Windows.Forms.Cursors.Default;
			this.objectListView1.FullRowSelect = true;
			this.objectListView1.HideSelection = false;
			this.objectListView1.Location = new System.Drawing.Point(0, 0);
			this.objectListView1.Name = "objectListView1";
			this.objectListView1.Size = new System.Drawing.Size(280, 252);
			this.objectListView1.TabIndex = 0;
			this.objectListView1.UseAlternatingBackColors = true;
			this.objectListView1.UseCompatibleStateImageBehavior = false;
			this.objectListView1.View = System.Windows.Forms.View.Details;
			// 
			// data_table_view
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Controls.Add(this.objectListView1);
			this.Name = "data_table_view";
			this.Text = "data_table_view";
			this.SizeChanged += new System.EventHandler(this.data_table_view_SizeChanged);
			((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private BrightIdeasSoftware.ObjectListView objectListView1;
		private BrightIdeasSoftware.OLVColumn olvColumn1;
		private BrightIdeasSoftware.OLVColumn olvColumn2;
		private BrightIdeasSoftware.OLVColumn olvColumn3;


	}
}