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
	public partial class enter_mmhis_direction : Form
	{
		public string the_selected_direction = "";

		public enter_mmhis_direction()
		{
			InitializeComponent();
			listBox_direction.SelectedIndex = 0;
		}

		private void button_use_selected_direction_Click(object sender, EventArgs e)
		{
			the_selected_direction = listBox_direction.Text;
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		private void button_no_direction_Click(object sender, EventArgs e)
		{
			the_selected_direction = "";
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
