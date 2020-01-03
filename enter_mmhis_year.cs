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
	public partial class enter_mmhis_year : Form
	{
		public string the_year;

		public enter_mmhis_year()
		{
			InitializeComponent();
		}

		private void enter_mmhis_year_Load(object sender, EventArgs e)
		{
			textBox_year.Text = the_year;
		}

		private void button_accept_year_number_Click(object sender, EventArgs e)
		{
			the_year = textBox_year.Text.Trim();
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}
	}
}
