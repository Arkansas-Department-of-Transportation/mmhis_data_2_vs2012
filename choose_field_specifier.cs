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
	public partial class choose_field_specifier : Form
	{
		public string which_specifier = "field"; // or "window"
		public string selected_specifier = "";

		public choose_field_specifier()
		{
			InitializeComponent();
		}

		private void choose_field_specifier_Load(object sender, EventArgs e)
		{
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new tech_serv_data_processing_modules.traffic_data_processing_general();
			tdpg.initialize_generic_members("sqlite");
			try
			{
				if (which_specifier == "field")
				{
					tdpg.construct_connection_string("", "", "", setting.data_field_format_program_setting_pathname);
					tdpg.open_conn_cmd(0);
					Text = "Choose Field Specifier";
					tdpg.cmd[0].CommandText = "select field_specifier as specifier from mmhis_data_field_format";
				}
				else if (which_specifier == "window")
				{
					tdpg.construct_connection_string("", "", "", setting.window_and_data_field_user_setting_pathname);
					tdpg.open_conn_cmd(0);
					Text = "Choose Window Specifier";
					tdpg.cmd[0].CommandText = "select window_specifier as specifier from mmhis_window";
				}
				else
				{
					Close();
				}
				tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
				while (tdpg.reader[0].Read())
				{
					listBox_field_specifier.Items.Add(tdpg.reader[0][0].ToString());
				}
			}
			catch
			{
			}
			finally
			{
				try
				{
					tdpg.reader[0].Close();
				}
				catch { }
				try
				{
					tdpg.close_conn_cmd(0);
				}
				catch { }
			}
		}

		private void listBox_field_specifier_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (checkBox_append.Checked)
			{
				textBox_field_specifier.Text += "|" + listBox_field_specifier.Text;
			}
			else
			{
				textBox_field_specifier.Text = listBox_field_specifier.Text;
			}
		}

		private void button_ok_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			selected_specifier = textBox_field_specifier.Text;
			Close();
		}

		private void textBox_field_specifier_TextChanged(object sender, EventArgs e)
		{
			button_ok.Enabled = (textBox_field_specifier.Text.Length > 0);
		}
	}
}
