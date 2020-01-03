using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mmhis_data_2_vs2012
{
	public partial class select_db_source : Form
	{
		public string mmhis_db_source = "(Not Specified)";
		public string mmhis_db = "";

		public select_db_source()
		{
			InitializeComponent();
		}

		private void button_access_Click(object sender, EventArgs e)
		{
			openFileDialog1.Filter = "MS Access Files (*.mdb)|*.mdb|All Files (*.*)|*.*";
			openFileDialog1.AddExtension = true;
			openFileDialog1.DefaultExt = "mdb";
			openFileDialog1.CheckFileExists = true;
			openFileDialog1.FileName = "AHTDMMHISMain.mdb";
			openFileDialog1.Title = "Specify MMHIS Main DB";
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				mmhis_db = openFileDialog1.FileName;
				mmhis_db_source = "Microsoft Access";
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}

		private void button_sql_server_Click(object sender, EventArgs e)
		{
			mmhis_db_source = "SQL Server";
			mmhis_db = "";
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		private void button_sqlite_Click(object sender, EventArgs e)
		{
			mmhis_db_source = "SQLite";
			mmhis_db = "";
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}
	}
}
