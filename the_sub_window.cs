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
	public partial class the_sub_window : Form
	{
		// the creator is responsible for setting the value of this
		public int objectlistview_item_index = 0;
		public Form1 the_parent_form = null;

		public the_sub_window()
		{
			InitializeComponent();
		}

		private void the_sub_window_FormClosing(object sender, FormClosingEventArgs e)
		{
			the_parent_form.list_window_detail[objectlistview_item_index].window_visibility = false;
			the_parent_form.objectListView_windows.RefreshObject(
				the_parent_form.list_window_detail[objectlistview_item_index]
			);
			the_parent_form.save_updated_window_details((window_detail)(the_parent_form.list_window_detail[objectlistview_item_index]));
			Visible = false;
			e.Cancel = true;
		}

		private void the_sub_window_Activated(object sender, EventArgs e)
		{
			set_parent_current_record();
		}

		private bool set_parent_current_record()
		{
			try
			{
				the_parent_form.current_window_detail = the_parent_form.list_window_detail[objectlistview_item_index];
				the_parent_form.objectListView_windows.SelectedIndex = objectlistview_item_index;
				the_parent_form.objectListView_windows.EnsureVisible(objectlistview_item_index);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private void the_sub_window_LocationChanged(object sender, EventArgs e)
		{
			if (set_parent_current_record())
			{
				the_parent_form.current_window_detail.left = this.Left;
				the_parent_form.current_window_detail.top = this.Top;
				the_parent_form.current_window_detail.save_operation = "update";
				the_parent_form.objectListView_windows.RefreshObject(the_parent_form.current_window_detail);
				the_parent_form.save_updated_window_details((window_detail)(the_parent_form.list_window_detail[objectlistview_item_index]));
			}
		}

		private void the_sub_window_SizeChanged(object sender, EventArgs e)
		{
			if (set_parent_current_record())
			{
				the_parent_form.current_window_detail.width = this.Width;
				the_parent_form.current_window_detail.height = this.Height;
				the_parent_form.current_window_detail.save_operation = "update";
				the_parent_form.objectListView_windows.RefreshObject(the_parent_form.current_window_detail);
				the_parent_form.save_updated_window_details((window_detail)(the_parent_form.list_window_detail[objectlistview_item_index]));
			}
		}
	}
}
