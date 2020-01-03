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
	public partial class data_table_view : the_sub_window
	{
		public System.Windows.Forms.Label label_logmeter;

		public BrightIdeasSoftware.ObjectListView objectListView_data_table1 = null;
		public List<Dictionary<string, string>> list_data_table = new List<Dictionary<string, string>>();

		public data_table_view()
		{
			InitializeComponent();
		}

		private void data_table_view_SizeChanged(object sender, EventArgs e)
		{
			if (objectListView_data_table1 != null)
			{
				objectListView_data_table1.Size = ClientSize;
			}
		}

		bool item_selection_done_by_code = false;
		public void label_logmeter_TextChanged(object sender, EventArgs e)
		{
			if (
				objectListView_data_table1.Focused
			)
			{
				return;
			}
			item_selection_done_by_code = true;

			// remove line segment of interest and point of interest if any
			the_parent_form.show_point_of_interest(new GMap.NET.PointLatLng(0.0, 0.0));
			the_parent_form.show_line_of_interest_segment(-1.0, -1.0);

			double logmeter = 0.0;
			double logmeter_0, logmeter_0_loaded, logmeter_1, logmeter_1_loaded;
			int index_of_first_selected_item = -1;
			int index_of_last_selected_item = -1;

			try
			{
				logmeter = Convert.ToDouble(label_logmeter.Text);
			}
			catch
			{
				return;
			}
			objectListView_data_table1.SelectedIndices.Clear();
			for (int i = 0; i < list_data_table.Count; i++)
			{
				logmeter_0 = logmeter_0_loaded = utility.get_double(list_data_table[i]["logmeter_0"], -10000.0);
				logmeter_1 = logmeter_1_loaded = utility.get_double(list_data_table[i]["logmeter_1"], -10000.0);

				// swap if necessary so the processing below can be simple
				// this is the anti-log case from data in old database
				if (
					(logmeter_1_loaded > -10000.0)
					&&
					(logmeter_0_loaded > logmeter_1_loaded)
				)
				{
					logmeter_0 = logmeter_1_loaded;
					logmeter_1 = logmeter_0_loaded;
				}

				if (
					((logmeter_0 <= logmeter) && (logmeter_1 >= logmeter))
					||
					((logmeter_1 < 0) && (logmeter_0 - 30 <= logmeter) && (logmeter_0 + 30 >= logmeter))
				)
				{
					if (index_of_first_selected_item == -1)
					{
						index_of_first_selected_item = i;
					}
					index_of_last_selected_item = i;
//					objectListView_data_table1.SelectedIndices.Add(i);
					objectListView_data_table1.SelectObject(list_data_table[i], false);
				}
			}

			// try to leave one unselected row on top
			if (index_of_first_selected_item < list_data_table.Count)
			{
				if (index_of_first_selected_item > 0)
				{
					try
					{
						objectListView_data_table1.EnsureVisible(index_of_first_selected_item - 1);
					}
					catch { }
				}
				else if (index_of_first_selected_item >= 0)
				{
					try
					{
						objectListView_data_table1.EnsureVisible(index_of_first_selected_item);
					}
					catch { }
				}
			}
			
			// try to leave one unselected row at the bottom
			if (index_of_last_selected_item >= 0)
			{
				if (index_of_last_selected_item < list_data_table.Count - 1)
				{
					try
					{
						objectListView_data_table1.EnsureVisible(index_of_last_selected_item + 1);
					}
					catch { }
				}
				else if (index_of_last_selected_item < list_data_table.Count)
				{
					try
					{
						objectListView_data_table1.EnsureVisible(index_of_last_selected_item);
					}
					catch { }
				}
			}
			item_selection_done_by_code = false;
		}

		// We want to be able to repeatedly handle this event to show the point in turn at begining logmeter
		// and ending logmeter.
		bool show_beginning_logmeter_point = false;
		int last_clicked_row_index = -1;
		long old_event_time = DateTime.Now.Ticks;
		public void objectListView_data_table1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (item_selection_done_by_code)
			{
				return;
			}
			object o = objectListView_data_table1.SelectedObject;

			if (o == null)
			{
				show_clicked_item(null, show_beginning_logmeter_point);
			}
			else
			{
				if (last_clicked_row_index != objectListView_data_table1.SelectedIndex)
				{
					show_beginning_logmeter_point = true;
					last_clicked_row_index = objectListView_data_table1.SelectedIndex;
				}
				show_clicked_item((Dictionary<string, string>)o, show_beginning_logmeter_point);
				show_beginning_logmeter_point = !show_beginning_logmeter_point;
			}
		}

		// selectedindexchanged event offered us enough so we don't need this anymore
		public void objectListView_data_table1_CellClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
		{
			//long new_event_time = DateTime.Now.Ticks;
			//Console.WriteLine("cell click:   " + new_event_time.ToString());
			//if (old_event_time + 10000000 < new_event_time)
			//{
			//	// event processed more than a second ago
			//	old_event_time = new_event_time;
			//	if (last_clicked_row_index != e.Item.Index)
			//	{
			//		show_beginning_logmeter_point = true;
			//		last_clicked_row_index = e.Item.Index;
			//	}
			//	show_clicked_item(list_data_table[e.Item.Index]);
			//	show_beginning_logmeter_point = !show_beginning_logmeter_point;
			//}
		}

		private void show_clicked_item(Dictionary<string, string> row, bool show_beginning_logmeter_point)
		{
			double logmeter = -1000.0;
			double logmeter_0 = -1000.0;
			double logmeter_1 = -1000.0;

			if (row == null)
			{
				the_parent_form.show_point_of_interest(new GMap.NET.PointLatLng(0.0, 0.0));
				the_parent_form.show_line_of_interest_segment(-1.0, -1.0);
			}
			else
			{
				logmeter_0 = utility.get_double(row["logmeter_0"], -1000.0);
				logmeter_1 = utility.get_double(row["logmeter_1"], -1000.0);
				if (logmeter_1 < 0)
				{
					logmeter_1 = logmeter_0;
				}
				if (show_beginning_logmeter_point)
				{
					logmeter = logmeter_0;
				}
				else
				{
					logmeter = logmeter_1;
				}
				if (logmeter >= 0)
				{
					the_parent_form.go_to_logmeter(logmeter);
				}
				the_parent_form.show_point_of_interest(
					new GMap.NET.PointLatLng(
						utility.get_double(row["latitude"], 0.0),
						utility.get_double(row["longitude"], 0.0)
					)
				);
				the_parent_form.show_line_of_interest_segment(
					logmeter_0,
					logmeter_1
				);
			}
		}
	}
}
