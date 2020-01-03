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
	public partial class opacity_adjustment_dialog : Form
	{
		public Form1 the_parent = null;

		public opacity_adjustment_dialog()
		{
			InitializeComponent();
		}

		private void button_ok_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void trackBar_opacity_ValueChanged(object sender, EventArgs e)
		{
			the_parent.current_window_detail.opacity = trackBar_opacity.Value;
			the_parent.save_updated_window_details();
			the_parent.apply_window_properties();
		}
	}
}
