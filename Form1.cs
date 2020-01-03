using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data.SQLite;

using System.IO;

// for the current_function_name() function
using System.Diagnostics;
using tech_serv_data_processing_modules;

using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms.Markers;
using System.Runtime.CompilerServices;

namespace mmhis_data_2_vs2012
{
	public partial class Form1 : Form
	{
		traffic_data_processing_general tdpg_in_memory = new traffic_data_processing_general();

		// the following is for debugging purpose: I want to be able to break execution
		// when a certain error message appears.
		private string the_error_message = "";
		public string error_message
		{
			get
			{
				return this.the_error_message;
			}
			set
			{
				this.the_error_message = value;
			}
		}

		private bool form_loaded = false;

		private string mmhis_db_source = ""; // "Microsoft Access", "SQL Server", "SQLite"

		private string the_county = ""; // short conuty (no leading zero)
		private string the_route = ""; // short format without direction. use utility.five_digit_route() and utility.short_route() to convert
		private string the_year = "";
		private string the_section = ""; // short section, use three_character_section() to convert
		private string the_note = "";
		private string the_road_id = "";
		private string mmhis_direction = ""; // "N", "S", "E", "W"
		private string target_table_name_prefix = "";
		private string road_id_with_no_direction = "";
		private string road_id_direction = ""; // "A", "B"

		private Color color_combobox_forecolor_default = SystemColors.ControlText;
		private Color color_combobox_backcolor_default = SystemColors.ControlLight;
		private Color color_combobox_forecolor_warning = Color.Red;
		private Color color_combobox_backcolor_warning = Color.Yellow;

		// this is to help debugging
		[MethodImpl(MethodImplOptions.NoInlining)]
		public string current_function_name()
		{
			var st = new StackTrace();
			var sf = st.GetFrame(1);

			return "\n" + sf.GetMethod().Name;
		}

		public Form1()
		{
			InitializeComponent();
			dlee_console_application.console_visibility.visible = false;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			if (!setting.prepare_user_setting_file())
			{
				MessageBox.Show(
					"MMHIS is not setup correctly.",
					"MMHIS"
				);
				Close();
			}

			color_combobox_forecolor_default = comboBox_route.ForeColor;
			color_combobox_backcolor_default = comboBox_route.BackColor;
			label_status.ForeColor = Color.Black;

			set_control_text();

			initialize_objectlistview_windows();
			initialize_objectListView_data_field();
			utility.prepare_sqlite_for_string_evaluation();

			load_window();
			load_data_field();

			form_loaded = true;
		}

		private void set_control_text()
		{
			foreach (Control c in tabControl1.TabPages["tabPage_roadlog_accident_job_bridge"].Controls)
			{
				setting.set_control_text(c);
			}

			foreach (Control c in tabControl1.TabPages["tabPage_pms_frameindex"].Controls)
			{
				setting.set_control_text(c);
			}

			foreach (Control c in tabControl1.TabPages["tabPage_preview"].Controls)
			{
				setting.set_control_text(c);
			}
		}

		private void update_dictionary_control_text()
		{
			foreach (Control c in tabControl1.TabPages["tabPage_roadlog_accident_job_bridge"].Controls)
			{
				setting.update_dictionary_control_text(c);
			}

			foreach (Control c in tabControl1.TabPages["tabPage_pms_frameindex"].Controls)
			{
				setting.update_dictionary_control_text(c);
			}

			foreach (Control c in tabControl1.TabPages["tabPage_preview"].Controls)
			{
				setting.update_dictionary_control_text(c);
			}
		}

		private void show_status(string msg = "")
		{
			label_status.Text = msg;
			label_status.Refresh();
			System.Threading.Thread.Sleep(0);
		}

#region objectlistview_windows related
		// for objectlistview
		public List<window_detail> list_window_detail = new List<window_detail>();
		public window_detail current_window_detail = null;

		// objectlistView_windows will be the place any time we want to deal with sub-windows
		private void initialize_objectlistview_windows()
		{
			objectListView_windows.FullRowSelect = true;

			// the following is in place so that if the window_type is not "map" these
			// cells should show nothing
			olvColumn_window_map_line_width_0.AspectToStringConverter = delegate(
				object x // represents the object for the current cell
			)
			{
				return utility.aspect_to_string<int>(x, 0);
			};
			olvColumn_window_map_line_width_1.AspectToStringConverter = delegate(
				object x // represents the object for the current cell
			)
			{
				return utility.aspect_to_string<int>(x, 0);
			};
			olvColumn_window_map_initial_center_latitude.AspectToStringConverter = delegate(
				object x // represents the object for the current cell
			)
			{
				return utility.aspect_to_string<double>(x, -1000.0);
			};
			olvColumn_window_map_initial_center_longitude.AspectToStringConverter = delegate(
				object x // represents the object for the current cell
			)
			{
				return utility.aspect_to_string<double>(x, -1000.0);
			};
			olvColumn_window_map_initial_zoom_level.AspectToStringConverter = delegate(
				object x // represents the object for the current cell
			)
			{
				return utility.aspect_to_string<double>(x, 0.0);
			};
			olvColumn_window_map_zoom_minimum.AspectToStringConverter = delegate(
				object x // represents the object for the current cell
			)
			{
				return utility.aspect_to_string<int>(x, 0);
			};
			olvColumn_window_map_zoom_maximum.AspectToStringConverter = delegate(
				object x // represents the object for the current cell
			)
			{
				return utility.aspect_to_string<int>(x, 0);
			};

			olvColumn_window_opacity.RendererDelegate = delegate(
				EventArgs e,
				Graphics g,
				Rectangle r,
				Object rowObject
			)
			{
				int opacity = ((window_detail)rowObject).opacity;
				g.FillRectangle(new SolidBrush(Color.White), r);
				if (r.Height < r.Width * 2)
				{
					r.X += Convert.ToInt32(r.Height / 4.0);
					r.Width -= Convert.ToInt32(r.Height / 1.4);
				}
				r.Y += Convert.ToInt32(r.Height / 4.0);
				r.Height = Convert.ToInt32(r.Height / 2.0);
				r.Width = Convert.ToInt32(r.Width * (opacity / 100.0));
				g.FillRectangle(new SolidBrush(Color.Blue), r);
				return true;
			};
			olvColumn_window_background_color.RendererDelegate = delegate(
				EventArgs e,
				Graphics g,
				Rectangle r,
				Object rowObject
			)
			{
				return utility.draw_color(g, r, ((window_detail)rowObject).background_color);
			};

			olvColumn_window_map_line_color_0.RendererDelegate = delegate(
				EventArgs e,
				Graphics g,
				Rectangle r,
				Object rowObject
			)
			{
				return utility.draw_color(g, r, ((window_detail)rowObject).map_line_color_0);
			};

			olvColumn_window_map_line_color_1.RendererDelegate = delegate(
				EventArgs e,
				Graphics g,
				Rectangle r,
				Object rowObject
			)
			{
				return utility.draw_color(g, r, ((window_detail)rowObject).map_line_color_1);
			};

			objectListView_windows.BooleanCheckStatePutter = delegate(object rowObject, bool newValue)
			{
				current_window_detail = (window_detail)rowObject;
				current_window_detail.window_visibility = Convert.ToBoolean(newValue);
				objectListView_windows.SelectedObject = current_window_detail;
				apply_window_properties(current_window_detail);
				Console.WriteLine("240");
				save_updated_window_details();

				// this is only for video frame so for other window it will throw an exception
				try
				{
					((video_frame)current_window_detail.the_window).show_image();
				}
				catch { }

				// this is only for map so for other window it will throw an exception
				try
				{
					((Map)current_window_detail.the_window).show_current_gps_location();
				}
				catch { }

				return newValue; // return the value that you want the control to use
			};

			olvColumn_window_control_box.AspectPutter = delegate(object rowObject, object newValue)
			{
				current_window_detail = (window_detail)rowObject;
				current_window_detail.control_box = Convert.ToBoolean(newValue);
				objectListView_windows.SelectedObject = current_window_detail;
				apply_window_properties(current_window_detail);
				Console.WriteLine("266");
				save_updated_window_details();
			};

			olvColumn_window_maximize_box.AspectPutter = delegate(object rowObject, object newValue)
			{
				current_window_detail = (window_detail)rowObject;
				current_window_detail.maximize_box = Convert.ToBoolean(newValue);
				objectListView_windows.SelectedObject = current_window_detail;
				apply_window_properties(current_window_detail);
				Console.WriteLine("276");
				save_updated_window_details();
			};

			olvColumn_window_minimize_box.AspectPutter = delegate(object rowObject, object newValue)
			{
				current_window_detail = (window_detail)rowObject;
				current_window_detail.minimize_box = Convert.ToBoolean(newValue);
				objectListView_windows.SelectedObject = current_window_detail;
				apply_window_properties(current_window_detail);
				Console.WriteLine("286");
				save_updated_window_details();
			};

			olvColumn_window_top_most.AspectPutter = delegate(object rowObject, object newValue)
			{
				current_window_detail = (window_detail)rowObject;
				current_window_detail.top_most = Convert.ToBoolean(newValue);
				objectListView_windows.SelectedObject = current_window_detail;
				apply_window_properties(current_window_detail);
				Console.WriteLine("296");
				save_updated_window_details();
			};

			olvColumn_window_show_icon.AspectPutter = delegate(object rowObject, object newValue)
			{
				current_window_detail = (window_detail)rowObject;
				current_window_detail.show_icon = Convert.ToBoolean(newValue);
				objectListView_windows.SelectedObject = current_window_detail;
				apply_window_properties(current_window_detail);
				Console.WriteLine("306");
				save_updated_window_details();
			};

			olvColumn_window_show_in_taskbar.AspectPutter = delegate(object rowObject, object newValue)
			{
				current_window_detail = (window_detail)rowObject;
				current_window_detail.show_in_taskbar = Convert.ToBoolean(newValue);
				objectListView_windows.SelectedObject = current_window_detail;
				apply_window_properties(current_window_detail);
				Console.WriteLine("316");
				save_updated_window_details();
			};

			olvColumn_window_map_center_current_location.AspectPutter = delegate(object rowObject, object newValue)
			{
				current_window_detail = (window_detail)rowObject;
				current_window_detail.map_center_current_location = Convert.ToBoolean(newValue);
				objectListView_windows.SelectedObject = current_window_detail;
				apply_gmap_properties(current_window_detail, true);
				Console.WriteLine("326");
				save_updated_window_details();
			};
		}

		private bool loading_window = false;
		private void load_window()
		{
			loading_window = true;
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();
			// destroy windows if any
			foreach (window_detail the_window_detail in list_window_detail)
			{
				try
				{
					the_window_detail.the_window.Close();
				}
				catch { }
			}

			tdpg.initialize_generic_members("sqlite");
			tdpg.construct_connection_string(
				"",
				"",
				"",
				setting.window_and_data_field_user_setting_pathname
			);
			tdpg.open_conn_cmd(0);
			tdpg.cmd[0].CommandText = "select * from mmhis_window";
			tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
			int item_number_in_windows_list = 0;
			list_window_detail.Clear();
			objectListView_windows.RefreshObjects(list_window_detail);

			while (tdpg.reader[0].Read())
			{
				window_detail the_window_detail = new window_detail("");
				the_window_detail.window_type = utility.get_string(tdpg.reader[0]["window_type"]);
				if (the_window_detail.window_type == "image")
				{
					the_window_detail.the_window = new video_frame();
				}
				else if (the_window_detail.window_type == "data")
				{
					the_window_detail.the_window = new site_data();
				}
				else if (the_window_detail.window_type == "map")
				{
					the_window_detail.the_window = new Map();
				}
				else if (the_window_detail.window_type == "data_table")
				{
					the_window_detail.the_window = new data_table_view();
				}
				else
				{
					// this happens when the user add a window record but did not specify a correct type
					MessageBox.Show("Wrong window type:" + the_window_detail.window_type);
				}

				the_window_detail.uuid = utility.get_string(tdpg.reader[0]["uuid"]);
				the_window_detail.user_id = utility.get_string(tdpg.reader[0]["user_id"], "unknown_user");
				the_window_detail.window_group_id = utility.get_string(tdpg.reader[0]["window_group_id"], "unknown_window_group_id");
				the_window_detail.window_specifier = utility.get_string(tdpg.reader[0]["window_specifier"], "unknown_window_specifier");
				the_window_detail.window_visibility = utility.get_boolean_from_int(tdpg.reader[0]["window_visibility"]);
				the_window_detail.caption_text = utility.get_string(tdpg.reader[0]["caption_text"], "unknown_caption");
				the_window_detail.control_box = utility.get_boolean_from_int(tdpg.reader[0]["control_box"]);
				the_window_detail.maximize_box = utility.get_boolean_from_int(tdpg.reader[0]["maximize_box"]);
				the_window_detail.minimize_box = utility.get_boolean_from_int(tdpg.reader[0]["minimize_box"]);
				the_window_detail.opacity = utility.get_int(tdpg.reader[0]["opacity"], 100);
				the_window_detail.top_most = utility.get_boolean_from_int(tdpg.reader[0]["top_most"]);
				the_window_detail.show_icon = utility.get_boolean_from_int(tdpg.reader[0]["show_icon"]);
				the_window_detail.show_in_taskbar = utility.get_boolean_from_int(tdpg.reader[0]["show_in_taskbar"]);
				the_window_detail.border_style = utility.get_string(tdpg.reader[0]["border_style"], "Sizable");
				the_window_detail.left = utility.get_int(tdpg.reader[0]["left"], 0);
				the_window_detail.top = utility.get_int(tdpg.reader[0]["top"], 0);
				the_window_detail.width = utility.get_int(tdpg.reader[0]["width"], 100);
				the_window_detail.height = utility.get_int(tdpg.reader[0]["height"], 100);
				the_window_detail.background_color = utility.get_string(tdpg.reader[0]["background_color"], "000000");

				the_window_detail.map_provider = utility.get_string(tdpg.reader[0]["map_provider"], "GoogleMapProvider");
				the_window_detail.map_line_color_0 = utility.get_string(tdpg.reader[0]["map_line_color_0"], "0000ff");
				the_window_detail.map_line_width_0 = utility.get_int(tdpg.reader[0]["map_line_width_0"], 1);
				the_window_detail.map_line_color_1 = utility.get_string(tdpg.reader[0]["map_line_color_1"], "00ff00");
				the_window_detail.map_line_width_1 = utility.get_int(tdpg.reader[0]["map_line_width_1"], 2);
				the_window_detail.map_initial_center_latitude = utility.get_double(tdpg.reader[0]["map_initial_center_latitude"], 34.671066);
				the_window_detail.map_initial_center_longitude = utility.get_double(tdpg.reader[0]["map_initial_center_longitude"], -92.382895);
				the_window_detail.map_initial_zoom_level = utility.get_double(tdpg.reader[0]["map_initial_zoom_level"], 10.0);
				the_window_detail.map_zoom_minimum = utility.get_int(tdpg.reader[0]["map_zoom_minimum"], 0);
				the_window_detail.map_zoom_maximum = utility.get_int(tdpg.reader[0]["map_zoom_maximum"], 22);
				the_window_detail.map_center_current_location = utility.get_boolean_from_int(tdpg.reader[0]["map_center_current_location"]);
	
				the_window_detail.save_operation = "";

				list_window_detail.Add(the_window_detail);
				try
				{
					the_window_detail.the_window.objectlistview_item_index = item_number_in_windows_list++;
					the_window_detail.the_window.the_parent_form = this;
				}
				catch { }

				// note that all subwindows are derived from "the_sub_window" so
				// that the field "objectlistview_item_index" can be added
				// each window's "objectlistview_item_index" saves the window's corrosponding
				// objectlistview item's index number so when the window is being hidden, it knows which
				// objectlistview item to uncheck.
				// in addition to the above, the field "the_parent_form" is offered so
				// the sub windows can get hold of the parent window.

				apply_window_properties(the_window_detail);
			}
			tdpg.reader[0].Close();
			tdpg.close_conn_cmd(0);

			objectListView_windows.SetObjects(list_window_detail);

			objectListView_windows.AddDecoration(
				new BrightIdeasSoftware.EditingCellBorderDecoration { UseLightbox = true }
			);

			set_image_path_replace();

			current_window_detail = null;
			loading_window = false;
		}

		private void set_image_path_replace()
		{
			foreach (window_detail the_window_detail in list_window_detail)
			{
				// not all windows have these properties
				// we use try block to handle this
				if (the_window_detail.window_type == "image")
				{
					((video_frame)(the_window_detail.the_window)).image_path_replace = textBox_preview_image_path_replace.Text;
					((video_frame)(the_window_detail.the_window)).image_path_replace_with = textBox_preview_image_path_replace_with.Text;
				}
			}
		}

		public void save_updated_window_details(window_detail the_window_detail = null)
		{
			if (the_window_detail == null)
			{
				the_window_detail = current_window_detail;
			}
			if (
				the_window_detail != null
				&&
				(
					(the_window_detail.save_operation == null)
					||
					(the_window_detail.save_operation == "")
				)
			)
			{
				the_window_detail.save_operation = "update";
			}
			//Console.WriteLine("453");
			save_window_details_changes(the_window_detail);
		}

		public bool save_window_details_changes(window_detail the_window_detail = null)
		{
			if (loading_window)
			{
				return true;
			}

			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();
			if (the_window_detail == null)
			{
				the_window_detail = current_window_detail;
			}
			if (the_window_detail == null)
			{
				return true;
			}
			if ((the_window_detail.save_operation == null) || (the_window_detail.save_operation == ""))
			{
				return true;
			}
			bool b_return = true;

			tdpg.initialize_generic_members("sqlite");
			tdpg.construct_connection_string(
				"",
				"",
				"",
				setting.window_and_data_field_user_setting_pathname
			);
			tdpg.open_conn_cmd(0);
			SQLiteCommand the_cmd = (SQLiteCommand)(tdpg.cmd[0]);
			if (the_window_detail.save_operation == "update")
			{
				the_cmd.CommandText =
					string.Format(
						"update {0} set" +
						" [{1}]=@{1}" +
						",[{2}]=@{2}" +
						",[{3}]=@{3}" +
						",[{4}]=@{4}" +
						",[{5}]=@{5}" +
						",[{6}]=@{6}" +
						",[{7}]=@{7}" +
						",[{8}]=@{8}" +
						",[{9}]=@{9}" +
						",[{10}]=@{10}" +
						",[{11}]=@{11}" +
						",[{12}]=@{12}" +
						",[{13}]=@{13}" +
						",[{14}]=@{14}" +
						",[{15}]=@{15}" +
						",[{16}]=@{16}" +
						",[{17}]=@{17}" +
						",[{18}]=@{18}" +
						",[{19}]=@{19}" +
						",[{20}]=@{20}" +
						",[{21}]=@{21}" +
						",[{22}]=@{22}" +
						",[{23}]=@{23}" +
						",[{24}]=@{24}" +
						",[{25}]=@{25}" +
						",[{26}]=@{26}" +
						",[{27}]=@{27}" +
						",[{28}]=@{28}" +
						" where {29}=@{29}",
						"mmhis_window",
						"window_specifier",
						"window_visibility",
						"window_type",
						"caption_text",
						"control_box",
						"maximize_box",
						"minimize_box",
						"opacity",
						"top_most",
						"show_icon",
						"show_in_taskbar",
						"border_style",
						"left",
						"top",
						"width",
						"height",
						"background_color",
						"map_provider",
						"map_line_color_0",
						"map_line_width_0",
						"map_line_color_1",
						"map_line_width_1",
						"map_initial_center_latitude",
						"map_initial_center_longitude",
						"map_initial_zoom_level",
						"map_zoom_minimum",
						"map_zoom_maximum",
						"map_center_current_location",
						"uuid"
					);
			}
			else if (the_window_detail.save_operation == "insert")
			{
				the_cmd.CommandText =
					string.Format(
						"insert into {0} " +
						"([{1}]" +
						",[{2}]" +
						",[{3}]" +
						",[{4}]" +
						",[{5}]" +
						",[{6}]" +
						",[{7}]" +
						",[{8}]" +
						",[{9}]" +
						",[{10}]" +
						",[{11}]" +
						",[{12}]" +
						",[{13}]" +
						",[{14}]" +
						",[{15}]" +
						",[{16}]" +
						",[{17}]" +
						",[{18}]" +
						",[{19}]" +
						",[{20}]" +
						",[{21}]" +
						",[{22}]" +
						",[{23}]" +
						",[{24}]" +
						",[{25}]" +
						",[{26}]" +
						",[{27}]" +
						",[{28}]" +
						",[{29}]" +
						",[{30}]" +
						",[{31}]" +
						")values" +
						"(@{1}" +
						",@{2}" +
						",@{3}" +
						",@{4}" +
						",@{5}" +
						",@{6}" +
						",@{7}" +
						",@{8}" +
						",@{9}" +
						",@{10}" +
						",@{11}" +
						",@{12}" +
						",@{13}" +
						",@{14}" +
						",@{15}" +
						",@{16}" +
						",@{17}" +
						",@{18}" +
						",@{19}" +
						",@{20}" +
						",@{21}" +
						",@{22}" +
						",@{23}" +
						",@{24}" +
						",@{25}" +
						",@{26}" +
						",@{27}" +
						",@{28}" +
						",@{29}" +
						",@{30}" +
						",@{31}" +
						")",
						"mmhis_window",
						"window_specifier",
						"window_visibility",
						"window_type",
						"caption_text",
						"control_box",
						"maximize_box",
						"minimize_box",
						"opacity",
						"top_most",
						"show_icon",
						"show_in_taskbar",
						"border_style",
						"left",
						"top",
						"width",
						"height",
						"background_color",
						"map_provider",
						"map_line_color_0",
						"map_line_width_0",
						"map_line_color_1",
						"map_line_width_1",
						"map_initial_center_latitude",
						"map_initial_center_longitude",
						"map_initial_zoom_level",
						"map_zoom_minimum",
						"map_zoom_maximum",
						"map_center_current_location",
						"uuid",
						"user_id",
						"window_group_id"
					);
			}
			else
			{
				b_return = false;
			}

			the_cmd.Parameters.Clear();
			the_cmd.Parameters.Add("window_specifier", System.Data.DbType.String);
			the_cmd.Parameters.Add("window_visibility", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("window_type", System.Data.DbType.String);
			the_cmd.Parameters.Add("caption_text", System.Data.DbType.String);
			the_cmd.Parameters.Add("control_box", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("maximize_box", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("minimize_box", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("opacity", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("top_most", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("show_icon", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("show_in_taskbar", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("border_style", System.Data.DbType.String);
			the_cmd.Parameters.Add("left", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("top", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("width", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("height", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("background_color", System.Data.DbType.String);
			the_cmd.Parameters.Add("map_provider", System.Data.DbType.String);
			the_cmd.Parameters.Add("map_line_color_0", System.Data.DbType.String);
			the_cmd.Parameters.Add("map_line_width_0", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("map_line_color_1", System.Data.DbType.String);
			the_cmd.Parameters.Add("map_line_width_1", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("map_initial_center_latitude", System.Data.DbType.Double);
			the_cmd.Parameters.Add("map_initial_center_longitude", System.Data.DbType.Double);
			the_cmd.Parameters.Add("map_initial_zoom_level", System.Data.DbType.Double);
			the_cmd.Parameters.Add("map_zoom_minimum", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("map_zoom_maximum", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("map_center_current_location", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("uuid", System.Data.DbType.String);
			the_cmd.Parameters.Add("user_id", System.Data.DbType.String);
			the_cmd.Parameters.Add("window_group_id", System.Data.DbType.String);

			the_cmd.Parameters["window_specifier"].Value = the_window_detail.window_specifier;
			the_cmd.Parameters["window_visibility"].Value = the_window_detail.window_visibility ? 1 : 0;
			the_cmd.Parameters["window_type"].Value = the_window_detail.window_type;
			the_cmd.Parameters["caption_text"].Value = the_window_detail.caption_text;
			the_cmd.Parameters["control_box"].Value = the_window_detail.control_box ? 1 : 0;
			the_cmd.Parameters["maximize_box"].Value = the_window_detail.maximize_box ? 1 : 0;
			the_cmd.Parameters["minimize_box"].Value = the_window_detail.minimize_box ? 1 : 0;
			the_cmd.Parameters["opacity"].Value = the_window_detail.opacity;
			the_cmd.Parameters["top_most"].Value = the_window_detail.top_most ? 1 : 0;
			the_cmd.Parameters["show_icon"].Value = the_window_detail.show_icon ? 1 : 0;
			the_cmd.Parameters["show_in_taskbar"].Value = the_window_detail.show_in_taskbar ? 1 : 0;
			the_cmd.Parameters["border_style"].Value = the_window_detail.border_style;
			the_cmd.Parameters["left"].Value = the_window_detail.left;
			the_cmd.Parameters["top"].Value = the_window_detail.top;
			the_cmd.Parameters["width"].Value = the_window_detail.width;
			the_cmd.Parameters["height"].Value = the_window_detail.height;
			the_cmd.Parameters["background_color"].Value = the_window_detail.background_color;

			// here we use the members behind each map properties because we want to
			// save the underlying values no matter what window type it has.
			the_cmd.Parameters["map_provider"].Value = the_window_detail.the_map_provider;
			the_cmd.Parameters["map_line_color_0"].Value = the_window_detail.the_map_line_color_0;
			the_cmd.Parameters["map_line_width_0"].Value = the_window_detail.the_map_line_width_0;
			the_cmd.Parameters["map_line_color_1"].Value = the_window_detail.the_map_line_color_1;
			the_cmd.Parameters["map_line_width_1"].Value = the_window_detail.the_map_line_width_1;
			the_cmd.Parameters["map_initial_center_latitude"].Value = the_window_detail.the_map_initial_center_latitude;
			the_cmd.Parameters["map_initial_center_longitude"].Value = the_window_detail.the_map_initial_center_longitude;
			the_cmd.Parameters["map_initial_zoom_level"].Value = the_window_detail.the_map_initial_zoom_level;
			the_cmd.Parameters["map_zoom_minimum"].Value = the_window_detail.the_map_zoom_minimum;
			the_cmd.Parameters["map_zoom_maximum"].Value = the_window_detail.the_map_zoom_maximum;
			the_cmd.Parameters["map_center_current_location"].Value = the_window_detail.the_map_center_current_location ? 1 : 0;

			the_cmd.Parameters["uuid"].Value = the_window_detail.uuid;
			the_cmd.Parameters["user_id"].Value = the_window_detail.user_id;
			the_cmd.Parameters["window_group_id"].Value = the_window_detail.window_group_id;

			try
			{
				the_cmd.ExecuteNonQuery();
				the_window_detail.save_operation = "";
				b_return = true;
			}
			catch (Exception exc)
			{
				MessageBox.Show(current_function_name() + "(): 654    " + exc.Message);
				b_return = false;
			}
			finally
			{
				tdpg.close_conn_cmd(0);
			}
			return b_return;
		}

		private void objectListView_windows_SelectedIndexChanged(object sender, EventArgs e)
		{
			object o = objectListView_windows.SelectedObject;

			if (o != null)
			{
				current_window_detail = (window_detail)o;
				try
				{
					current_window_detail.the_window.BringToFront();
				}
				catch { }
			}
			else
			{
				current_window_detail = null;
			}
			button_preview_delete_window.Enabled =
				(objectListView_windows.SelectedIndices.Count > 0);
			checkBox_preview_move_window.Enabled =
				checkBox_preview_window_change_size.Enabled =
				(objectListView_windows.SelectedIndices.Count == 1);
		}

		private void delete_window_detail_record()
		{
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();
			tdpg.initialize_generic_members("sqlite");
			tdpg.construct_connection_string(
				"",
				"",
				"",
				setting.window_and_data_field_user_setting_pathname
			);
			tdpg.open_conn_cmd(0);
			SQLiteCommand the_cmd = (SQLiteCommand)(tdpg.cmd[0]);

			the_cmd.CommandText =
				"delete from [mmhis_window]" + window_delete_uuid_where_clause();
			try
			{
				the_cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				MessageBox.Show(current_function_name() + "(): 741     " + exc.Message);
			}
			finally
			{
				tdpg.close_conn_cmd(0);
			}

			load_window();
			load_data_field();
			update_all_views(top_level_uuid);
		}

		private string window_delete_uuid_where_clause()
		{
			StringBuilder where_clause =
				 new StringBuilder("where (1=0)");
			// To simply the following processing

			for (int i = objectListView_windows.Items.Count - 1; i >= 0; i--)
			{
				if (objectListView_windows.Items[i].Selected)
				{
					where_clause.Append(
						"or([uuid]='" +
						// Here shows how the object for a certain row
						// can be retrieved using an index.
						((window_detail)(objectListView_windows.GetModelObject(i))).uuid +
						"')"
					);
				}
			}
			return where_clause.ToString();
		}

		public void add_window_detail_record()
		{
			window_detail the_window_detail =
				new window_detail(
					System.Guid.NewGuid().ToString("N"),
					"insert"
				);
			the_window_detail.background_color = "FF0000";
			the_window_detail.border_style = "Sizable";
			the_window_detail.caption_text = "new window";
			the_window_detail.height = 100;
			the_window_detail.left = 0;
			the_window_detail.map_center_current_location = true;
			the_window_detail.map_initial_center_latitude = 34.671167;
			the_window_detail.map_initial_center_longitude = -92.382576;
			the_window_detail.map_initial_zoom_level = 10.0;
			the_window_detail.map_line_color_0 = "0000FF";
			the_window_detail.map_line_width_0 = 3;
			the_window_detail.map_line_color_1 = "00FF00";
			the_window_detail.map_line_width_1 = 4;
			the_window_detail.map_provider = "GoogleMapProvider";
			the_window_detail.map_zoom_maximum = 22;
			the_window_detail.map_zoom_minimum = 0;
			the_window_detail.opacity = 100;
			the_window_detail.save_operation = "insert";
			the_window_detail.show_icon = false;
			the_window_detail.show_in_taskbar = false;
			the_window_detail.top_most = false;
			the_window_detail.user_id = setting.user_id;
			the_window_detail.uuid = System.Guid.NewGuid().ToString("N");
			the_window_detail.width = 100;
			the_window_detail.window_group_id = "";
			the_window_detail.window_specifier = "new_window";
			the_window_detail.window_type = "data";
			the_window_detail.window_visibility = true;
			list_window_detail.Add(the_window_detail);

			// Shows how one more object is added to the ObjectListView.
			objectListView_windows.AddObject(
				list_window_detail[list_window_detail.Count - 1]
			);

			// Select the new row.
			current_window_detail = list_window_detail[list_window_detail.Count - 1];
			objectListView_windows.SelectedObject = current_window_detail;

			// Shows how a row can be brought (scroll) to view.
			objectListView_windows.EnsureVisible(
				objectListView_windows.SelectedIndex
			);

			MessageBox.Show(
				"A blank record was added to the bottom of the list. To enter values for the new record, double-click the individual cells of the fields for the new record.",
				"MMHIS"
			);

			objectListView_windows.Focus();
			Console.WriteLine("825");
			save_updated_window_details();

			load_window();
			load_data_field();
			update_all_views(top_level_uuid);
		}

		private void button_preview_add_window_Click(object sender, EventArgs e)
		{
			add_window_detail_record();
		}

		private void button_preview_delete_window_Click(object sender, EventArgs e)
		{
			delete_window_detail_record();
		}

		// popup menu handling

		// this is for communication with other functions that handles objectlistview's cell editing
		private string aspect_being_edited = "";

		private void objectlistview_windows_choose_popup_menu_item(object sender, EventArgs e)
		{
			MenuItem mi_clicked = (MenuItem)sender;
			if (
				(current_window_detail[aspect_being_edited]).ToString()
				==
				mi_clicked.Text
			)
			{
				// no change
				return;
			}

			current_window_detail[aspect_being_edited] = mi_clicked.Text;
			
			objectListView_windows.
				RefreshObject(
					current_window_detail
				);

			apply_window_properties();
			if (aspect_being_edited.Contains("map_"))
			{
				apply_gmap_properties(null, true);
			}

			Console.WriteLine("866");
			save_updated_window_details();

			// window type changed, need to reload window
			if (aspect_being_edited == "window_type")
			{
				load_window();
				load_data_field();
				update_all_views(top_level_uuid);
			}
		}

		private void objectListView_windows_CellEditStarting(object sender, BrightIdeasSoftware.CellEditEventArgs e)
		{
			current_window_detail = (window_detail)e.RowObject;

			// No more in-place edit
			e.Cancel = true;
			aspect_being_edited = e.Column.AspectName;

			// for non-map windows, these fields cannot be edited.
			if (aspect_being_edited.Contains("map_"))
			{
				if (current_window_detail.window_type != "map")
				{
					return;
				}
			}

			// Custom processing for specific columns
			switch (aspect_being_edited)
			{
				case "window_type":
					{
						ContextMenu m;
						m = new ContextMenu();
						m.MenuItems.Add("data", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("image", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("map", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("data_table", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.Show(this, PointToClient(Cursor.Position));
					}
					break;
				case "border_style":
					{
						ContextMenu m;
						m = new ContextMenu();
						m.MenuItems.Add("None", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("FixedSingle", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("Fixed3D", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("FixedDialog", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("Sizable", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("FixedToolWindow", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("SizableToolWindow", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.Show(this, PointToClient(Cursor.Position));
					}
					break;
				case "opacity":
					{
						opacity_adjustment_dialog oad = new opacity_adjustment_dialog();
						oad.the_parent = this;
						oad.trackBar_opacity.Value = current_window_detail.opacity;
						oad.Location = Cursor.Position;
						oad.ShowDialog();

						// the above dialog handles saving the changes to the database
					}
					break;
				case "background_color":
				case "map_line_color_0":
				case "map_line_color_1":
					{
						colorDialog1.Color = utility.get_color_from_string(current_window_detail[aspect_being_edited]);
						if (colorDialog1.ShowDialog() == DialogResult.OK)
						{
							// Change the value of the column in the
							// current row.
							current_window_detail[aspect_being_edited] = utility.get_string_from_color(colorDialog1.Color);
							// Update the display for the current row.
							objectListView_windows.RefreshObject(current_window_detail);

							Console.WriteLine("937");
							save_updated_window_details();
						}
					}
					apply_window_properties();
					if (aspect_being_edited.Contains("map_"))
					{
						apply_gmap_properties(null, true);
						//show_whole_section_on_window();
					}
					break;
				case "map_provider":
					{
						ContextMenu m;
						m = new ContextMenu();
//						m.MenuItems.Add("ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("ArcGIS_Imagery_World_2D_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("ArcGIS_ShadedRelief_World_2D_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("ArcGIS_StreetMap_World_2D_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("ArcGIS_Topo_US_2D_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("ArcGIS_World_Physical_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("ArcGIS_World_Shaded_Relief_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("ArcGIS_World_Street_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("ArcGIS_World_Terrain_Base_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("ArcGIS_World_Topo_MapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("BingHybridMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("BingMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("BingSatelliteMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("CloudMadeMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("EmptyProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("GoogleHybridMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("GoogleMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("GoogleSatelliteMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.MenuItems.Add("GoogleTerrainMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("MapBenderWMSProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("NearHybridMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("NearMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("NearSatelliteMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OpenCycleLandscapeMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OpenCycleMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OpenCycleTransportMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OpenStreet4UMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OpenStreetMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OpenStreetMapQuestHybridProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OpenStreetMapQuestProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OpenStreetMapQuestSatteliteProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OviHybridMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OviMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OviSatelliteMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("OviTerrainMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("WikiMapiaMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("YahooHybridMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("YahooMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("YahooSatelliteMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("YandexHybridMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("YandexMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
//						m.MenuItems.Add("YandexSatelliteMapProvider", new EventHandler(objectlistview_windows_choose_popup_menu_item));
						m.Show(this, PointToClient(Cursor.Position));
					}
					break;
				default:
					// For the rest of columns that don't need
					// special in-place edit processing
					// we use the default in-place edit.
					e.Cancel = false;
					// And the saving part will be handled in
					// the CellEditingFinishing event handler
					break;
			}
		}

		private void objectListView_windows_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e)
		{
			aspect_being_edited = e.Column.AspectName;
			object new_value = e.NewValue;
			object old_value = current_window_detail[aspect_being_edited];

			// For the_opacity field, the number should be between 0 and 255
			if (aspect_being_edited == "opacity")
			{
				if (Convert.ToInt32(new_value) < 0)
				{
					new_value = 0;
				}
				else if (Convert.ToInt32(new_value) > 100)
				{
					new_value = 100;
				}
			}
			else if ((aspect_being_edited == "width") || (aspect_being_edited == "height"))
			{
				if (Convert.ToInt32(new_value) < 10)
				{
					new_value = 10;
				}
				else if (Convert.ToInt32(new_value) > 8000)
				{
					new_value = 8000;
				}
			}
			else if (aspect_being_edited == "map_line_width_0")
			{
				if (Convert.ToInt32(new_value) < 0)
				{
					new_value = 0;
				}
				else if (Convert.ToInt32(new_value) > 30)
				{
					new_value = 30;
				}
			}
			else if (aspect_being_edited == "map_line_width_1")
			{
				if (Convert.ToInt32(new_value) < 0)
				{
					new_value = 0;
				}
				else if (Convert.ToInt32(new_value) > 30)
				{
					new_value = 30;
				}
			}
			else if (aspect_being_edited == "map_initial_center_latitude")
			{
				if (Convert.ToDouble(new_value) < -90)
				{
					new_value = -90.0;
				}
				else if (Convert.ToDouble(new_value) > 90)
				{
					new_value = 90.0;
				}
			}
			else if (aspect_being_edited == "map_initial_center_longitude")
			{
				if (Convert.ToDouble(new_value) < -180)
				{
					new_value = -180.0;
				}
				else if (Convert.ToDouble(new_value) > 180)
				{
					new_value = 180.0;
				}
			}
			else if (aspect_being_edited == "map_initial_zoom_level")
			{
				if (Convert.ToDouble(new_value) < 1)
				{
					new_value = 1.0;
				}
				else if (Convert.ToDouble(new_value) > 22)
				{
					new_value = 22.0;
				}
			}
			else if (aspect_being_edited == "map_zoom_minimum")
			{
				if (Convert.ToInt32(new_value) > current_window_detail.map_zoom_maximum)
				{
					new_value = current_window_detail.map_zoom_maximum;
				}
				if (Convert.ToInt32(new_value) < 0)
				{
					new_value = 0;
				}
				else if (Convert.ToInt32(new_value) > 22)
				{
					new_value = 22;
				}
			}
			else if (aspect_being_edited == "map_zoom_maximum")
			{
				if (Convert.ToInt32(new_value) < current_window_detail.map_zoom_minimum)
				{
					new_value = current_window_detail.map_zoom_minimum;
				}
				if (Convert.ToInt32(new_value) < 0)
				{
					new_value = 0;
				}
				else if (Convert.ToInt32(new_value) > 22)
				{
					new_value = 22;
				}
			}

			if (new_value != old_value)
			{
				current_window_detail[aspect_being_edited] = new_value;
				apply_window_properties();
				if (aspect_being_edited.Contains("map_"))
				{
					apply_gmap_properties(null, true);
				}
				if (aspect_being_edited == "window_specifier")
				{
					load_data_field();
					show_whole_section_on_window();
					update_all_views(top_level_uuid);
				}

				Console.WriteLine("1122");
				save_updated_window_details();
			}
		}
#endregion

		// public function because opacity adjustment dialog will call this
		public void apply_window_properties(window_detail the_window_detail = null)
		{
			if (the_window_detail == null)
			{
				the_window_detail = current_window_detail;
			}
			if (the_window_detail == null)
			{
				return;
			}

			try
			{
				the_window_detail.the_window.Text = the_window_detail.caption_text;
				the_window_detail.the_window.ControlBox = the_window_detail.control_box;
				the_window_detail.the_window.MaximizeBox = the_window_detail.maximize_box;
				the_window_detail.the_window.MinimizeBox = the_window_detail.minimize_box;
				the_window_detail.the_window.Opacity = the_window_detail.opacity / 100.0;
				the_window_detail.the_window.TopMost = the_window_detail.top_most;
				the_window_detail.the_window.ShowIcon = the_window_detail.show_icon;
				the_window_detail.the_window.ShowInTaskbar = the_window_detail.show_in_taskbar;
				switch (the_window_detail.border_style)
				{
					case "None":
						the_window_detail.the_window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
						break;
					case "FixedSingle":
						the_window_detail.the_window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
						break;
					case "Fixed3D":
						the_window_detail.the_window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
						break;
					case "FixedDialog":
						the_window_detail.the_window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
						break;
					case "Sizable":
						the_window_detail.the_window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
						break;
					case "FixedToolWindow":
						the_window_detail.the_window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
						break;
					case "SizableToolWindow":
						the_window_detail.the_window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
						break;
					default:
						the_window_detail.the_window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
						break;
				}
				the_window_detail.the_window.Left = the_window_detail.left;
				the_window_detail.the_window.Top = the_window_detail.top;
				the_window_detail.the_window.Width = the_window_detail.width;
				the_window_detail.the_window.Height = the_window_detail.height;
				the_window_detail.the_window.BackColor = utility.get_color_from_string(the_window_detail.background_color);
				the_window_detail.the_window.Visible = the_window_detail.window_visibility;
				the_window_detail.the_window.Refresh();
			}
			catch { }
		}

		public void apply_gmap_properties(window_detail the_window_detail = null, bool exclude_initial_properties = false)
		{
			if (the_window_detail == null)
			{
				the_window_detail = current_window_detail;
			}
			if (the_window_detail == null)
			{
				return;
			}
			if (the_window_detail.window_type == "map")
			{
				((Map)(the_window_detail.the_window)).set_map_provider(the_window_detail.map_provider);
				((Map)(the_window_detail.the_window)).map_line_color_0 = utility.get_color_from_string(the_window_detail.map_line_color_0);
				((Map)(the_window_detail.the_window)).map_line_width_0 = the_window_detail.map_line_width_0;
				((Map)(the_window_detail.the_window)).map_line_color_1 = utility.get_color_from_string(the_window_detail.map_line_color_1);
				((Map)(the_window_detail.the_window)).map_line_width_1 = the_window_detail.map_line_width_1;
				if (!exclude_initial_properties)
				{
					((Map)(the_window_detail.the_window)).set_map_center(
						new PointLatLng(the_window_detail.map_initial_center_latitude, the_window_detail.map_initial_center_longitude)
					);
				}
				((Map)(the_window_detail.the_window)).set_max_zoom(the_window_detail.map_zoom_maximum);
				((Map)(the_window_detail.the_window)).set_min_zoom(the_window_detail.map_zoom_minimum);
				if (!exclude_initial_properties)
				{
					((Map)(the_window_detail.the_window)).set_zoom(the_window_detail.map_initial_zoom_level);
				}
				((Map)(the_window_detail.the_window)).map_center_current_location = the_window_detail.map_center_current_location;
				for (int i = 0; i < ((Map)(the_window_detail.the_window)).gMapControl_map.Overlays[0].Routes.Count; i++)
				{
					((Map)(the_window_detail.the_window)).gMapControl_map.Overlays[0].Routes[i].Stroke = new Pen(
						((Map)(the_window_detail.the_window)).map_line_color_0,
						((Map)(the_window_detail.the_window)).map_line_width_0
					);
				}
				for (int i = 0; i < ((Map)(the_window_detail.the_window)).gMapControl_map.Overlays[1].Routes.Count; i++)
				{
					((Map)(the_window_detail.the_window)).gMapControl_map.Overlays[1].Routes[i].Stroke = new Pen(
						((Map)(the_window_detail.the_window)).map_line_color_1,
						((Map)(the_window_detail.the_window)).map_line_width_1
					);
				}
				the_window_detail.the_window.Refresh();
			}
		}

#region objectlistview with format handling
		// dictionary from field_specifier to data_field_detail
		public Dictionary<string, data_field_detail> dictionary_data_field_detail = new Dictionary<string, data_field_detail>();
		public List<data_field_detail> list_data_field_detail = new List<data_field_detail>();
		public data_field_detail current_data_field_detail = null;

		private void initialize_objectListView_data_field()
		{
			objectListView_data_field_detail.FullRowSelect = true;
			objectListView_data_field_detail.HideSelection = false;
			objectListView_data_field_detail.EmptyListMsg = "Field List is Empty";
			olvColumn_field_caption_color.RendererDelegate= delegate(
				EventArgs e,
				Graphics g, // the graphics of the cell
				Rectangle r, // the area of the cell
				Object rowObject // the whole row
			)
			{
				return utility.draw_color(g, r, ((data_field_detail)rowObject).caption_color);
			};
			olvColumn_field_value_color.RendererDelegate = delegate(
				EventArgs e,
				Graphics g, // the graphics of the cell
				Rectangle r, // the area of the cell
				Object rowObject // the whole row
			)
			{
				return utility.draw_color(g, r, ((data_field_detail)rowObject).value_color);
			};
			olvColumn_field_unit_color.RendererDelegate = delegate(
				EventArgs e,
				Graphics g, // the graphics of the cell
				Rectangle r, // the area of the cell
				Object rowObject // the whole row
			)
			{
				return utility.draw_color(g, r, ((data_field_detail)rowObject).unit_color);
			};
			olvColumn_field_caption_font_bold.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.caption_font_bold = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				apply_label_properties("caption", current_data_field_detail);
				save_updated_data_field_detail();
			};
			olvColumn_field_caption_font_italic.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.caption_font_italic = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				apply_label_properties("caption", current_data_field_detail);
				save_updated_data_field_detail();
			};
			olvColumn_field_caption_font_underline.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.caption_font_underline = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				apply_label_properties("caption", current_data_field_detail);
				save_updated_data_field_detail();
			};
			olvColumn_field_value_evaluation_required.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.value_evaluation_required = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				save_updated_data_field_detail();
			};
			olvColumn_field_value_font_bold.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.value_font_bold = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				apply_label_properties("value", current_data_field_detail);
				save_updated_data_field_detail();
			};
			olvColumn_field_value_font_italic.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.value_font_italic = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				apply_label_properties("value", current_data_field_detail);
				save_updated_data_field_detail();
			};
			olvColumn_field_value_font_underline.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.value_font_underline = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				apply_label_properties("value", current_data_field_detail);
				save_updated_data_field_detail();
			};
			olvColumn_field_unit_font_bold.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.unit_font_bold = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				apply_label_properties("unit", current_data_field_detail);
				save_updated_data_field_detail();
			};
			olvColumn_field_unit_font_italic.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.unit_font_italic = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				apply_label_properties("unit", current_data_field_detail);
				save_updated_data_field_detail();
			};
			olvColumn_field_unit_font_underline.AspectPutter = delegate(
				object rowObject,
				object newValue
			)
			{
				current_data_field_detail = (data_field_detail)rowObject;
				current_data_field_detail.unit_font_underline = Convert.ToBoolean(newValue);
				objectListView_data_field_detail.SelectedObject = current_data_field_detail;
				apply_label_properties("unit", current_data_field_detail);
				save_updated_data_field_detail();
			};
		}

		private void load_data_field()
		{
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();
			tdpg.initialize_generic_members("sqlite");
			tdpg.construct_connection_string(
				"",
				"",
				"",
				setting.window_and_data_field_user_setting_pathname
			);
			tdpg.open_conn_cmd(0);
			tdpg.cmd[0].CommandText = "select * from mmhis_data_field order by display_order";
			tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();

			#region destroy all the labels recorded in dictionary_data_field_detail
			foreach (data_field_detail the_data_field_detail in dictionary_data_field_detail.Values)
			{
				try
				{
					the_data_field_detail.the_caption_label.Dispose();
				}
				catch { }
				try
				{
					the_data_field_detail.the_unit_label.Dispose();
				}
				catch { }
				try
				{
					the_data_field_detail.the_value_label.Dispose();
				}
				catch { }
			}
			dictionary_data_field_detail.Clear();
			list_data_field_detail.Clear();
			#endregion

			foreach (window_detail the_window_detail in list_window_detail)
			{
				try
				{
					the_window_detail.the_window.Controls.Clear();
					the_window_detail.the_window.SuspendLayout();
				}
				catch { }
			}

			// Since we have cleared all the controls in the image window and map window,
			// we need to create the picture box in each image window
			// and the gMap control in each map window
			// and the objectlistview control in each data_table window
			foreach (window_detail the_window_detail in list_window_detail)
			{
				if (the_window_detail.window_type == "image")
				{
					// the pictureBox has to be created programmatically because it was
					// destroyed in steps above
					try
					{
						((video_frame)(the_window_detail.the_window)).pictureBox_frame = new System.Windows.Forms.PictureBox();
						((video_frame)(the_window_detail.the_window)).pictureBox_frame.Dock = System.Windows.Forms.DockStyle.Fill;
						((video_frame)(the_window_detail.the_window)).pictureBox_frame.Location = new System.Drawing.Point(0, 0);
						((video_frame)(the_window_detail.the_window)).pictureBox_frame.Name = "pictureBox_frame";
						((video_frame)(the_window_detail.the_window)).pictureBox_frame.Size = new System.Drawing.Size(100, 100);
						((video_frame)(the_window_detail.the_window)).pictureBox_frame.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
						((video_frame)(the_window_detail.the_window)).pictureBox_frame.TabIndex = 0;
						((video_frame)(the_window_detail.the_window)).pictureBox_frame.TabStop = true;
						((video_frame)(the_window_detail.the_window)).Controls.Add(((video_frame)(the_window_detail.the_window)).pictureBox_frame);
					}
					catch { }
				}
				if (the_window_detail.window_type == "map")
				{
					// the gMap control has to be created programmatically because it was
					// destroyed in steps above
					try
					{
						((Map)(the_window_detail.the_window)).gMapControl_map = new GMap.NET.WindowsForms.GMapControl();
						((Map)(the_window_detail.the_window)).initialize_gMap_control();
						((Map)(the_window_detail.the_window)).gMapControl_map.MouseUp +=
							new System.Windows.Forms.MouseEventHandler(
								((Map)(the_window_detail.the_window)).gMapControl_map_MouseUp
							);
						((Map)(the_window_detail.the_window)).Controls.Add(((Map)(the_window_detail.the_window)).gMapControl_map);
					}
					catch { }
					apply_gmap_properties(the_window_detail);
				}
				if (the_window_detail.window_type == "data_table")
				{
					try
					{
						data_table_view dtv = (data_table_view)(the_window_detail.the_window);
						dtv.objectListView_data_table1 = new BrightIdeasSoftware.ObjectListView();

						((System.ComponentModel.ISupportInitialize)(dtv.objectListView_data_table1)).BeginInit();
						dtv.SuspendLayout();
						dtv.objectListView_data_table1.Location = new System.Drawing.Point(0, 0);
						dtv.objectListView_data_table1.Size = dtv.ClientSize;
						dtv.objectListView_data_table1.Name = "objectListView_data_table";
						dtv.objectListView_data_table1.TabIndex = 0;
						dtv.objectListView_data_table1.UseCompatibleStateImageBehavior = false;
						dtv.objectListView_data_table1.FullRowSelect = true;
						dtv.objectListView_data_table1.AlternateRowBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
						dtv.objectListView_data_table1.UseAlternatingBackColors = true;
						dtv.objectListView_data_table1.HideSelection = false;
						dtv.objectListView_data_table1.GridLines = true;
						dtv.objectListView_data_table1.UseFilterIndicator = false;
						dtv.objectListView_data_table1.UseFiltering = false;
						dtv.objectListView_data_table1.View = System.Windows.Forms.View.Details;
						dtv.objectListView_data_table1.SelectedIndexChanged += new System.EventHandler(dtv.objectListView_data_table1_SelectedIndexChanged);
						dtv.objectListView_data_table1.CellClick += new System.EventHandler<BrightIdeasSoftware.CellClickEventArgs>(dtv.objectListView_data_table1_CellClick);
						((System.ComponentModel.ISupportInitialize)(dtv.objectListView_data_table1)).EndInit();

						dtv.Controls.Add(dtv.objectListView_data_table1);

						dtv.ResumeLayout();
					}
					catch { }
				}
			}

			int display_order = 0;
			// always regulate display_order to simplify later processing

			while (tdpg.reader[0].Read())
			{
				current_data_field_detail = new data_field_detail("", 0);
				current_data_field_detail.uuid = utility.get_string(tdpg.reader[0]["uuid"]);
				current_data_field_detail.user_id = utility.get_string(tdpg.reader[0]["user_id"], "unknown_user");
				current_data_field_detail.format_group_id = utility.get_string(tdpg.reader[0]["format_group_id"], "unknown_format_group_id");
				current_data_field_detail.field_specifier = utility.get_string(tdpg.reader[0]["field_specifier"], "unknown_field_specifier");
				current_data_field_detail.field_specifier_array = split_field_specifier(current_data_field_detail.field_specifier);
				current_data_field_detail.window_specifier = utility.get_string(tdpg.reader[0]["window_specifier"], "unknown_window_specifier");
				current_data_field_detail.caption_text = utility.get_string(tdpg.reader[0]["caption_text"], "");
				current_data_field_detail.caption_position_x = utility.get_int(tdpg.reader[0]["caption_position_x"], 0);
				current_data_field_detail.caption_position_y = utility.get_int(tdpg.reader[0]["caption_position_y"], 0);
				current_data_field_detail.caption_font_name = utility.get_string(tdpg.reader[0]["caption_font_name"], "Times New Roman");
				current_data_field_detail.caption_font_size = utility.get_double(tdpg.reader[0]["caption_font_size"], 20.0);
				current_data_field_detail.caption_font_bold = utility.get_boolean_from_int(tdpg.reader[0]["caption_font_bold"]);
				current_data_field_detail.caption_font_italic = utility.get_boolean_from_int(tdpg.reader[0]["caption_font_italic"]);
				current_data_field_detail.caption_font_underline = utility.get_boolean_from_int(tdpg.reader[0]["caption_font_underline"]);
				current_data_field_detail.caption_color = utility.get_string(tdpg.reader[0]["caption_color"], "000000");
				current_data_field_detail.value_text = utility.get_string(tdpg.reader[0]["value_text"], "");
				current_data_field_detail.value_conversion_factor = utility.get_double(tdpg.reader[0]["value_conversion_factor"], 1.0);
				current_data_field_detail.value_format = utility.get_string(tdpg.reader[0]["value_format"], "");
				current_data_field_detail.value_evaluation_required = utility.get_boolean_from_int(tdpg.reader[0]["value_evaluation_required"]);
				current_data_field_detail.value_position_x = utility.get_int(tdpg.reader[0]["value_position_x"], 0);
				current_data_field_detail.value_position_y = utility.get_int(tdpg.reader[0]["value_position_y"], 0);
				current_data_field_detail.value_font_name = utility.get_string(tdpg.reader[0]["value_font_name"], "Times New Roman");
				current_data_field_detail.value_font_size = utility.get_double(tdpg.reader[0]["value_font_size"], 20.0);
				current_data_field_detail.value_font_bold = utility.get_boolean_from_int(tdpg.reader[0]["value_font_bold"]);
				current_data_field_detail.value_font_italic = utility.get_boolean_from_int(tdpg.reader[0]["value_font_italic"]);
				current_data_field_detail.value_font_underline = utility.get_boolean_from_int(tdpg.reader[0]["value_font_underline"]);
				current_data_field_detail.value_color = utility.get_string(tdpg.reader[0]["value_color"], "000000");
				current_data_field_detail.unit_text = utility.get_string(tdpg.reader[0]["unit_text"], "");
				current_data_field_detail.unit_position_x = utility.get_int(tdpg.reader[0]["unit_position_x"], 0);
				current_data_field_detail.unit_position_y = utility.get_int(tdpg.reader[0]["unit_position_y"], 0);
				current_data_field_detail.unit_font_name = utility.get_string(tdpg.reader[0]["unit_font_name"], "Times New Roman");
				current_data_field_detail.unit_font_size = utility.get_double(tdpg.reader[0]["unit_font_size"], 20.0);
				current_data_field_detail.unit_font_bold = utility.get_boolean_from_int(tdpg.reader[0]["unit_font_bold"]);
				current_data_field_detail.unit_font_italic = utility.get_boolean_from_int(tdpg.reader[0]["unit_font_italic"]);
				current_data_field_detail.unit_font_underline = utility.get_boolean_from_int(tdpg.reader[0]["unit_font_underline"]);
				current_data_field_detail.unit_color = utility.get_string(tdpg.reader[0]["unit_color"], "000000");
				current_data_field_detail.display_order = display_order;
				current_data_field_detail.save_operation = "";
				current_data_field_detail.the_window_detail = null;
				current_data_field_detail.the_caption_label = null;
				current_data_field_detail.the_value_label = null;
				current_data_field_detail.the_unit_label = null;
				current_data_field_detail.updated = false;
				list_data_field_detail.Add(current_data_field_detail);

				display_order++;

				create_labels_on_window();
			}
			tdpg.reader[0].Close();
			tdpg.close_conn_cmd(0);

			objectListView_data_field_detail.SetObjects(list_data_field_detail);
			objectListView_data_field_detail.AddDecoration(
				new BrightIdeasSoftware.EditingCellBorderDecoration { UseLightbox = true }
			);

			current_data_field_detail = null;

			foreach (window_detail the_window_detail in list_window_detail)
			{
				if (
					the_window_detail.window_type == "data"
					||
					the_window_detail.window_type == "image"
					||
					the_window_detail.window_type == "map"
				)
				{
					try
					{
						the_window_detail.the_window.ResumeLayout(false);
						the_window_detail.the_window.PerformLayout();
					}
					catch { }
				}
			}

			show_whole_section_on_window();
		}

		private string[] split_field_specifier(string field_specifier)
		{
			if (field_specifier.Trim().Length > 0)
			{
				try
				{
					return current_data_field_detail.field_specifier.Split(
						new char[] { '|' },
						StringSplitOptions.RemoveEmptyEntries
					);
				}
				catch
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		private void apply_label_properties(string which_one, data_field_detail the_data_field_detail)
		{
			Label the_label = null;

			switch (which_one){
				case "caption":
					the_label = the_data_field_detail.the_caption_label;
					break;
				case "value":
					the_label = the_data_field_detail.the_value_label;
					break;
				case "unit":
					the_label = the_data_field_detail.the_unit_label;
					break;
			}
			if (the_label == null)
			{
				return;
			}

			the_label.Font = create_label_font(which_one, the_data_field_detail);
			the_label.ForeColor = utility.get_color_from_string(the_data_field_detail[which_one + "_color"]);
			the_label.Location = new System.Drawing.Point(
				Convert.ToInt32(the_data_field_detail[which_one + "_position_x"]),
				Convert.ToInt32(the_data_field_detail[which_one + "_position_y"])
			);
			if (the_data_field_detail[which_one + "_text"] != null)
			{
				the_label.Text = the_data_field_detail[which_one + "_text"].ToString().Trim();
			}
		}

		private System.Drawing.Font create_label_font(string which_one, data_field_detail the_data_field_detail)
		{
			try
			{
				return
					new System.Drawing.Font(
						the_data_field_detail[which_one + "_font_name"].ToString(),
						Convert.ToSingle(the_data_field_detail[which_one + "_font_size"]),
						(
							Convert.ToBoolean(the_data_field_detail[which_one + "_font_bold"]) ?
							System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular
						)
						|
						(
							Convert.ToBoolean(the_data_field_detail[which_one + "_font_italic"]) ?
							System.Drawing.FontStyle.Italic : System.Drawing.FontStyle.Regular
						)
						|
						(
							Convert.ToBoolean(the_data_field_detail[which_one + "_font_underline"]) ?
							System.Drawing.FontStyle.Underline : System.Drawing.FontStyle.Regular
						),
						System.Drawing.GraphicsUnit.Point,
						((byte)(0))
					);
			}
			catch
			{
				return
					new System.Drawing.Font(
						"Times New Roman",
						10,
						System.Drawing.FontStyle.Regular,
						System.Drawing.GraphicsUnit.Point,
						((byte)(0))
					);
			}
		}

		private void create_label(
			string which_one,
			data_field_detail the_data_field_detail,
			the_sub_window tsw
		)
		{
			// originally i do it perfectly
			// now i do it conveniently
			// i will create the label no matter it has text in it OR NOT!
			// that's why i comment the following out
			//if (
			//	(the_data_field_detail[which_one + "_text"] == null)
			//	||
			//	(the_data_field_detail[which_one + "_text"].ToString().Length == 0)
			//)
			//{
			//	// no need to create label
			//	the_data_field_detail["the_" + which_one + "_label"] = null;
			//	return;
			//}

			the_data_field_detail["the_" + which_one + "_label"] = new System.Windows.Forms.Label();
			((Label) the_data_field_detail["the_" + which_one + "_label"]).AutoSize = true;

			((Label)the_data_field_detail["the_" + which_one + "_label"]).Name = System.Guid.NewGuid().ToString("N");
			// The Name property is not used by my program, so I just put some random stuff here.

			((Label)the_data_field_detail["the_" + which_one + "_label"]).Size = new System.Drawing.Size(1, 1);
			((Label)the_data_field_detail["the_" + which_one + "_label"]).TabIndex = 0;
			((Label)the_data_field_detail["the_" + which_one + "_label"]).UseMnemonic = false;
			tsw.Controls.Add(((Label) the_data_field_detail["the_" + which_one + "_label"]));
		}

		private void checkBox_preview_data_show_data_field_details_CheckedChanged(object sender, EventArgs e)
		{
			this.olvColumn_field_field_specifier.IsVisible =
			this.olvColumn_field_window_specifier.IsVisible =
			this.olvColumn_field_caption_position_x.IsVisible =
			this.olvColumn_field_caption_position_y.IsVisible =
			this.olvColumn_field_caption_font_name.IsVisible =
			this.olvColumn_field_caption_font_size.IsVisible =
			this.olvColumn_field_caption_font_bold.IsVisible =
			this.olvColumn_field_caption_font_italic.IsVisible =
			this.olvColumn_field_caption_font_underline.IsVisible =
			this.olvColumn_field_caption_color.IsVisible =
			this.olvColumn_field_value_conversion_factor.IsVisible =
			this.olvColumn_field_value_format.IsVisible =
			this.olvColumn_field_value_evaluation_required.IsVisible =
			this.olvColumn_field_value_position_x.IsVisible =
			this.olvColumn_field_value_position_y.IsVisible =
			this.olvColumn_field_value_font_name.IsVisible =
			this.olvColumn_field_value_font_size.IsVisible =
			this.olvColumn_field_value_font_bold.IsVisible =
			this.olvColumn_field_value_font_italic.IsVisible =
			this.olvColumn_field_value_font_underline.IsVisible =
			this.olvColumn_field_value_color.IsVisible =
			this.olvColumn_field_unit_position_x.IsVisible =
			this.olvColumn_field_unit_position_y.IsVisible =
			this.olvColumn_field_unit_font_name.IsVisible =
			this.olvColumn_field_unit_font_size.IsVisible =
			this.olvColumn_field_unit_font_bold.IsVisible =
			this.olvColumn_field_unit_font_italic.IsVisible =
			this.olvColumn_field_unit_font_underline.IsVisible =
			this.olvColumn_field_unit_color.IsVisible =
				checkBox_preview_data_show_data_field_details.Checked;
			button_preview_add_data_field_record.Enabled =
				checkBox_preview_data_show_data_field_details.Checked;
			button_preview_delete_data_field_records.Enabled =
				(objectListView_data_field_detail.SelectedIndices.Count > 0)
				&&
				checkBox_preview_data_show_data_field_details.Checked;
			objectListView_data_field_detail.RebuildColumns();
		}

		private void save_updated_data_field_detail(data_field_detail the_data_field_detail = null)
		{
			if (the_data_field_detail == null)
			{
				the_data_field_detail = current_data_field_detail;
			}
			if (the_data_field_detail == null)
			{
				return;
			}

			if (
				the_data_field_detail != null
				&&
				(
					(the_data_field_detail.save_operation == null)
					||
					(the_data_field_detail.save_operation == "")
				)
			)
			{
				the_data_field_detail.save_operation = "update";
			}
			save_data_field_detail_changes(the_data_field_detail);
		}

		private bool save_data_field_detail_changes(data_field_detail the_data_field_detail = null)
		{
			if (the_data_field_detail == null)
			{
				the_data_field_detail = current_data_field_detail;
			}
			if (the_data_field_detail == null)
			{
				return true;
			}
			if (the_data_field_detail.save_operation == "")
			{
				return true;
			}
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();
			bool b_return = true;

			tdpg.initialize_generic_members("sqlite");
			tdpg.construct_connection_string(
				"",
				"",
				"",
				setting.window_and_data_field_user_setting_pathname
			);
			tdpg.open_conn_cmd(0);
			SQLiteCommand the_cmd = (SQLiteCommand)(tdpg.cmd[0]);
			if (the_data_field_detail.save_operation == "update")
			{
				the_cmd.CommandText =
					string.Format(
						"update {0} set" +
						" [{2}]=@{2}" +
						",[{3}]=@{3}" +
						",[{4}]=@{4}" +
						",[{5}]=@{5}" +
						",[{6}]=@{6}" +
						",[{7}]=@{7}" +
						",[{8}]=@{8}" +
						",[{9}]=@{9}" +
						",[{10}]=@{10}" +
						",[{11}]=@{11}" +
						",[{12}]=@{12}" +
						",[{13}]=@{13}" +
						",[{14}]=@{14}" +
						",[{15}]=@{15}" +
						",[{16}]=@{16}" +
						",[{17}]=@{17}" +
						",[{18}]=@{18}" +
						",[{19}]=@{19}" +
						",[{20}]=@{20}" +
						",[{21}]=@{21}" +
						",[{22}]=@{22}" +
						",[{23}]=@{23}" +
						",[{24}]=@{24}" +
						",[{25}]=@{25}" +
						",[{26}]=@{26}" +
						",[{27}]=@{27}" +
						",[{28}]=@{28}" +
						",[{29}]=@{29}" +
						",[{30}]=@{30}" +
						",[{31}]=@{31}" +
						",[{32}]=@{32}" +
						",[{33}]=@{33}" +
						",[{34}]=@{34}" +
						",[{35}]=@{35}" +
						",[{36}]=@{36}" +
						" where {1}=@{1}",
						"mmhis_data_field",
						"uuid",
						"user_id",
						"format_group_id",
						"field_specifier",
						"window_specifier",
						"caption_text",
						"caption_position_x",
						"caption_position_y",
						"caption_font_name",
						"caption_font_size",
						"caption_font_bold",
						"caption_font_italic",
						"caption_font_underline",
						"caption_color",
						"value_text",
						"value_conversion_factor",
						"value_format",
						"value_evaluation_required",
						"value_position_x",
						"value_position_y",
						"value_font_name",
						"value_font_size",
						"value_font_bold",
						"value_font_italic",
						"value_font_underline",
						"value_color",
						"unit_text",
						"unit_position_x",
						"unit_position_y",
						"unit_font_name",
						"unit_font_size",
						"unit_font_bold",
						"unit_font_italic",
						"unit_font_underline",
						"unit_color",
						"display_order"
					);
			}
			else if (the_data_field_detail.save_operation == "insert")
			{
				the_cmd.CommandText =
					string.Format(
						"insert into {0} " +
						"([{1}]" +
						",[{2}]" +
						",[{3}]" +
						",[{4}]" +
						",[{5}]" +
						",[{6}]" +
						",[{7}]" +
						",[{8}]" +
						",[{9}]" +
						",[{10}]" +
						",[{11}]" +
						",[{12}]" +
						",[{13}]" +
						",[{14}]" +
						",[{15}]" +
						",[{16}]" +
						",[{17}]" +
						",[{18}]" +
						",[{19}]" +
						",[{20}]" +
						",[{21}]" +
						",[{22}]" +
						",[{23}]" +
						",[{24}]" +
						",[{25}]" +
						",[{26}]" +
						",[{27}]" +
						",[{28}]" +
						",[{29}]" +
						",[{30}]" +
						",[{31}]" +
						",[{32}]" +
						",[{33}]" +
						",[{34}]" +
						",[{35}]" +
						",[{36}]" +
						")values" +
						"(@{1}" +
						",@{2}" +
						",@{3}" +
						",@{4}" +
						",@{5}" +
						",@{6}" +
						",@{7}" +
						",@{8}" +
						",@{9}" +
						",@{10}" +
						",@{11}" +
						",@{12}" +
						",@{13}" +
						",@{14}" +
						",@{15}" +
						",@{16}" +
						",@{17}" +
						",@{18}" +
						",@{19}" +
						",@{21}" +
						",@{22}" +
						",@{23}" +
						",@{24}" +
						",@{25}" +
						",@{26}" +
						",@{27}" +
						",@{28}" +
						",@{29}" +
						",@{30}" +
						",@{31}" +
						",@{32}" +
						",@{33}" +
						",@{34}" +
						",@{35}" +
						",@{36}" +
						")",
						"mmhis_data_field",
						"uuid",
						"user_id",
						"format_group_id",
						"field_specifier",
						"window_specifier",
						"caption_text",
						"caption_position_x",
						"caption_position_y",
						"caption_font_name",
						"caption_font_size",
						"caption_font_bold",
						"caption_font_italic",
						"caption_font_underline",
						"caption_color",
						"value_text",
						"value_conversion_factor",
						"value_format",
						"value_evaluation_required",
						"value_position_x",
						"value_position_y",
						"value_font_name",
						"value_font_size",
						"value_font_bold",
						"value_font_italic",
						"value_font_underline",
						"value_color",
						"unit_text",
						"unit_position_x",
						"unit_position_y",
						"unit_font_name",
						"unit_font_size",
						"unit_font_bold",
						"unit_font_italic",
						"unit_font_underline",
						"unit_color",
						"display_order"
					);
			}
			else
			{
				b_return = false;
			}

			the_cmd.Parameters.Clear();
			the_cmd.Parameters.Add("uuid", System.Data.DbType.String);
			the_cmd.Parameters.Add("user_id", System.Data.DbType.String);
			the_cmd.Parameters.Add("format_group_id", System.Data.DbType.String);
			the_cmd.Parameters.Add("field_specifier", System.Data.DbType.String);
			the_cmd.Parameters.Add("window_specifier", System.Data.DbType.String);
			the_cmd.Parameters.Add("caption_text", System.Data.DbType.String);
			the_cmd.Parameters.Add("caption_position_x", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("caption_position_y", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("caption_font_name", System.Data.DbType.String);
			the_cmd.Parameters.Add("caption_font_size", System.Data.DbType.Double);
			the_cmd.Parameters.Add("caption_font_bold", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("caption_font_italic", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("caption_font_underline", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("caption_color", System.Data.DbType.String);
			the_cmd.Parameters.Add("value_text", System.Data.DbType.String);
			the_cmd.Parameters.Add("value_conversion_factor", System.Data.DbType.Double);
			the_cmd.Parameters.Add("value_format", System.Data.DbType.String);
			the_cmd.Parameters.Add("value_evaluation_required", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("value_position_x", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("value_position_y", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("value_font_name", System.Data.DbType.String);
			the_cmd.Parameters.Add("value_font_size", System.Data.DbType.Double);
			the_cmd.Parameters.Add("value_font_bold", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("value_font_italic", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("value_font_underline", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("value_color", System.Data.DbType.String);
			the_cmd.Parameters.Add("unit_text", System.Data.DbType.String);
			the_cmd.Parameters.Add("unit_position_x", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("unit_position_y", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("unit_font_name", System.Data.DbType.String);
			the_cmd.Parameters.Add("unit_font_size", System.Data.DbType.Double);
			the_cmd.Parameters.Add("unit_font_bold", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("unit_font_italic", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("unit_font_underline", System.Data.DbType.Int32);
			the_cmd.Parameters.Add("unit_color", System.Data.DbType.String);
			the_cmd.Parameters.Add("display_order", System.Data.DbType.Int32);

			the_cmd.Parameters["uuid"].Value = the_data_field_detail.uuid;                    
			the_cmd.Parameters["user_id"].Value = the_data_field_detail.user_id;                 
			the_cmd.Parameters["format_group_id"].Value = the_data_field_detail.format_group_id;         
			the_cmd.Parameters["field_specifier"].Value = the_data_field_detail.field_specifier;         
			the_cmd.Parameters["window_specifier"].Value = the_data_field_detail.window_specifier;        
			the_cmd.Parameters["caption_text"].Value = the_data_field_detail.caption_text;            
			the_cmd.Parameters["caption_position_x"].Value = the_data_field_detail.caption_position_x;      
			the_cmd.Parameters["caption_position_y"].Value = the_data_field_detail.caption_position_y;      
			the_cmd.Parameters["caption_font_name"].Value = the_data_field_detail.caption_font_name;       
			the_cmd.Parameters["caption_font_size"].Value = the_data_field_detail.caption_font_size;       
			the_cmd.Parameters["caption_font_bold"].Value = the_data_field_detail.caption_font_bold;       
			the_cmd.Parameters["caption_font_italic"].Value = the_data_field_detail.caption_font_italic;     
			the_cmd.Parameters["caption_font_underline"].Value = the_data_field_detail.caption_font_underline;  
			the_cmd.Parameters["caption_color"].Value = the_data_field_detail.caption_color;           
			the_cmd.Parameters["value_text"].Value = the_data_field_detail.value_text;
			the_cmd.Parameters["value_conversion_factor"].Value = the_data_field_detail.value_conversion_factor;
			the_cmd.Parameters["value_format"].Value = the_data_field_detail.value_format;
			the_cmd.Parameters["value_evaluation_required"].Value = the_data_field_detail.value_evaluation_required;
			the_cmd.Parameters["value_position_x"].Value = the_data_field_detail.value_position_x;        
			the_cmd.Parameters["value_position_y"].Value = the_data_field_detail.value_position_y;        
			the_cmd.Parameters["value_font_name"].Value = the_data_field_detail.value_font_name;         
			the_cmd.Parameters["value_font_size"].Value = the_data_field_detail.value_font_size;         
			the_cmd.Parameters["value_font_bold"].Value = the_data_field_detail.value_font_bold;         
			the_cmd.Parameters["value_font_italic"].Value = the_data_field_detail.value_font_italic;       
			the_cmd.Parameters["value_font_underline"].Value = the_data_field_detail.value_font_underline;    
			the_cmd.Parameters["value_color"].Value = the_data_field_detail.value_color;             
			the_cmd.Parameters["unit_text"].Value = the_data_field_detail.unit_text;               
			the_cmd.Parameters["unit_position_x"].Value = the_data_field_detail.unit_position_x;         
			the_cmd.Parameters["unit_position_y"].Value = the_data_field_detail.unit_position_y;         
			the_cmd.Parameters["unit_font_name"].Value = the_data_field_detail.unit_font_name;          
			the_cmd.Parameters["unit_font_size"].Value = the_data_field_detail.unit_font_size;          
			the_cmd.Parameters["unit_font_bold"].Value = the_data_field_detail.unit_font_bold;          
			the_cmd.Parameters["unit_font_italic"].Value = the_data_field_detail.unit_font_italic;        
			the_cmd.Parameters["unit_font_underline"].Value = the_data_field_detail.unit_font_underline;     
			the_cmd.Parameters["unit_color"].Value = the_data_field_detail.unit_color;              
			the_cmd.Parameters["display_order"].Value = the_data_field_detail.display_order;            

			try
			{
				the_cmd.ExecuteNonQuery();
				the_data_field_detail.save_operation = "";
				b_return = true;
			}
			catch (Exception exc)
			{
				MessageBox.Show(current_function_name() + "(): 1800   " + exc.Message);
				b_return = false;
			}
			finally
			{
				tdpg.close_conn_cmd(0);
			}
			return b_return;
		}

		private void save_all_format_details()
		{
			for (int i = 0; i < list_data_field_detail.Count; i++)
			{
				save_data_field_detail_changes(list_data_field_detail[i]);
			}
		}

		private void objectListView_data_field_detail_SelectionChanged(object sender, EventArgs e)
		{
			object o = objectListView_data_field_detail.SelectedObject;

			if (o != null)
			{
				current_data_field_detail = (data_field_detail)o;
				if (current_data_field_detail.the_window_detail != null)
				{
					current_data_field_detail.the_window_detail.the_window.BringToFront();
				}
				button_preview_data_field_move_down.Enabled =
					(current_data_field_detail.display_order < objectListView_data_field_detail.Items.Count - 1);
				button_preview_data_field_move_up.Enabled =
					(current_data_field_detail.display_order > 0);
			}
			else
			{
				current_data_field_detail = null;
				button_preview_data_field_move_down.Enabled =
					button_preview_data_field_move_up.Enabled = false;
			}

			button_preview_delete_data_field_records.Enabled =
				(objectListView_data_field_detail.SelectedIndices.Count > 0)
				&&
				checkBox_preview_data_show_data_field_details.Checked;

			checkBox_preview_data_move_caption.Enabled =
				checkBox_preview_data_move_value.Enabled =
				checkBox_preview_data_move_unit.Enabled =
				(objectListView_data_field_detail.SelectedIndices.Count == 1);
		}

		private void delete_data_field_detail_record()
		{
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();
			tdpg.initialize_generic_members("sqlite");
			tdpg.construct_connection_string(
				"",
				"",
				"",
				setting.window_and_data_field_user_setting_pathname
			);
			tdpg.open_conn_cmd(0);
			SQLiteCommand the_cmd = (SQLiteCommand)(tdpg.cmd[0]);

			the_cmd.CommandText =
				"delete from [mmhis_data_field]" + data_field_delete_uuid_where_clause();
			try
			{
				the_cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				MessageBox.Show(current_function_name() + "(): 1847     " + exc.Message);
			}
			finally
			{
				tdpg.close_conn_cmd(0);
			}

			load_data_field();
			update_all_views(top_level_uuid);
		}

		private string data_field_delete_uuid_where_clause()
		{
			StringBuilder where_clause =
				 new StringBuilder("where (1=0)");
			// To simply the following processing

			for (int i = objectListView_data_field_detail.Items.Count - 1; i >= 0; i--)
			{
				if (objectListView_data_field_detail.Items[i].Selected)
				{
					where_clause.Append(
						"or([uuid]='" +
						// Here shows how the object for a certain row
						// can be retrieved using an index.
						((data_field_detail)(objectListView_data_field_detail.GetModelObject(i))).uuid +
						"')"
					);
				}
			}
			return where_clause.ToString();
		}

		public void add_data_field_detail_record()
		{
			list_data_field_detail.Add(
				new data_field_detail(
					System.Guid.NewGuid().ToString("N"),
					list_data_field_detail.Count,
					"insert"
				)
			);

			// Shows how one more object is added to the ObjectListView.
			objectListView_data_field_detail.AddObject(
				list_data_field_detail[list_data_field_detail.Count - 1]
			);

			// Select the new row.
			current_data_field_detail = list_data_field_detail[list_data_field_detail.Count - 1];
			objectListView_data_field_detail.SelectedObject = current_data_field_detail;

			create_labels_on_window();

			// Shows how a row can be brought (scroll) to view.
			objectListView_data_field_detail.EnsureVisible(
				objectListView_data_field_detail.SelectedIndex
			);

			MessageBox.Show(
				"A blank record was added to the bottom of the list. To enter values for the new record, double-click the individual cells of the fields for the new record.",
				"MMHIS"
			);

			objectListView_data_field_detail.Focus();
			save_updated_data_field_detail();
		}

		private void button_preview_add_data_field_record_Click(object sender, EventArgs e)
		{
			add_data_field_detail_record();
		}

		private void button_preview_delete_data_field_records_Click(object sender, EventArgs e)
		{
			delete_data_field_detail_record();
		}

		private void objectListView_data_field_detail_CellEditStarting(object sender, BrightIdeasSoftware.CellEditEventArgs e)
		{
			bool need_to_save = false;
			bool need_to_update_display = false;

			current_data_field_detail = (data_field_detail)e.RowObject;
			e.Cancel = true;
			aspect_being_edited = e.Column.AspectName;
			switch (aspect_being_edited)
			{
				case "caption_color":
					{
						colorDialog1.Color = utility.get_color_from_string(current_data_field_detail.caption_color);
						if (colorDialog1.ShowDialog() == DialogResult.OK)
						{
							current_data_field_detail.caption_color = utility.get_string_from_color(colorDialog1.Color);
							apply_label_properties("caption", current_data_field_detail);
							need_to_save = true;
						}
					}
					break;
				case "value_color":
					{
						colorDialog1.Color = utility.get_color_from_string(current_data_field_detail.value_color);
						if (colorDialog1.ShowDialog() == DialogResult.OK)
						{
							current_data_field_detail.value_color = utility.get_string_from_color(colorDialog1.Color);
							apply_label_properties("value", current_data_field_detail);
							need_to_save = true;
						}
					}
					break;
				case "unit_color":
					{
						colorDialog1.Color = utility.get_color_from_string(current_data_field_detail.unit_color);
						if (colorDialog1.ShowDialog() == DialogResult.OK)
						{
							current_data_field_detail.unit_color = utility.get_string_from_color(colorDialog1.Color);
							apply_label_properties("unit", current_data_field_detail);
							need_to_save = true;
						}
					}
					break;
				case "caption_font_name":
					{
						fontDialog1.Font = create_label_font("caption", current_data_field_detail);
						if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
						{
							current_data_field_detail.caption_font_name = fontDialog1.Font.Name;
							current_data_field_detail.caption_font_bold = fontDialog1.Font.Bold;
							current_data_field_detail.caption_font_italic = fontDialog1.Font.Italic;
							current_data_field_detail.caption_font_underline = fontDialog1.Font.Underline;
							//current_data_field_detail.caption_color = utility.get_string_from_color(fontDialog1.Color);
							current_data_field_detail.caption_font_size = fontDialog1.Font.SizeInPoints;
							apply_label_properties("caption", current_data_field_detail);
							need_to_save = true;
						}
					}
					break;
				case "value_font_name":
					{
						fontDialog1.Font = create_label_font("value", current_data_field_detail);
						if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
						{
							current_data_field_detail.value_font_name = fontDialog1.Font.Name;
							current_data_field_detail.value_font_bold = fontDialog1.Font.Bold;
							current_data_field_detail.value_font_italic = fontDialog1.Font.Italic;
							current_data_field_detail.value_font_underline = fontDialog1.Font.Underline;
							//current_data_field_detail.value_color = utility.get_string_from_color(fontDialog1.Color);
							current_data_field_detail.value_font_size = fontDialog1.Font.SizeInPoints;
							apply_label_properties("value", current_data_field_detail);
							need_to_save = true;
						}
					}
					break;
				case "unit_font_name":
					{
						fontDialog1.Font = create_label_font("unit", current_data_field_detail);
						if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
						{
							current_data_field_detail.unit_font_name = fontDialog1.Font.Name;
							current_data_field_detail.unit_font_bold = fontDialog1.Font.Bold;
							current_data_field_detail.unit_font_italic = fontDialog1.Font.Italic;
							current_data_field_detail.unit_font_underline = fontDialog1.Font.Underline;
							//current_data_field_detail.unit_color = utility.get_string_from_color(fontDialog1.Color);
							current_data_field_detail.unit_font_size = fontDialog1.Font.SizeInPoints;
							apply_label_properties("unit", current_data_field_detail);
							need_to_save = true;
						}
					}
					break;
				case "field_specifier":
					{
						choose_field_specifier cfs = new choose_field_specifier();
						cfs.which_specifier = "field";
						if (cfs.ShowDialog() == System.Windows.Forms.DialogResult.OK)
						{
							string old_value = current_data_field_detail.field_specifier;
							if (old_value != cfs.selected_specifier)
							{
								current_data_field_detail.field_specifier = cfs.selected_specifier;
								current_data_field_detail.field_specifier_array = split_field_specifier(current_data_field_detail.field_specifier);
								data_field_format the_data_field_format = null;
								if (
									setting.dictionary_data_field_format.TryGetValue(
										current_data_field_detail.field_specifier,
										out the_data_field_format
									)
								)
								{
									current_data_field_detail.caption_text = the_data_field_format.caption_suggestion;
									current_data_field_detail.unit_text = the_data_field_format.unit;
									current_data_field_detail.value_conversion_factor = the_data_field_format.conversion_factor;
									current_data_field_detail.value_format = the_data_field_format.value_format;
									current_data_field_detail.value_evaluation_required = false;
								}
								need_to_save = true;
								create_labels_on_window(old_value);
								need_to_update_display = true;
							}
						}
					}
					break;
				case "window_specifier":
					{
						choose_field_specifier cfs = new choose_field_specifier();
						cfs.which_specifier = "window";
						if (cfs.ShowDialog() == System.Windows.Forms.DialogResult.OK)
						{
							if (cfs.selected_specifier != current_data_field_detail.window_specifier)
							{
								current_data_field_detail.window_specifier = cfs.selected_specifier;
								need_to_save = true;
								create_labels_on_window(current_data_field_detail.field_specifier);
								need_to_update_display = true;
							}
						}
					}
					break;
				default:
					e.Cancel = false;
					break;
			}
			objectListView_data_field_detail.RefreshObject(current_data_field_detail);
			if (need_to_save)
			{
				save_updated_data_field_detail();
			}
			if (need_to_update_display)
			{
				update_all_views(top_level_uuid);
			}
		}

		private void objectListView_data_field_detail_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e)
		{
			aspect_being_edited = e.Column.AspectName;
			object new_value = e.NewValue;
			object old_value = current_data_field_detail[aspect_being_edited];
			if (new_value != old_value)
			{
				current_data_field_detail[aspect_being_edited] = new_value;
				save_updated_data_field_detail();
				update_all_views(top_level_uuid);
				apply_label_properties("caption", current_data_field_detail);
				apply_label_properties("value", current_data_field_detail);
				apply_label_properties("unit", current_data_field_detail);
			}
		}

		private void create_labels_on_window(
			string previous_field_specifier = "" // to be used to remove old label(s)
		)
		{
			if (previous_field_specifier.Length == 0)
			{
				// we don't have a previous field_specifier so no label to destroy.
				// to guarantee we don't remove any label
				// we use guid here
				previous_field_specifier = System.Guid.NewGuid().ToString("N");
			}

			// keep the item relating to this field (current_data_field_detail) in the list_data_field_detail

			// destroy all the labels relating to this field using the dictionary

			// at most 10 keys for a field
			for (int i = 0; i < 10; i++)
			{
				foreach (string dk in dictionary_data_field_detail.Keys)
				{
					if (
						dk.StartsWith(previous_field_specifier)
						&&
						(
							Char.IsDigit(dk.Substring(dk.Length - 1, 1)[0])
							&&
							(dk.Length == previous_field_specifier.Length + 1)
						)
					)
					{
						try
						{
							dictionary_data_field_detail[dk].the_caption_label.Dispose();
						}
						catch { }
						try
						{
							dictionary_data_field_detail[dk].the_unit_label.Dispose();
						}
						catch { }
						try
						{
							dictionary_data_field_detail[dk].the_value_label.Dispose();
						}
						catch { }
						dictionary_data_field_detail.Remove(dk);

						// note that because the current dictionary entry is removed
						// the enumeration in foreach will not be able to continue. we need 
						// to break out of this enumeration loop and start over
						break;
					}
				}
			}

			// create new labels in the new window
			{
				bool field_associated_window_exists = false;
				string the_window_specifier = current_data_field_detail.window_specifier;

				foreach (window_detail the_window_detail in list_window_detail)
				{
					// Find the window with the window_specifier.
					// Note that in the format specification the window specifier is allowed to be a
					// substring of the actual window specifier. for example, it can be "map_". in
					// this case this format specifier will put the field value on all windows containing
					// "map_" in their window specifiers, such as "map_street", "map_image".
					if (the_window_detail.window_specifier.Contains(the_window_specifier))
					{
						field_associated_window_exists = true;

						string key_prefix = current_data_field_detail.field_specifier;
						string key = "";

						// for the case the dictionary has just one item for the field_specifier
						// or even when it has more than one but for the first one
						// the following the_data_field_detail is not used. garbage collector will claim it back when it
						// goes out of scope
						data_field_detail the_data_field_detail = new data_field_detail("", 0);
						the_data_field_detail.uuid = current_data_field_detail.uuid;
						the_data_field_detail.user_id = current_data_field_detail.user_id;
						the_data_field_detail.format_group_id = current_data_field_detail.format_group_id;
						the_data_field_detail.field_specifier = current_data_field_detail.field_specifier;
						the_data_field_detail.window_specifier = current_data_field_detail.window_specifier;
						the_data_field_detail.caption_text = current_data_field_detail.caption_text;
						the_data_field_detail.caption_position_x = current_data_field_detail.caption_position_x;
						the_data_field_detail.caption_position_y = current_data_field_detail.caption_position_y;
						the_data_field_detail.caption_font_name = current_data_field_detail.caption_font_name;
						the_data_field_detail.caption_font_size = current_data_field_detail.caption_font_size;
						the_data_field_detail.caption_font_bold = current_data_field_detail.caption_font_bold;
						the_data_field_detail.caption_font_italic = current_data_field_detail.caption_font_italic;
						the_data_field_detail.caption_font_underline = current_data_field_detail.caption_font_underline;
						the_data_field_detail.caption_color = current_data_field_detail.caption_color;
						the_data_field_detail.value_text = current_data_field_detail.value_text;
						the_data_field_detail.value_conversion_factor = current_data_field_detail.value_conversion_factor;
						the_data_field_detail.value_format = current_data_field_detail.value_format;
						the_data_field_detail.value_position_x = current_data_field_detail.value_position_x;
						the_data_field_detail.value_position_y = current_data_field_detail.value_position_y;
						the_data_field_detail.value_font_name = current_data_field_detail.value_font_name;
						the_data_field_detail.value_font_size = current_data_field_detail.value_font_size;
						the_data_field_detail.value_font_bold = current_data_field_detail.value_font_bold;
						the_data_field_detail.value_font_italic = current_data_field_detail.value_font_italic;
						the_data_field_detail.value_font_underline = current_data_field_detail.value_font_underline;
						the_data_field_detail.value_color = current_data_field_detail.value_color;
						the_data_field_detail.unit_text = current_data_field_detail.unit_text;
						the_data_field_detail.unit_position_x = current_data_field_detail.unit_position_x;
						the_data_field_detail.unit_position_y = current_data_field_detail.unit_position_y;
						the_data_field_detail.unit_font_name = current_data_field_detail.unit_font_name;
						the_data_field_detail.unit_font_size = current_data_field_detail.unit_font_size;
						the_data_field_detail.unit_font_bold = current_data_field_detail.unit_font_bold;
						the_data_field_detail.unit_font_italic = current_data_field_detail.unit_font_italic;
						the_data_field_detail.unit_font_underline = current_data_field_detail.unit_font_underline;
						the_data_field_detail.unit_color = current_data_field_detail.unit_color;
						the_data_field_detail.save_operation = "";
						the_data_field_detail.the_window_detail = the_window_detail;
						the_data_field_detail.the_caption_label = null;
						the_data_field_detail.the_value_label = null;
						the_data_field_detail.the_unit_label = null;
						the_data_field_detail.updated = false;

						data_field_detail the_data_field_detail_use = null;

						// one field can target at most 10 windows
						for (int i = 0; i < 10; i++)
						{
							key = key_prefix + i.ToString();
							// here is a lazy way to handle this case.
							// if the key existed, we just try the next one.
							// this way we don't need to keep track of
							// the sequence number for the suffix.
							try
							{
								if (i == 0)
								{
									the_data_field_detail_use = current_data_field_detail;
								}
								else
								{
									the_data_field_detail_use = the_data_field_detail;
								}

								the_data_field_detail_use.the_window_detail = the_window_detail;
								// add new entries to the dictionary for the field
								dictionary_data_field_detail.Add(key, the_data_field_detail_use);
								break;
							}
							catch (Exception exc)
							{
								error_message = exc.Message;
							}
						}

						the_sub_window tsw = (the_sub_window)(the_window_detail.the_window);

						// the caption label is created and its text does not change
						create_label("caption", the_data_field_detail_use, tsw);

						// originally i do it perfectly
						// now i do it conveniently
						// i will create the label no matter it has text in it OR NOT!
						// that's why i comment the following out
						//// the value label is created
						//// here we create the label regardless whether its value is empty or not: forcefully
						//// put a value in.
						//if (the_data_field_detail_use.value_text == null || the_data_field_detail_use.value_text.Length == 0)
						//{
						//	the_data_field_detail_use.value_text = " ";
						//}
						create_label("value", the_data_field_detail_use, tsw);

						// and its text changes later with the use of this dictionary
						if (the_data_field_detail_use.the_value_label != null)
						{
							// This way we have a uniform way of handling field for both
							// value fields and video frame fields. we use a label control
							// in video frame window and a text changed event handler
							// to load the image to the picture frame
							if (the_window_detail.window_type == "image")
							{
								((video_frame)(the_window_detail.the_window)).label_image_pathname = the_data_field_detail_use.the_value_label;
								((video_frame)(the_window_detail.the_window)).label_image_pathname.TextChanged +=
									new System.EventHandler(
										((video_frame)(the_window_detail.the_window)).label_image_pathname_TextChanged
									);
								// Note that if more than one field target the same "image" window, then whichever
								// appears last in the db replaces earlier ones. Earlier ones are lost
								// although they are created. The text change event for earlier ones will
								// not be handled. Therefore, do NOT put more than one field to target
								// one common "image" window.
							}
							if (the_window_detail.window_type == "map")
							{
								((Map)(the_window_detail.the_window)).label_current_gps_coordinates = the_data_field_detail_use.the_value_label;
								((Map)(the_window_detail.the_window)).label_current_gps_coordinates.TextChanged +=
									new System.EventHandler(
										((Map)(the_window_detail.the_window)).label_current_gps_coordinates_TextChanged
									);
								// Note that if more than one field target the same "map" window, then whichever
								// appears last in the db replaces earlier ones. Earlier ones are lost
								// although they are created. The text change event for earlier ones will
								// not be handled. Therefore, do NOT put more than one field to target
								// one common "map" window.
							}
							if (the_window_detail.window_type == "data_table")
							{
								((data_table_view)(the_window_detail.the_window)).label_logmeter = the_data_field_detail_use.the_value_label;
								((data_table_view)(the_window_detail.the_window)).label_logmeter.TextChanged +=
									new System.EventHandler(
										((data_table_view)(the_window_detail.the_window)).label_logmeter_TextChanged
									);
							}
						}

						// the unit label is created and its text does not change
						create_label("unit", the_data_field_detail_use, tsw);

						apply_label_properties("caption", the_data_field_detail_use);
						apply_label_properties("value", the_data_field_detail_use);
						apply_label_properties("unit", the_data_field_detail_use);
					}
				}

				if (!field_associated_window_exists)
				{
					// update the list_data_field_detail with the new label values
					current_data_field_detail.the_caption_label = null;
					current_data_field_detail.the_value_label = null;
					current_data_field_detail.the_unit_label = null;
				}
			}
		}

		private void button_preview_data_field_move_up_Click(object sender, EventArgs e)
		{
			int display_order = current_data_field_detail.display_order;
			data_field_detail the_data_field_detail_temp = list_data_field_detail[display_order];
			list_data_field_detail[display_order] =
				list_data_field_detail[display_order - 1];
			list_data_field_detail[display_order - 1] = the_data_field_detail_temp;
			list_data_field_detail[display_order].display_order = display_order;
			list_data_field_detail[display_order - 1].display_order = display_order - 1;
			list_data_field_detail[display_order].save_operation = "update";
			list_data_field_detail[display_order - 1].save_operation = "update";
			objectListView_data_field_detail.SetObjects(list_data_field_detail);
			objectListView_data_field_detail.EnsureVisible(display_order - 1);
			objectListView_data_field_detail.SelectedIndex = display_order - 1;
			save_all_format_details();
		}

		private void button_preview_data_field_move_down_Click(object sender, EventArgs e)
		{
			int display_order = current_data_field_detail.display_order;
			data_field_detail the_data_field_detail_temp = list_data_field_detail[display_order];
			list_data_field_detail[display_order] =
				list_data_field_detail[display_order + 1];
			list_data_field_detail[display_order + 1] = the_data_field_detail_temp;
			list_data_field_detail[display_order].display_order = display_order;
			list_data_field_detail[display_order + 1].display_order = display_order + 1;
			list_data_field_detail[display_order].save_operation = "update";
			list_data_field_detail[display_order + 1].save_operation = "update";
			objectListView_data_field_detail.SetObjects(list_data_field_detail);
			objectListView_data_field_detail.EnsureVisible(display_order + 1);
			objectListView_data_field_detail.SelectedIndex = display_order + 1;
			save_all_format_details();
		}

#endregion

#region data display

		private void update_all_views(
			string top_level_uuid,
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			int frame_number = hScrollBar_play_position.Value;
			reset_value_field_updated();
			if (frame_number < 0)
			{
				return;
			}
			System.Data.DataRowCollection drc = null;
			try
			{
				drc = tdpg_in_mem.ds[0].Tables[0].Rows;
			}
			catch
			{
				return;
			}

			// ld = "link_down"
			string ld = System.Guid.NewGuid().ToString("N");
			// This value will guarantee that no data will be found.

			string logmeter = "";

			if (frame_number < drc.Count)
			{
				ld = utility.get_string(drc[frame_number]["ld"]);
				logmeter = utility.get_string(drc[frame_number]["logmeter_0"]);
			}

			tdpg_in_mem.create_query_snap_shot(
				1,
				setting.play_mmhis_data_all_script
				.Replace("[[level_1_to_2_uuid]]", ld)
				.Replace("[[level_0_to_1_uuid]]", top_level_uuid)
				.Replace("[[logmeter]]", logmeter),
				"fen",
				0
			);

			utility.empty_value_compound_component_text_from_dictionary_data_field_format();

			// This loop goes through all records but only shows the values that
			// have an entry in the dictionary_data_field_detail. At the same time
			// the value_compound_component_text is stored in dictionary_data_field_format
			// for ALL result records (i.e., data fields).
			foreach (DataRow dr in tdpg_in_mem.ds[1].Tables[0].Rows)
			{
				show_value(
					utility.get_string(dr["field_specifier"]),
					utility.get_string(dr["field_value"])
				);
			}

			// Special treatment for data_table_view windows
			// The field_specifier for data_table_view windows to get
			// the logmeter is not a result record from the query of
			// the mmhis_fen table. Hence we need to loop through
			// all the values in the dictionary.
			// Important: for this to work correctly, the field_specifier
			// used for "data_table" windows is used to transfer logmeter
			// to these windows, therefore, the field_specifier should NOT
			// be from "mmhis_fen" table, for "mmhis_fen" table does not
			// contain "logmeter" values. It only contains "logmile" values
			// which is not in meters but in miles. If a field_specifier from
			// mmhis_fen is used, the value passed to the data_table_view
			// window will not be what it expects, and therefore, will
			// select the wrong row(s). Then when the 'logmeter' is processed
			// the right row(s) will be selected, causing the list to
			// scroll to the wrong row, then right row, and so on. It will slow down
			// the display and everything else, and make a mess.
			// When running the program (for end users) and specify the field_specifier
			// for data targeting "data_table_view" window, do not select
			// a value from the "field_specifier" list. Instead, directly enter
			// a value that is not in the list in the box below.
			string value_format_processed = "";
			data_field_format the_data_field_format = null;
			foreach (data_field_detail the_data_field_detail in dictionary_data_field_detail.Values)
			{
				if (the_data_field_detail.the_window_detail.window_type == "data_table")
				{
					// Send the logmeter over to the "data_table" window so
					// it can do it's own magic of selecting the rows relating to that
					// logmeter
					show_value(
						the_data_field_detail.field_specifier,
						logmeter
					);
				}

				// compound data item processing
				if (the_data_field_detail.value_evaluation_required)
				{
					value_format_processed = the_data_field_detail.value_format;
					// process value_format
					foreach (string field_specifier in the_data_field_detail.field_specifier_array)
					{
						if (
							setting.dictionary_data_field_format.TryGetValue(
								field_specifier,
								out the_data_field_format
							)
						)
						{
							value_format_processed = value_format_processed.Replace(
								"[[" + field_specifier + "]]",
								the_data_field_format.value_compound_component_text
							);
						}
					}

					// evaluate value_format_processed to get the final value_text
					// and show the value
					show_value(
						the_data_field_detail.field_specifier,
						utility.evaluate_value_format(value_format_processed)
					);
				}
			}

			clear_absent_value_fields();

			// update the objectlistview in one function call
			objectListView_data_field_detail.RefreshObjects(list_data_field_detail);
		}

		// find the formatted text for this field using entry from
		// dictionary_data_field_format, with unit
		private string get_value(string field_specifier, string value_from_fen)
		{
			data_field_format the_data_field_format = null;
			double numeric_value_original = -1000000001.0; // an impossible value
			double numeric_value =
				numeric_value_original = utility.get_double(value_from_fen, -1000000001.0);
			if (
				setting.dictionary_data_field_format.TryGetValue(
					field_specifier,
					out the_data_field_format
				)
			)
			{
				if (numeric_value_original > -1000000000.0)
				{
					numeric_value = numeric_value_original * the_data_field_format.conversion_factor;
				}

				if (
					(the_data_field_format.value_format != null)
					&&
					(the_data_field_format.value_format.Length > 0)
				)
				{
					try
					{
						if (numeric_value_original > -1000000000.0)
						{
							value_from_fen =
								string.Format(the_data_field_format.value_format, numeric_value)
								+
								" "
								+
								the_data_field_format.unit;
						}
						else
						{
							value_from_fen =
								string.Format(the_data_field_format.value_format, value_from_fen)
								+
								" "
								+
								the_data_field_format.unit;
						}
					}
					catch (Exception exc)
					{
						// if the code reaches here value_from_fen will still
						// have the original value passed into this function
						Console.WriteLine(the_data_field_format.value_format + " - " + exc.Message);
					}
				}
			}

			return value_from_fen;
		}

		// This function is called from different places.
		// When looping through the queried out mmhis_fen records, all are original data fields.
		// In this case the data_field_detail for the fields supposed to be displayed on windows
		// should have no value_evaluation_required set.
		// Then the program loops through the dictionary_data_field_detail and do special processing
		// for "data_table_view" windows. This should also has no value_evaluation_required set.
		// Then in the same loop the program process the case for value_evaluation_required fields.
		// This is for compound data fields, which contain (normally) more than one original field.
		// When this function is call at this time, the previous calls to this function already prepared
		// the "value_text", which is already done the "evaluation". Therefore, this call, when it sees
		// value_evaluation_required is true, simply shows the value_text without any further format processing.
		private void show_value(string field_specifier, string value_text)
		{
			data_field_detail the_data_field_detail = null;
			double numeric_value_original = -1000000001.0; // an impossible value
			double numeric_value =
				numeric_value_original = utility.get_double(value_text, -1000000001.0);
			string value_compound_component_text = "";

			#region handles the field that actually gets shown on the windows -- a subset of all the records (i.e., data fields) queried out
			// The field_specifier is always the real field_specifier with a sequence number as the suffix of it.
			// One field can target at most ten windows. At least one window is targeted, so suffix 0 will always exist
			// unless the value label was not created for some reason (e.g., the window does not exist).
			// If a field_specifier never existed in the dictionary, this loop will break out immediately
			for (int i = 0; i < 10; i++)
			{
				if (
					dictionary_data_field_detail.TryGetValue(
						field_specifier + i.ToString(),
						out the_data_field_detail
					)
				)
				{
					if (the_data_field_detail.value_evaluation_required)
					{
						// do nothing
					}
					else
					{
						if (numeric_value_original > -1000000000.0)
						{
							numeric_value = numeric_value_original * the_data_field_detail.value_conversion_factor;
						}

						if (
							(the_data_field_detail.value_format != null)
							&&
							(the_data_field_detail.value_format.Length > 0)
						)
						{
							try
							{
								if (numeric_value_original > -1000000000.0)
								{
									value_text = string.Format(the_data_field_detail.value_format, numeric_value);
								}
								else
								{
									value_text = string.Format(the_data_field_detail.value_format, value_text);
								}
							}
							catch (Exception exc)
							{
								// if the code reaches here value_text will still have the original
								// value passed into this function.
								Console.WriteLine(the_data_field_detail.value_format + " - " + exc.Message);
							}
						}
						else
						{
							// do nothing so value_text remains the original value
						}
					}

					if (the_data_field_detail.the_value_label != null)
					{
						the_data_field_detail.the_value_label.Text = value_text;
					}
					the_data_field_detail.value_text = value_text;
					the_data_field_detail.updated = true;

					if (i == 0)
					{
						// Update the objectlistview, too.
						// Note that this is commented out because we want to refresh the whole list by just one function call
						//objectListView_data_field_detail.RefreshObject(the_data_field_detail);

						value_compound_component_text = value_text;
					}
				}
				else
				{
					break;
				}
			}
			#endregion

			#region each field queried out -- no matter shown on window or not -- is checked here and stored in setting.dictionary_data_field_format
			// store value_compound_component_text in setting.dictionary_data_field_format
			// for use later in compound data item calculation
			utility.get_data_field_value_compound_component_text(
				field_specifier,
				value_text,
				value_compound_component_text
			);
			#endregion
		}

		private void clear_absent_value_fields()
		{
			foreach (data_field_detail the_data_field_detail in dictionary_data_field_detail.Values)
			{
				if (!the_data_field_detail.updated)
				{
					if (the_data_field_detail.the_value_label != null)
					{
						the_data_field_detail.the_value_label.Text = "";
					}
					the_data_field_detail.value_text = "";
					the_data_field_detail.updated = true;

					// Update the objectlistview, too.
					// Note that this is commented out because we want to refresh the whole list by just one function call
					//objectListView_data_field_detail.RefreshObject(the_data_field_detail);
				}
			}
		}

		private void reset_value_field_updated()
		{
			foreach (data_field_detail the_data_field_detail in dictionary_data_field_detail.Values)
			{
				the_data_field_detail.updated = false;
			}
		}
#endregion

		private void button_specify_mmhis_db_Click(object sender, EventArgs e)
		{
			select_db_source sds = new select_db_source();
			if (sds.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				setting.current_mmhis_db = sds.mmhis_db;
				mmhis_db_source = sds.mmhis_db_source;
				label_current_mmhis_db_source.Text = mmhis_db_source + " " + sds.mmhis_db;
				if (mmhis_db_source == "Microsoft Access" && sds.mmhis_db.Length > 0)
				{
					load_mmhis_access_db();
				}
				else if (mmhis_db_source == "SQL Server")
				{
					load_mmhis_db_from_sql_server();
				}
				else if (mmhis_db_source == "SQLite")
				{
					load_mmhis_db_from_sqlite();
				}
			}
		}

		private void load_mmhis_db_from_sql_server()
		{
			//todo
		}

		private void load_mmhis_db_from_sqlite()
		{
			//todo
		}

		private void load_mmhis_access_db()
		{
			show_status("Loading...");
			populate_route();
		}

		private void populate_route()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			comboBox_route.Items.Clear();
			comboBox_route.Text = "";
			comboBox_route.Refresh();
			if (setting.current_mmhis_db.Trim().Length == 0)
			{
				return;
			}

			try
			{
				tdpa.open_conn_cmd(0, setting.current_mmhis_db);
				tdpa.cmd[0].CommandText =
					"select space(6-len(RouteNumber)) + RouteNumber + ' ' + iif(Direction is null or Direction = '', '', Direction) from Arkansas_Master group by space(6-len(RouteNumber)) + RouteNumber + ' ' + iif(Direction is null or Direction = '', '', Direction) order by 1";
				tdpa.reader[0] = tdpa.cmd[0].ExecuteReader();
				while (tdpa.reader[0].Read())
				{
					comboBox_route.Items.Add(utility.get_string(tdpa.reader[0][0]).TrimStart());
				}
				tdpa.reader[0].Close();
				if (comboBox_route.Items.Count > 0)
				{
					comboBox_route.SelectedIndex = 0;
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 2480    " + error_message);
			}
			finally
			{
				tdpa.close_conn_cmd(0);
			}
		}

		//query Arkansas_Master table using route number and direction
		private void populate_year()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			comboBox_year.Items.Clear();
			comboBox_year.Text = "";
			comboBox_year.Refresh();
			if (the_route.Length == 0)
			{
				return;
			}

			try
			{
				tdpa.close_conn_cmd(0);
				if (!tdpa.open_conn_cmd(0, setting.current_mmhis_db))
				{
					populate_section();
					return;
				}
				tdpa.cmd[0].CommandText = "select [Year] from Arkansas_Master where RouteNumber = '" + the_route + "'" + " and (Direction = '" + mmhis_direction + "' or Direction is null) order by 1 desc";
				tdpa.reader[0] = tdpa.cmd[0].ExecuteReader();
				while (tdpa.reader[0].Read())
				{
					comboBox_year.Items.Add(utility.get_string(tdpa.reader[0][0]));
				}
				tdpa.reader[0].Close();
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 2518    " + error_message);
			}
			finally
			{
				tdpa.close_conn_cmd(0);
			}

			if (comboBox_year.Items.Count > 0)
			{
				comboBox_year.SelectedIndex = 0;
			}
			else
			{
				populate_section();
			}
		}

		// query the sub db's roadway table to find the sections
		private void populate_section()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			comboBox_section.Items.Clear();
			comboBox_section.Text = "";
			comboBox_section.Refresh();
			if (the_route.Length == 0 || the_year.Length == 0)
			{
				return;
			}

			find_sub_db_and_jpi_path();

			if (textBox_access_target_db_pathname.Text.Length == 0)
			{
				section_selection_change_handler();
				return;
			}

			//tdpa.reader[0].Close();
			//tdpa.close_conn_cmd(0);
			if (!tdpa.open_conn_cmd(0, textBox_access_target_db_pathname.Text))
			{
				section_selection_change_handler();
				return;
			}
			tdpa.cmd[0].CommandText =
				"select [Section] from [" + textBox_access_target_db_roadway_table.Text + "] group by [Section] order by space(10 - len(str(val([Section])))) + [Section]";
			try
			{
				tdpa.reader[0] = tdpa.cmd[0].ExecuteReader();
				while (tdpa.reader[0].Read())
				{
					comboBox_section.Items.Add(utility.get_string(tdpa.reader[0][0]));
				}
				tdpa.reader[0].Close();
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 2575     " + error_message);
			}
			finally
			{
				tdpa.close_conn_cmd(0);
			}

			if (comboBox_section.Items.Count > 0)
			{
				comboBox_section.SelectedIndex = 0;
			}
			else
			{
				section_selection_change_handler();
			}
		}

		private void section_selection_change_handler()
		{
			if (
				the_route.Length == 0 ||
				the_year.Length == 0 ||
				the_section.Length == 0
			)
			{
				set_combobox_color_warning(true);
				//return;
			}

			find_the_county();
			find_section_notes();
			update_road_id();
			Console.WriteLine("Calling find_jpi_filename() from section_selection_change_handler()");
			find_jpi_filename();
		}

		private void find_section_notes()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			try
			{
				the_note = "";
				tdpa.close_conn_cmd(0);
				tdpa.open_conn_cmd(0, setting.current_mmhis_db);
				tdpa.cmd[0].CommandText =
					"SELECT [Remarks] FROM [SectionNotes] WHERE [Route] = '" + the_route + "'" +
					" AND ([Direction] is null OR [Direction] = '" + mmhis_direction +
					"') AND " + utility.section_clause("Section", the_section) +
					" AND [Year] = '" + the_year + "'";
				tdpa.reader[0] = tdpa.cmd[0].ExecuteReader();
				while (tdpa.reader[0].Read())
				{
					the_note = utility.get_string(tdpa.reader[0][0]).Trim();
				}
				tdpa.reader[0].Close();
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 2633         " + error_message);
			}
			finally
			{
				tdpa.close_conn_cmd(0);
			}
		}

		private void set_combobox_color_warning(bool db_not_ready)
		{
			if (db_not_ready)
			{
				comboBox_route.ForeColor =
				comboBox_year.ForeColor =
				comboBox_section.ForeColor = color_combobox_forecolor_warning;
				comboBox_route.BackColor =
				comboBox_year.BackColor =
				comboBox_section.BackColor = color_combobox_backcolor_warning;
			}
			else
			{
				comboBox_route.ForeColor =
					comboBox_year.ForeColor =
					comboBox_section.ForeColor = color_combobox_forecolor_default;
				comboBox_route.BackColor =
					comboBox_year.BackColor =
					comboBox_section.BackColor = color_combobox_backcolor_default;

			}
		}

		private void find_the_county()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();
			the_county = "";
			if (
				the_route.Length == 0 ||
				the_section.Length == 0
			)
			{
				return;
			}
			if (textBox_access_target_db_pathname.Text.Trim().Length > 0)
			{
				show_status("Querying " + textBox_access_target_db_roadway_table.Text + " from " + textBox_access_target_db_pathname.Text + " for the_county...");
				try
				{
					if (tdpa.open_conn_cmd(0, textBox_access_target_db_pathname.Text))
					{
						tdpa.cmd[0].CommandText =
							"SELECT TOP 1 [County] FROM [" + textBox_access_target_db_roadway_table.Text +
							"] WHERE " + utility.section_clause("Section", the_section);
						tdpa.reader[0] = tdpa.cmd[0].ExecuteReader();
						if (tdpa.reader[0].Read())
						{
							the_county = utility.get_string(tdpa.reader[0][0]);
						}
						tdpa.reader[0].Close();
						tdpa.close_conn_cmd(0);
						if (the_county.Length > 0)
						{
							set_combobox_color_warning(false);
							show_status("County from MMHIS DB: " + the_county);
							return;
						}
					}
				}
				catch (Exception exc)
				{
					error_message = exc.Message;
					Console.WriteLine(current_function_name() + "(): 2701      " + error_message);
				}
			}

			//county not found from mmhis db
			//query the real roadlog db to get the county
			tdpg.initialize_generic_members("sql_server");
			tdpg.construct_connection_string(
				textBox_roadlog_db_server.Text,
				"", "",
				textBox_roadlog_db.Text
			);
			if (tdpg.open_conn_cmd(0))
			{
				// try to query the old roadlog
				tdpg.cmd[0].CommandText =
					"select top 1 [County] from [" +
					textBox_roadlog_db.Text + "].[" +
					textBox_roadlog_schema.Text + "].[" +
					textBox_roadlog_table_view.Text +
					"] where [Route] = '" +
					utility.five_digit_route(the_route) +
					"' and [Sectn]='" +
					utility.three_character_section(the_section) + "'";
				try
				{
					tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
					if (tdpg.reader[0].Read())
					{
						the_county = utility.get_string(tdpg.reader[0][0]);
					}
				}
				catch (Exception exc)
				{
					error_message = exc.Message;
					Console.WriteLine(current_function_name() + "(): 2736           " + error_message);
					// try to query the new arnold-ready roadlog
					tdpg.cmd[0].CommandText =
						"select top 1 [AH_County] from [" +
						textBox_roadlog_db.Text + "].[" +
						textBox_roadlog_schema.Text + "].[" +
						textBox_roadlog_table_view.Text +
						"] where [AH_Route] = '" +
						utility.short_route(the_route) +
						"' and [AH_Section] = '" +
						the_section + "'";
					try
					{
						tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
						if (tdpg.reader[0].Read())
						{
							the_county = utility.get_string(tdpg.reader[0][0]);
						}
					}
					catch (Exception exc1)
					{
						error_message = exc1.Message;
						Console.WriteLine(current_function_name() + "(): 2758        " + error_message);

					}
				}
				tdpg.reader[0].Close();
				tdpg.close_conn_cmd(0);
			}
			set_combobox_color_warning(true);
			if (the_county.Length > 0)
			{
				show_status("County from roadway inventory: " + the_county);
			}
			else
			{
				show_status("County not known.");
			}
		}

		private void update_road_id()
		{
			road_id_with_no_direction = the_county + "x" + the_route + "x" + the_section;
			find_road_id_direction_from_mmhis_direction();
		}

		private void find_road_id_direction_from_mmhis_direction()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			show_status("Querying " + comboBox_direction_conversion_table.Text + " from " + textBox_direction_conversion_db.Text + " for road_id_direction...");
			road_id_direction = "?";
			if (the_section.Length > 0 && textBox_direction_conversion_db.Text.Length > 0)
			{
				try
				{
					tdpa.open_conn_cmd(1, textBox_direction_conversion_db.Text);
					tdpa.cmd[1].CommandText =
						"SELECT [road_id_direction] FROM [" + comboBox_direction_conversion_table.Text +
						"] WHERE [county] = '" + the_county +
						"' AND [route] = '" + the_route +
						"' AND " + utility.section_clause("Section", the_section) +
						" AND [mmhis_direction] = '" + mmhis_direction + "'";
					tdpa.reader[1] = tdpa.cmd[1].ExecuteReader();
					if (tdpa.reader[1].Read())
					{
						road_id_direction = utility.get_string(tdpa.reader[1][0]).ToUpper();
					}
					tdpa.reader[1].Close();
				}
				catch (Exception exc)
				{
					error_message = exc.Message;
					Console.WriteLine(current_function_name() + "(): 2805           " + error_message);
				}
				finally
				{
					tdpa.close_conn_cmd(1);
				}
			}
			textBox_road_id.Text = road_id_with_no_direction + "x" + road_id_direction;
			if (the_county.Length == 0)
			{
				show_status("The specified section was not found.");
			}
			else if (road_id_direction == "?")
			{
				show_status("The road ID direction was not specified.");
			}
			else
			{
				show_status("The road ID direction was determined.");
			}
		}

		private void find_mmhis_direction_from_road_id_direction()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			show_status("Querying " + comboBox_direction_conversion_table.Text + " from " + textBox_direction_conversion_db.Text + " for mmhis_direction...");
			mmhis_direction = "?";
			if (the_section.Length > 0 && textBox_direction_conversion_db.Text.Length > 0)
			{
				try
				{
					tdpa.open_conn_cmd(1, textBox_direction_conversion_db.Text);
					tdpa.cmd[1].CommandText =
						"SELECT [mmhis_direction] FROM [" + comboBox_direction_conversion_table.Text +
						"] WHERE [county] = '" + the_county +
						"' AND [route] = '" + the_route +
						"' AND " + utility.section_clause("Section", the_section) +
						" AND [road_id_direction] = '" + road_id_direction + "'";
					tdpa.reader[1] = tdpa.cmd[1].ExecuteReader();
					if (tdpa.reader[1].Read())
					{
						mmhis_direction = utility.get_string(tdpa.reader[1][0]).ToUpper();
					}
					tdpa.reader[1].Close();
				}
				catch (Exception exc)
				{
					error_message = exc.Message;
					Console.WriteLine(current_function_name() + "(): 2850          " + error_message);
				}
				finally
				{
					tdpa.close_conn_cmd(1);
				}
			}
		}

		private void find_jpi_filename()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			show_status("Querying " + textBox_access_target_db_frame_index_table.Text + " from " + textBox_access_target_db_pathname.Text + " for JPI file name...");
			Console.WriteLine("Querying " + textBox_access_target_db_frame_index_table.Text + " from " + textBox_access_target_db_pathname.Text + " for JPI file name...");
			try
			{
				if (tdpa.open_conn_cmd(0, textBox_access_target_db_pathname.Text))
				{
					tdpa.cmd[0].CommandText =
						"SELECT TOP 1 [VideoFileName] FROM [" + textBox_access_target_db_frame_index_table.Text +
						"] WHERE " + utility.section_clause("Section", the_section);
					Console.WriteLine(tdpa.cmd[0].CommandText);
					tdpa.reader[0] = tdpa.cmd[0].ExecuteReader();
					if (tdpa.reader[0].Read())
					{
						textBox_access_jpi_file.Text = utility.get_string(tdpa.reader[0][0]);
						show_status("JPI filename retrieved from MMHIS DB.");
					}
					else
					{
						construct_jpi_filename();
					}

					tdpa.reader[0].Close();
					tdpa.close_conn_cmd(0);
					return;
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 2889        " + error_message);
			}
			construct_jpi_filename();
		}

		private void construct_jpi_filename()
		{
			textBox_access_jpi_file.Text = the_route + "s" + the_section + road_id_direction + the_year + ".jpi";
			show_status("JPI filename constructed.");
		}

		// query Arkansas_Master to find the sub db and jpi_path
		private void find_sub_db_and_jpi_path()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			show_status("Querying Arkansas_Master from " + setting.current_mmhis_db + " for DatabaseFileName and VideoFilePath...");
			if (!tdpa.open_conn_cmd(0, setting.current_mmhis_db))
			{
				show_status("Invalid MMHIS main DB encountered.");
				return;
			}
			tdpa.cmd[0].CommandText =
				"select [DatabaseFileName], [VideoFilePath] from Arkansas_Master where RouteNumber = '" +
				the_route + "' and [Year] = '" + the_year + "'";
			if (mmhis_direction.Length > 0)
			{
				tdpa.cmd[0].CommandText += " and Direction = '" + mmhis_direction + "'";
			}
			tdpa.cmd[0].CommandText += " order by 1";
			tdpa.reader[0] = tdpa.cmd[0].ExecuteReader();
			textBox_access_target_db_pathname.Text = "";
			if (tdpa.reader[0].Read())
			{
				textBox_access_target_db_pathname.Text = Path.Combine(
					Path.GetDirectoryName(setting.current_mmhis_db),
					utility.get_string(tdpa.reader[0][0])
				);
				textBox_access_jpi_path.Text = utility.get_string(tdpa.reader[0][1]);
				set_combobox_color_warning(false);
				show_status("Route / section exist in current MMHIS DB");
			}
			else
			{
				// this route direction year is not authored yet
				set_combobox_color_warning(true);

				find_the_county();

				update_road_id();
				construct_jpi_filename();
				show_status("Route / section do not exist in current MMHIS DB");
			}
		}

		private void position_cursor_when_necessary(Control c)
		{
			if (!c.ClientRectangle.Contains(c.PointToClient(Control.MousePosition)))
			{
				Cursor.Position = c.PointToScreen(new Point(c.Width - 3, 10));
			}
		}

		private void expand_location_selection()
		{
			panel_location_selection.Height = tabControl1.Bottom - panel_location_selection.Top - 2;
			panel_location_selection.BringToFront();
		}

		private void collapse_location_selection()
		{
			panel_location_selection.Height = button_specify_mmhis_db.Height - 2;
		}

		private void comboBox_route_Enter(object sender, EventArgs e)
		{
			position_cursor_when_necessary(comboBox_route);
			expand_location_selection();
		}

		private void comboBox_route_Leave(object sender, EventArgs e)
		{
			collapse_location_selection();
		}

		private void comboBox_route_MouseDown(object sender, MouseEventArgs e)
		{
			expand_location_selection();
		}

		private void comboBox_route_TextChanged(object sender, EventArgs e)
		{
			the_route = comboBox_route.Text;
			extract_mmhis_direction_from_route_input();
			populate_year();
		}

		private void extract_mmhis_direction_from_route_input()
		{
			try
			{
				string[] route_split = the_route.Trim().Split(new char[] { ' ' });
				if (route_split.Length == 2)
				{
					mmhis_direction = route_split[1].Trim().Substring(0, 1);
					the_route = route_split[0].Trim();
				}
				else
				{
					the_route = the_route.Trim();
					mmhis_direction = "";
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 3003        " + error_message);
				
				mmhis_direction = "";
			}
			construct_target_table_names();
		}

		private void construct_target_table_names()
		{
			target_table_name_prefix = "Arkansas_" + the_route + "_" + ((mmhis_direction.Length > 0) ? mmhis_direction + "_" : "");
			textBox_access_target_db_accident_table.Text = target_table_name_prefix + "Accident";
			textBox_access_target_db_bridge_table.Text = target_table_name_prefix + "Bridge";
			textBox_access_target_db_format_table.Text = target_table_name_prefix + "Format";
			textBox_access_target_db_frame_index_table.Text = target_table_name_prefix + "FrameIndex";
			textBox_access_target_db_job_record_table.Text = target_table_name_prefix + "JobRecord";
			textBox_access_target_db_pms_table.Text = target_table_name_prefix + "PMS";
			textBox_access_target_db_roadway_table.Text = target_table_name_prefix + "Roadway";
			textBox_access_target_db_turn_table.Text = target_table_name_prefix + "Turn";
		}

		private void comboBox_year_Enter(object sender, EventArgs e)
		{
			position_cursor_when_necessary(comboBox_year);
			expand_location_selection();
		}

		private void comboBox_year_Leave(object sender, EventArgs e)
		{
			collapse_location_selection();
		}

		private void comboBox_year_MouseDown(object sender, MouseEventArgs e)
		{
			expand_location_selection();
		}

		private void comboBox_year_TextChanged(object sender, EventArgs e)
		{
			the_year = comboBox_year.Text.Trim();
			populate_section();
		}

		private void comboBox_section_Enter(object sender, EventArgs e)
		{
			position_cursor_when_necessary(comboBox_section);
			expand_location_selection();
		}

		private void comboBox_section_Leave(object sender, EventArgs e)
		{
			collapse_location_selection();
		}

		private void comboBox_section_MouseDown(object sender, MouseEventArgs e)
		{
			expand_location_selection();
		}

		private void comboBox_section_TextChanged(object sender, EventArgs e)
		{
			the_section = comboBox_section.Text.Trim();
			section_selection_change_handler();
		}

		private void textBox_direction_conversion_db_TextChanged(object sender, EventArgs e)
		{
			populate_direction_conversion_table();
		}

		private void populate_direction_conversion_table()
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			tdpa.populate_access_table_list(comboBox_direction_conversion_table, textBox_direction_conversion_db.Text);
			if (comboBox_direction_conversion_table.Items.Count > 0)
			{
				comboBox_direction_conversion_table.SelectedIndex = 0;
			}
		}

		private void button_browse_direction_conversion_db_Click(object sender, EventArgs e)
		{
			openFileDialog1.Filter = "MS Access Files (*.mdb)|*.mdb|All Files (*.*)|*.*";
			openFileDialog1.AddExtension = true;
			openFileDialog1.DefaultExt = "mdb";
			openFileDialog1.CheckFileExists = false;
			openFileDialog1.FileName = "direction_conversion.mdb";
			openFileDialog1.Title = "Specify Direction Conversion DB";
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				textBox_direction_conversion_db.Text = openFileDialog1.FileName;
				create_direction_conversion_db_if_necessary();
			}
		}

		private void create_direction_conversion_db_if_necessary()
		{
			if (textBox_direction_conversion_db.Text.Trim().Length == 0)
			{
				return;
			}
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			bool valid = tdpa.open_conn_cmd(0, textBox_direction_conversion_db.Text);
			tdpa.close_conn_cmd(0);
			if (!valid)
			{
				if (
					MessageBox.Show(
						"The file specified is not a valid database. Create a valid one?",
						"MMHIS Data",
						MessageBoxButtons.YesNo
					)
					== System.Windows.Forms.DialogResult.Yes
				)
				{
					File.Delete(textBox_direction_conversion_db.Text);
					db_utility.utility.create_access_db(textBox_direction_conversion_db.Text, true);
				}
				else
				{
					textBox_direction_conversion_db.Text = "";
				}
			}
		}

		private void comboBox_direction_conversion_table_TextChanged(object sender, EventArgs e)
		{
			update_road_id();
		}

		private void textBox_road_id_Leave(object sender, EventArgs e)
		{
			//store_road_id_direction_and_mmhis_direction_conversion();
		}

		private void store_road_id_direction_and_mmhis_direction_conversion()
		{
			if (road_id_direction == "?" || mmhis_direction == "?" || mmhis_direction.Trim().Length == 0)
			{
				return;
			}

			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			if (tdpa.open_conn_cmd(1, textBox_direction_conversion_db.Text))
			{
				tdpa.cmd[1].CommandText =
					"DELETE FROM [" + comboBox_direction_conversion_table.Text.Trim() +
					"] WHERE [county] = '" + the_county +
					"' AND [route] = '" + the_route +
					"' AND " + utility.section_clause("Section", the_section) +
					" AND [mmhis_direction] = '" + mmhis_direction + "'";
				tdpa.cmd[1].ExecuteNonQuery();
				tdpa.cmd[1].CommandText =
					"INSERT INTO [" + comboBox_direction_conversion_table.Text.Trim() +
					"] ([county], [route], [section], [road_id_direction], [mmhis_direction]) VALUES ('" +
					the_county + "','" + the_route + "','" + the_section + "','" + road_id_direction + "','" + mmhis_direction + "')";
				tdpa.cmd[1].ExecuteNonQuery();
				tdpa.close_conn_cmd(1);
			}
			Console.WriteLine("Calling find_jpi_filename() from store_road_id_direction_and_mmhis_direction_conversion()");
			find_jpi_filename();
		}

		private void populate_new_runs(ListBox listbox_run)
		{
			Cursor.Current = Cursors.WaitCursor;
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();
			listbox_run.Items.Clear();
			show_selected_run(listbox_run); // to clear the existing line on the map

			try
			{
				if (textBox_pms_frame_index_source_data_db_server.Text.Length == 0)
				{
					tdpg.initialize_generic_members("sqlite");
				}
				else
				{
					tdpg.initialize_generic_members("sql server");
				}
				tdpg.construct_connection_string(
					textBox_pms_frame_index_source_data_db_server.Text,
					"", "",
					textBox_pms_frame_index_source_data_db.Text
				);
				if (tdpg.open_conn_cmd(0))
				{
					foreach (string s in utility.load_script(setting.script("retrieve_all_new_runs")))
					{
						string roadid_condition = "";
						string year_condition = "";

						if (textBox_pms_frame_index_source_data_db_server.Text.Length == 0)
						{
							tdpg.cmd[0].CommandText =
								s.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_table.Text + "]")
								.Replace("[[extract_date]]", "date(Min([CollectionTime]))");
							if (textBox_refine_list_year.Text.Trim().Length > 0)
							{
								year_condition = "HAVING strftime('%Y', Min([CollectionTime])) = '" + textBox_refine_list_year.Text + "'";
							}
						}
						else
						{
							tdpg.cmd[0].CommandText =
								s.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_db.Text + "].[" + textBox_pms_frame_index_source_data_db_schema.Text + "].[" + textBox_pms_frame_index_source_data_table.Text + "]")
								.Replace("[[extract_date]]", "Convert(date, Min([CollectionTime]))");
							if (textBox_refine_list_year.Text.Trim().Length > 0)
							{
								year_condition = "HAVING YEAR(MIN([CollectionTime])) = " + textBox_refine_list_year.Text.Trim();
							}
						}

						if (textBox_refine_list_county.Text.Trim().Length > 0)
						{
							roadid_condition += textBox_refine_list_county.Text.Trim() + "x";
						}
						else
						{
							roadid_condition += "%x";
						}
						if (textBox_refine_list_route.Text.Trim().Length > 0)
						{
							roadid_condition += textBox_refine_list_route.Text.Trim() + "x";
						}
						else
						{
							roadid_condition += "%x";
						}
						if (textBox_refine_list_section.Text.Trim().Length > 0)
						{
							roadid_condition += textBox_refine_list_section.Text.Trim() + "x";
						}
						else
						{
							roadid_condition += "%x";
						}
						if (textBox_refine_list_direction.Text.Trim().Length > 0)
						{
							roadid_condition += textBox_refine_list_direction.Text.Trim();
						}
						else
						{
							roadid_condition += "%";
						}
						if (roadid_condition == "%x%x%x%")
						{
							roadid_condition = "";
						}
						tdpg.cmd[0].CommandText =
							tdpg.cmd[0].CommandText
							.Replace(
								"[[more_condition]]",
								(
									(textBox_refine_list_more_condition.Text.Trim().Length > 0) ?
									textBox_refine_list_more_condition.Text :
									"1=1"
								)
							)
							.Replace(
								"[[roadid_condition]]",
								(
									(roadid_condition.Length > 0) ?
									"[RoadID] LIKE '" + roadid_condition + "'" :
									"1=1"
								)
							)
							.Replace(
								"[[unique_run_condition]]",
								(
									(textBox_refine_list_unique_run.Text.Trim().Length > 0) ?
									"[UniqueRun] LIKE '" + textBox_refine_list_unique_run.Text + "'" :
									"1=1"
								)
							)
							.Replace(
								"[[year_condition]]",
								(
									(year_condition.Length > 0) ?
									year_condition :
									""
								)
							);

						//Console.WriteLine(tdpg.cmd[0].CommandText);
						tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
						while (tdpg.reader[0].Read())
						{
							listbox_run.Items.Add(
								new aran_run(
									utility.get_string(tdpg.reader[0][0]),
									utility.get_string(tdpg.reader[0][1]),
									Convert.ToDateTime(tdpg.reader[0][2]).ToString("yyyy-MM-dd"),
									utility.get_double(tdpg.reader[0][3]),
									utility.get_double(tdpg.reader[0][4])
								)
							);
						}
						tdpg.reader[0].Close();
						tdpg.reader[0] = null;
					}
					tdpg.close_conn_cmd(0);
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 3234           " + error_message);
			}
			Cursor.Current = Cursors.Default;
		}

		// the value of skip_points should be saved in the settings db
		private void show_selected_run(ListBox listbox_run, int skip_points = 30)
		{
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();
			// show the selected run on the map
			if ((listbox_run.Items.Count == 0) || (listbox_run.Text.Length == 0))
			{
				show_line_of_interest(null);
				show_point_of_interest(new PointLatLng(0, 0));
			}
			else
			{
				try
				{
					if (textBox_pms_frame_index_source_data_db_server.Text.Length == 0)
					{
						tdpg.initialize_generic_members("sqlite");
					}
					else
					{
						tdpg.initialize_generic_members("sql server");
					}
					tdpg.construct_connection_string(
						textBox_pms_frame_index_source_data_db_server.Text,
						"", "",
						textBox_pms_frame_index_source_data_db.Text
					);
					if (tdpg.open_conn_cmd(0))
					{
						foreach (string s in utility.load_script(setting.script("query_pms")))
						{
							if (textBox_pms_frame_index_source_data_db_server.Text.Length == 0)
							{
								tdpg.cmd[0].CommandText =
									s.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_table.Text + "]")
									.Replace("[[road_id]]", ((aran_run)listbox_run.SelectedItem).road_id)
									.Replace("[[unique_run]]", ((aran_run)listbox_run.SelectedItem).unique_run);
							}
							else
							{
								tdpg.cmd[0].CommandText =
									s.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_db.Text + "].[" + textBox_pms_frame_index_source_data_db_schema.Text + "].[" + textBox_pms_frame_index_source_data_table.Text + "]")
									.Replace("[[road_id]]", ((aran_run)listbox_run.SelectedItem).road_id)
									.Replace("[[unique_run]]", ((aran_run)listbox_run.SelectedItem).unique_run);
							}
							//Console.WriteLine(tdpg.cmd[0].CommandText);
							tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
							List<PointLatLng> points = new List<PointLatLng>();
							int skip = 0;
							while (tdpg.reader[0].Read())
							{
								if (skip++ % skip_points == 0)
								{
									double latitude = utility.get_double(tdpg.reader[0]["Latitude"], -1);
									double longitude = utility.get_double(tdpg.reader[0]["Longitude"], 0.0);

									// igonre bad gps point
									if (latitude > 20 && longitude < -80)
									{
										points.Add(new PointLatLng(latitude, longitude));
									}
								}
							}
							tdpg.reader[0].Close();
							tdpg.reader[0] = null;
							show_line_of_interest(points);
							show_point_of_interest(points[0]);
							points.Clear();
						}
					}
				}
				catch (Exception exc)
				{
					error_message = exc.Message;
					Console.WriteLine(current_function_name() + "(): 3335          " + error_message);
				}
				finally
				{
					tdpg.close_conn_cmd(0);
				}
			}
		}

		private void textBox_pms_frame_index_source_data_db_server_TextChanged(object sender, EventArgs e)
		{
			//populate_new_runs(listBox_all_new_run);
		}

		private void textBox_pms_frame_index_source_data_db_TextChanged(object sender, EventArgs e)
		{
			//populate_new_runs(listBox_all_new_run);
		}

		private void textBox_pms_frame_index_source_data_db_schema_TextChanged(object sender, EventArgs e)
		{
			//populate_new_runs(listBox_all_new_run);
		}

		private void textBox_pms_frame_index_source_data_table_TextChanged(object sender, EventArgs e)
		{
			//populate_new_runs(listBox_all_new_run);
		}

		private void textBox_access_target_db_pathname_Leave(object sender, EventArgs e)
		{
			check_target_db_validaty();
		}

		private void check_target_db_validaty()
		{
			if (textBox_access_target_db_pathname.Text.Trim().Length == 0)
			{
				return;
			}
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			bool valid = tdpa.open_conn_cmd(0, textBox_access_target_db_pathname.Text);
			tdpa.close_conn_cmd(0);
			if (!valid)
			{
				if (
					MessageBox.Show(
						"The target DB specified is not valid or does not exist. Create new one?",
						"MMHIS DB Authoring",
						MessageBoxButtons.YesNo
					)
					== System.Windows.Forms.DialogResult.Yes
				)
				{
					File.Delete(textBox_access_target_db_pathname.Text);
					db_utility.utility.create_access_db(textBox_access_target_db_pathname.Text, true);
				}
				else
				{
					textBox_access_target_db_pathname.Text = "";
				}
			}
		}

		private void button_browse_for_access_target_db_Click(object sender, EventArgs e)
		{
			saveFileDialog1.FileName = textBox_access_target_db_pathname.Text;
			saveFileDialog1.CheckFileExists = false;
			saveFileDialog1.OverwritePrompt = false;
			saveFileDialog1.CheckPathExists = true;
			saveFileDialog1.DefaultExt = "mdb";
			saveFileDialog1.Filter = "MS Access Files (*.mdb)|*.mdb|All Files (*.*)|*.*";
			try
			{
				saveFileDialog1.InitialDirectory = Path.GetDirectoryName(setting.current_mmhis_db);
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 3409          " + error_message);
			}
			saveFileDialog1.RestoreDirectory = true;
			saveFileDialog1.Title = "Target DB";
			if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				textBox_access_target_db_pathname.Text = saveFileDialog1.FileName;
				check_target_db_validaty();
			}
		}

		// return whether records already exist
		private bool check_all_target_table_validity()
		{
			if (
				check_target_table_validity(setting.script("check_table_validity_accident"), textBox_access_target_db_accident_table.Text, utility.three_character_section(the_section)) ||
				check_target_table_validity(setting.script("check_table_validity_bridge"), textBox_access_target_db_bridge_table.Text, the_section) ||
				check_target_table_validity(setting.script("check_table_validity_format"), textBox_access_target_db_format_table.Text, the_section) ||
				check_target_table_validity(setting.script("check_table_validity_frameindex"), textBox_access_target_db_frame_index_table.Text, the_section) ||
				check_target_table_validity(setting.script("check_table_validity_jobrecord"), textBox_access_target_db_job_record_table.Text, the_section) ||
				check_target_table_validity(setting.script("check_table_validity_pms"), textBox_access_target_db_pms_table.Text, the_section) ||
				check_target_table_validity(setting.script("check_table_validity_roadway"), textBox_access_target_db_roadway_table.Text, the_section) ||
				check_target_table_validity(setting.script("check_table_validity_turn"), textBox_access_target_db_turn_table.Text, the_section)
			)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private void remove_all_existing_data_for_section()
		{
			remove_existing_data_for_section(setting.script("remove_existing_data"), textBox_access_target_db_accident_table.Text, utility.three_character_section(the_section));
			remove_existing_data_for_section(setting.script("remove_existing_data"), textBox_access_target_db_bridge_table.Text, the_section);
			remove_existing_data_for_section(setting.script("remove_existing_data"), textBox_access_target_db_frame_index_table.Text, the_section);
			remove_existing_data_for_section(setting.script("remove_existing_data"), textBox_access_target_db_job_record_table.Text, the_section);
			remove_existing_data_for_section(setting.script("remove_existing_data"), textBox_access_target_db_pms_table.Text, the_section);
			remove_existing_data_for_section(setting.script("remove_existing_data"), textBox_access_target_db_roadway_table.Text, the_section);
		}

		private void remove_existing_data_for_section(
			string sql_script_pathname,
			string table_name,
			string section_number
		)
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			bool valid = tdpa.open_conn_cmd(0, textBox_access_target_db_pathname.Text);
			try
			{
				foreach (string s in utility.load_script(sql_script_pathname))
				{
					tdpa.cmd[0].CommandText =
						s.Replace("[[table_name]]", table_name)
						.Replace("[[section_number]]", section_number);
					tdpa.cmd[0].ExecuteNonQuery();
				}
			}
			catch { }
			finally
			{
				tdpa.close_conn_cmd(0);
			}
		}

		private bool check_target_table_validity(string sql_script_pathname, string table_name, string section_number)
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			bool record_exist = false;
			bool valid = tdpa.open_conn_cmd(0, textBox_access_target_db_pathname.Text);
			try
			{
				foreach (string s in utility.load_script(sql_script_pathname))
				{
					tdpa.cmd[0].CommandText =
						s.Replace("[[table_name]]", table_name)
						.Replace("[[section_number]]", section_number);
					tdpa.reader[0] = tdpa.cmd[0].ExecuteReader();
					if (tdpa.reader[0].Read())
					{//there is at least one record for the section
						record_exist = true;
					}
					tdpa.reader[0].Close();
					tdpa.reader[0] = null;
				}
			}
			catch
			{
				create_target_table(
					sql_script_pathname.Replace("check_table_validity", "create_table"),
					table_name
				);
			}
			finally
			{
				tdpa.close_conn_cmd(0);
			}
			return record_exist;
		}

		private void create_target_table(string sql_script_pathname, string table_name)
		{
			traffic_data_processing_access tdpa = new traffic_data_processing_access();
			bool valid = tdpa.open_conn_cmd(0, textBox_access_target_db_pathname.Text);
			try
			{
				foreach (string s in utility.load_script(sql_script_pathname))
				{
					tdpa.cmd[0].CommandText =
						s.Replace("[[table_name]]", table_name);
					tdpa.cmd[0].ExecuteNonQuery();
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 3525     " + error_message);
				MessageBox.Show(
					"Error executing command in " + sql_script_pathname,
					"MMHIS Authoring"
				);
			}
			finally
			{
				tdpa.close_conn_cmd(0);
			}
		}

		// for query location dropping down management
		// -and-
		// label moving
		// -and-
		// window moving
		// -and-
		// window sizing
		bool in_timer1 = false;
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (in_timer1)
			{
				return;
			}
			in_timer1 = true;

			if (panel_location_selection.ClientRectangle.Contains(panel_location_selection.PointToClient(Control.MousePosition)))
			{
				//expand_location_selection();
			}
			else
			{
				collapse_location_selection();
			}

			// label moving processing
			if (
				checkBox_preview_data_move_caption.Checked
				&&
				(dlee_win32.win32.GetAsyncKeyState(17) < 0)
			)
			{
				if (current_data_field_detail != null)
				{
					try
					{
						// move labels relating to the field on all specified windows
						data_field_detail the_data_field_detail;
						for (int i = 0; i < 10; i++)
						{
							if (
								dictionary_data_field_detail.TryGetValue(
									current_data_field_detail.field_specifier + i.ToString(),
									out the_data_field_detail
								)
							)
							{
								the_data_field_detail.the_caption_label.Location =
									the_data_field_detail.the_window_detail.the_window.PointToClient(Control.MousePosition);
								the_data_field_detail.caption_position_x = the_data_field_detail.the_caption_label.Location.X;
								the_data_field_detail.caption_position_y = the_data_field_detail.the_caption_label.Location.Y;
							}
							else
							{
								// no more keys in the dictionary
								// don't waste time
								break;
							}
						}

						objectListView_data_field_detail.RefreshObject(current_data_field_detail);
					}
					catch
					{
						checkBox_preview_data_move_caption.Checked = false;
					}
				}
				else
				{
					checkBox_preview_data_move_caption.Checked = false;
				}
			}
			else
			{
				if (checkBox_preview_data_move_caption.Checked)
				{
					checkBox_preview_data_move_caption.Checked = false;
					save_current_field_specifier_related_detail_items();
				}
			}

			// value moving processing
			if (
				checkBox_preview_data_move_value.Checked
				&&
				(dlee_win32.win32.GetAsyncKeyState(17) < 0)
			)
			{
				if (current_data_field_detail != null)
				{
					try
					{
						// move labels relating to the field on all specified windows
						data_field_detail the_data_field_detail;
						for (int i = 0; i < 10; i++)
						{
							if (
								dictionary_data_field_detail.TryGetValue(
									current_data_field_detail.field_specifier + i.ToString(),
									out the_data_field_detail
								)
							)
							{
								the_data_field_detail.the_value_label.Location =
									the_data_field_detail.the_window_detail.the_window.PointToClient(Control.MousePosition);
								the_data_field_detail.value_position_x = the_data_field_detail.the_value_label.Location.X;
								the_data_field_detail.value_position_y = the_data_field_detail.the_value_label.Location.Y;
							}
							else
							{
								// no more keys in the dictionary
								// don't waste time
								break;
							}
						}

						objectListView_data_field_detail.RefreshObject(current_data_field_detail);
					}
					catch
					{
						checkBox_preview_data_move_value.Checked = false;
					}
				}
				else
				{
					checkBox_preview_data_move_value.Checked = false;
				}
			}
			else
			{
				if (checkBox_preview_data_move_value.Checked)
				{
					checkBox_preview_data_move_value.Checked = false;
					save_current_field_specifier_related_detail_items();
				}
			}

			// unit moving processing
			if (
				checkBox_preview_data_move_unit.Checked
				&&
				(dlee_win32.win32.GetAsyncKeyState(17) < 0)
			)
			{
				if (current_data_field_detail != null)
				{
					try
					{
						// move labels relating to the field on all specified windows
						data_field_detail the_data_field_detail;
						for (int i = 0; i < 10; i++)
						{
							if (
								dictionary_data_field_detail.TryGetValue(
									current_data_field_detail.field_specifier + i.ToString(),
									out the_data_field_detail
								)
							)
							{
								the_data_field_detail.the_unit_label.Location =
									the_data_field_detail.the_window_detail.the_window.PointToClient(Control.MousePosition);
								the_data_field_detail.unit_position_x = the_data_field_detail.the_unit_label.Location.X;
								the_data_field_detail.unit_position_y = the_data_field_detail.the_unit_label.Location.Y;
							}
							else
							{
								// no more keys in the dictionary
								// don't waste time
								break;
							}
						}

						objectListView_data_field_detail.RefreshObject(current_data_field_detail);
					}
					catch
					{
						checkBox_preview_data_move_unit.Checked = false;
					}
				}
				else
				{
					checkBox_preview_data_move_unit.Checked = false;
				}
			}
			else
			{
				if (checkBox_preview_data_move_unit.Checked)
				{
					checkBox_preview_data_move_unit.Checked = false;
					save_current_field_specifier_related_detail_items();
				}
			}

			// window moving processing
			if (
				checkBox_preview_move_window.Checked
				&&
				(dlee_win32.win32.GetAsyncKeyState(17) < 0)
			)
			{
				if (current_window_detail != null)
				{
					try
					{
						Point mouse_position = Control.MousePosition;
						current_window_detail.left = mouse_position.X - current_window_detail.width / 2;
						current_window_detail.top = mouse_position.Y - current_window_detail.height / 2;
						mouse_position.Offset(-current_window_detail.width / 2, -current_window_detail.height / 2);
						current_window_detail.the_window.Location = mouse_position;
						objectListView_windows.RefreshObject(current_window_detail);
						//save_updated_window_details();
					}
					catch
					{
						checkBox_preview_move_window.Checked = false;
					}
				}
				else
				{
					checkBox_preview_move_window.Checked = false;
				}
			}
			else
			{
				if (checkBox_preview_move_window.Checked)
				{
					checkBox_preview_move_window.Checked = false;
					save_updated_window_details();
				}
			}

			// window change size processing
			if (
				checkBox_preview_window_change_size.Checked
				&&
				(dlee_win32.win32.GetAsyncKeyState(17) < 0)
			)
			{
				if (current_window_detail != null)
				{
					try
					{
						Point mouse_position = Control.MousePosition;
						int new_left = current_window_detail.left;
						int new_width = current_window_detail.width;
						int new_top = current_window_detail.top;
						int new_height = current_window_detail.height;
						if (mouse_position.X > current_window_detail.left)
						{
							new_width = mouse_position.X - current_window_detail.left;
						}
						else
						{
							new_width = 0;
						}
						if (mouse_position.Y > current_window_detail.top)
						{
							new_height = mouse_position.Y - current_window_detail.top;
						}
						else
						{
							new_height = 0;
						}
						current_window_detail.left = new_left;
						current_window_detail.top = new_top;
						current_window_detail.width = new_width;
						current_window_detail.height = new_height;
						current_window_detail.the_window.Location = new Point(new_left, new_top);
						current_window_detail.the_window.Width = new_width;
						current_window_detail.the_window.Height = new_height;
						objectListView_windows.RefreshObject(current_window_detail);
						//save_updated_window_details();
					}
					catch
					{
						checkBox_preview_window_change_size.Checked = false;
					}
				}
				else
				{
					checkBox_preview_window_change_size.Checked = false;
				}
			}
			else
			{
				if (checkBox_preview_window_change_size.Checked)
				{
					checkBox_preview_window_change_size.Checked = false;
					save_updated_window_details();
				}
			}

			// console toggling
			if (
				(dlee_win32.win32.GetAsyncKeyState(16) < 0) // shift
				&&
				(dlee_win32.win32.GetAsyncKeyState(17) < 0) // ctrl
				&&
				(dlee_win32.win32.GetAsyncKeyState(18) < 0) // alt
				&&
				(dlee_win32.win32.GetAsyncKeyState(20) < 0) // capital lock
			)
			{
				dlee_console_application.console_visibility.visible = true;
			}

			in_timer1 = false;
		}

		private void save_current_field_specifier_related_detail_items()
		{
			if (current_data_field_detail != null)
			{
				try
				{
					data_field_detail the_data_field_detail;
					for (int i = 0; i < 10; i++)
					{
						if (
							dictionary_data_field_detail.TryGetValue(
								current_data_field_detail.field_specifier + i.ToString(),
								out the_data_field_detail
							)
						)
						{
							save_updated_data_field_detail(the_data_field_detail);
						}
						else
						{
							// no more keys in the dictionary
							// don't waste time
							break;
						}
					}
				}
				catch { }
			}
		}

		// for enabling buttons
		private void timer2_Tick(object sender, EventArgs e)
		{
			enable_write_button();
		}

		// for playing video
		private void timer3_Tick(object sender, EventArgs e)
		{
			if (tabControl1.SelectedTab != tabControl1.TabPages["tabPage_preview"])
			{
				return;
			}

			if (trackBar_play_control.Value > 0)
			{
				change_frame(1);
				//if (hScrollBar_play_position.Value < hScrollBar_play_position.Maximum)
				//{
				//	hScrollBar_play_position.Value += 1;
				//}
				//else
				//{
				//	trackBar_play_control.Value = 0;
				//}
			}
			else if (trackBar_play_control.Value < 0)
			{
				change_frame(-1);
				//if (hScrollBar_play_position.Value > hScrollBar_play_position.Minimum)
				//{
				//	hScrollBar_play_position.Value -= 1;
				//}
				//else
				//{
				//	trackBar_play_control.Value = 0;
				//}
			}
		}

		public void change_frame(int step)
		{
			if (step > 0)
			{
				if (hScrollBar_play_position.Value <= hScrollBar_play_position.Maximum - step)
				{
					hScrollBar_play_position.Value += step;
				}
				else
				{
					hScrollBar_play_position.Value = hScrollBar_play_position.Maximum;
					trackBar_play_control.Value = 0;
				}
			}
			else if (step < 0)
			{
				if (hScrollBar_play_position.Value >= hScrollBar_play_position.Minimum - step)
				{
					hScrollBar_play_position.Value += step;
				}
				else
				{
					hScrollBar_play_position.Value = hScrollBar_play_position.Minimum;
					trackBar_play_control.Value = 0;
				}
			}
		}

		private void enable_write_button()
		{
			button_write_to_target_db.Enabled =
				(
					the_county.Length > 0 &&
					road_id_direction != "?" &&
					textBox_accident_db.Text.Length > 0 &&
					textBox_accident_db_server.Text.Length > 0 &&
					textBox_accident_crash_table.Text.Length > 0 &&
					textBox_bridge_db.Text.Length > 0 &&
					textBox_bridge_db_server.Text.Length > 0 &&
					textBox_bridge_table_view.Text.Length > 0 &&
					textBox_direction_conversion_db.Text.Length > 0 &&
					textBox_job_db.Text.Length > 0 &&
					textBox_job_table.Text.Length > 0 &&
					textBox_access_jpi_file.Text.Length > 0 &&
					textBox_access_jpi_path.Text.Length > 0 &&
					//textBox_pms_frame_index_source_data_db.Text.Length > 0 &&
					//textBox_pms_frame_index_source_data_db_server.Text.Length > 0 &&
					//textBox_pms_frame_index_source_data_table.Text.Length > 0 &&
					the_road_id.Length > 0 &&
					textBox_roadlog_db.Text.Length > 0 &&
					textBox_roadlog_db_server.Text.Length > 0 &&
					textBox_roadlog_table_view.Text.Length > 0 &&
					textBox_access_target_db_accident_table.Text.Length > 0 &&
					textBox_access_target_db_bridge_table.Text.Length > 0 &&
					textBox_access_target_db_format_table.Text.Length > 0 &&
					textBox_access_target_db_frame_index_table.Text.Length > 0 &&
					textBox_access_target_db_job_record_table.Text.Length > 0 &&
					textBox_access_target_db_pathname.Text.Length > 0 &&
					textBox_access_target_db_pms_table.Text.Length > 0 &&
					textBox_access_target_db_roadway_table.Text.Length > 0 &&
					textBox_access_target_db_turn_table.Text.Length > 0 &&
					listBox_all_new_run.Text.Length > 0
				);
			button_take_the_selected_run.Enabled =
				listBox_all_new_run.Text.Length > 0;
		}

		private void button_write_to_target_db_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			check_target_db_validaty();
			if (check_all_target_table_validity())
			{
				if (
					MessageBox.Show(
						"The data for this section already exist in the tables. Replace?",
						"MMHIS Database Authoring",
						MessageBoxButtons.YesNo
					) == System.Windows.Forms.DialogResult.No
				)
				{
					Cursor.Current = Cursors.Default;
					return;
				}
				remove_all_existing_data_for_section();
			}

			db_utility.utility.m_i_retry_delay_in_milliseconds = 0;
			db_utility.utility.m_i_retry_limit_on_error = 0;

			//accident table
			append_to_table(
				textBox_access_target_db_accident_table.Text,
				utility.load_script(setting.script("query_accident"))[0]
					.Replace("[[database]]", textBox_accident_db.Text)
					.Replace("[[schema]]", textBox_accident_schema.Text)
					.Replace("[[crash_table]]", textBox_accident_crash_table.Text)
					.Replace("[[person_table]]", textBox_accident_person_table.Text)
					.Replace("[[road_id]]", the_road_id),
				textBox_accident_db_server.Text,
				"",
				"",
				textBox_accident_db.Text,
				"sql_server"
			);

			//bridge table
			append_to_table(
				textBox_access_target_db_bridge_table.Text,
				utility.load_script(setting.script("query_bridge"))[0]
					.Replace("[[database]]", textBox_bridge_db.Text)
					.Replace("[[schema]]", textBox_bridge_db_schema.Text)
					.Replace("[[table_name]]", textBox_bridge_table_view.Text)
					.Replace("[[five_digit_route]]", utility.five_digit_route(the_route))
					.Replace("[[three_digit_section]]", utility.three_character_section(the_section))
				//.Replace("[[road_id_with_no_direction]]", road_id_with_no_direction)
					,
				textBox_bridge_db_server.Text,
				"gisuser",
				"gis",
				textBox_bridge_db.Text,
				"sql_server"
			);

			//job table
			append_to_table(
				textBox_access_target_db_job_record_table.Text,
				utility.load_script(setting.script("query_job_record"))[0]
					.Replace("[[database]]", textBox_job_sql_db.Text)
					.Replace("[[schema]]", textBox_job_sql_schema.Text)
					.Replace("[[table_name]]", textBox_job_sql_table_view.Text)
					.Replace("[[road_id]]", the_road_id)
					,
				textBox_job_sql_db_server.Text,
				"",
				"",
				textBox_job_sql_db.Text,
				"sql_server"
			);

			//pms table
			append_to_table(
				textBox_access_target_db_pms_table.Text,
				(
					(textBox_pms_frame_index_source_data_db_server.Text.Length == 0) ?
					utility.load_script(setting.script("query_pms"))[0]
						.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_table.Text + "]")
						.Replace("[[road_id]]", the_road_id)
						.Replace("[[unique_run]]", ((aran_run)listBox_all_new_run.SelectedItem).unique_run)
						.Replace("[[the_section]]", the_section)
					:
					utility.load_script(setting.script("query_pms"))[0]
						.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_db.Text + "].[" + textBox_pms_frame_index_source_data_db_schema.Text + "].[" + textBox_pms_frame_index_source_data_table.Text + "]")
						.Replace("[[road_id]]", the_road_id)
						.Replace("[[unique_run]]", ((aran_run)listBox_all_new_run.SelectedItem).unique_run)
						.Replace("[[the_section]]", the_section)
				),
				textBox_pms_frame_index_source_data_db_server.Text,
				"",
				"",
				textBox_pms_frame_index_source_data_db.Text,
				(textBox_pms_frame_index_source_data_db_server.Text.Length == 0) ? "sqlite" : "sql_server"
			);

			//frame index table
			append_to_table(
				textBox_access_target_db_frame_index_table.Text,
				(
					(textBox_pms_frame_index_source_data_db_server.Text.Length == 0) ?
					utility.load_script(setting.script("query_frame_index_sqlite"))[0]
						.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_table.Text + "]")
						.Replace("[[road_id]]", the_road_id)
						.Replace("[[unique_run]]", ((aran_run)listBox_all_new_run.SelectedItem).unique_run)
						.Replace("[[the_section]]", the_section)
						.Replace("[[the_video_filename]]", textBox_access_jpi_file.Text.Trim())
					:
					utility.load_script(setting.script("query_frame_index_sql_server"))[0]
						.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_db.Text + "].[" + textBox_pms_frame_index_source_data_db_schema.Text + "].[" + textBox_pms_frame_index_source_data_table.Text + "]")
						.Replace("[[road_id]]", the_road_id)
						.Replace("[[unique_run]]", ((aran_run)listBox_all_new_run.SelectedItem).unique_run)
						.Replace("[[the_section]]", the_section)
						.Replace("[[the_video_filename]]", textBox_access_jpi_file.Text.Trim())
				),
				textBox_pms_frame_index_source_data_db_server.Text,
				"",
				"",
				textBox_pms_frame_index_source_data_db.Text,
				(textBox_pms_frame_index_source_data_db_server.Text.Length == 0) ? "sqlite" : "sql_server"
			);

			//roadway inventory table
			append_to_table(
				textBox_access_target_db_roadway_table.Text,
				utility.load_script(setting.script("query_roadway_inventory"))[0]
					.Replace("[[database]]", textBox_roadlog_db.Text)
					.Replace("[[schema]]", textBox_roadlog_schema.Text)
					.Replace("[[table_name]]", textBox_roadlog_table_view.Text)
					.Replace("[[road_id]]", the_road_id)
					,
				textBox_roadlog_db_server.Text,
				"",
				"",
				textBox_roadlog_db.Text,
				"sql_server"
			);

			//bridge: use pms table to find the correct arnold logmiles
			update_bridge_logmile_to_arnold_logmile();

			create_jpi();

			update_mmhis_main();

			show_status("Done.");

			Cursor.Current = Cursors.Default;
		}

		// source_query_file contains the sql query to get the source data
		private void append_to_table(
			string target_table_name,
			string source_query,
			string source_server,
			string source_userid,
			string source_password,
			string source_db,
			string source_db_type
		)
		{
			traffic_data_processing_general tdpg_source = new traffic_data_processing_general();
			traffic_data_processing_general tdpg_target = new traffic_data_processing_general();
			tdpg_source.initialize_generic_members(source_db_type);
			tdpg_source.construct_connection_string(
				source_server,
				source_userid,
				source_password,
				source_db
			);
			tdpg_source.open_conn_cmd(0);

			tdpg_target.initialize_generic_members("access");
			tdpg_target.construct_connection_string(
				"",
				"",
				"",
				textBox_access_target_db_pathname.Text
			);
			tdpg_target.open_conn_cmd(0);

			string generated_create_table_sql = "";
			db_utility.utility.copy_table(
				tdpg_source.conn[0],
				tdpg_target.conn[0],
				source_query,
				"",
				"",
				target_table_name,
				ref generated_create_table_sql,
				"access",
				1
			);

			tdpg_source.close_conn_cmd(0);
			tdpg_target.close_conn_cmd(0);
		}

		private ArrayList al_arnold_points = null;
		// containing arnold_point objects

		private void load_arnold_points()
		{
			traffic_data_processing_general tdpg_target = new traffic_data_processing_general();
			tdpg_target.initialize_generic_members("access");
			tdpg_target.construct_connection_string(
				"",
				"",
				"",
				textBox_access_target_db_pathname.Text
			);
			tdpg_target.open_conn_cmd(0);

			// load all lat/long from pms table into an array
			al_arnold_points = new ArrayList();
			tdpg_target.cmd[0].CommandText =
				"SELECT LogMile, Latitude, Longitude FROM [" + textBox_access_target_db_pms_table.Text +
				"] WHERE " + utility.section_clause("Section", the_section);
			tdpg_target.reader[0] = tdpg_target.cmd[0].ExecuteReader();
			while (tdpg_target.reader[0].Read())
			{
				al_arnold_points.Add(
					new arnold_point(
						utility.get_double(tdpg_target.reader[0][0]),
						utility.get_double(tdpg_target.reader[0][1]),
						utility.get_double(tdpg_target.reader[0][2])
					)
				);
			}
			tdpg_target.reader[0].Close();
		}

		private void update_bridge_logmile_to_arnold_logmile()
		{
			if (al_arnold_points == null)
			{
				load_arnold_points();
			}

			traffic_data_processing_general tdpg_target = new traffic_data_processing_general();
			tdpg_target.initialize_generic_members("access");
			tdpg_target.construct_connection_string(
				"",
				"",
				"",
				textBox_access_target_db_pathname.Text
			);
			tdpg_target.open_conn_cmd(0);
			tdpg_target.create_query_snap_shot(
				0,
				"select * from [" + textBox_access_target_db_bridge_table.Text + "]",
				"bridge_table"
			);

			for (int i = 0; i < tdpg_target.ds[0].Tables["bridge_table"].Rows.Count; i++)
			{
				double logmeter = 0;
				double distance = 0;
				double bridge_length =
					utility.get_double(tdpg_target.ds[0].Tables["bridge_table"].Rows[i]["EndingLogMile"])
					-
					utility.get_double(tdpg_target.ds[0].Tables["bridge_table"].Rows[i]["BeginningLogMile"]);

				find_the_arnold_point(
					utility.get_double(tdpg_target.ds[0].Tables["bridge_table"].Rows[i]["Latitude"]),
					utility.get_double(tdpg_target.ds[0].Tables["bridge_table"].Rows[i]["Longitude"]),
					out logmeter,
					out distance,
					ref al_arnold_points
				);

				tdpg_target.ds[0].Tables["bridge_table"].Rows[i]["LogMile"] =
				tdpg_target.ds[0].Tables["bridge_table"].Rows[i]["BeginningLogMile"] = logmeter / 1609.344;
				tdpg_target.ds[0].Tables["bridge_table"].Rows[i]["EndingLogMile"] = logmeter / 1609.344 + bridge_length;
			}

			tdpg_target.adapter[0].Update(tdpg_target.ds[0].Tables["bridge_table"]);

			tdpg_target.close_conn_cmd(0);

			show_status("Bridge records updated with Arnold logmiles.");
		}

		// find the arnold point on the road that is cloest to the given point, together with the arnold logmeter
		private void find_the_arnold_point(
			double lat,
			double lng,
			out double logmeter,
			out double distance,
			ref ArrayList al_arnold_points
		)
		{
			distance = 100000000.0; // large enough
			logmeter = -1.0; // impossible logmeter
			double new_distance = 0;
			for (int i = 0; i < al_arnold_points.Count; i++)
			{
				new_distance = db_utility.utility.find_earth_surface_distance(
					lat,
					lng,
					((arnold_point)al_arnold_points[i]).latitude,
					((arnold_point)al_arnold_points[i]).longitude
				);
				if (new_distance < distance)
				{
					distance = new_distance;
					logmeter = ((arnold_point)al_arnold_points[i]).logmeter;
				}
			}
		}

		private void create_jpi()
		{
			traffic_data_processing_general tdpg_source = new traffic_data_processing_general();
			if (textBox_pms_frame_index_source_data_db_server.Text.Length == 0)
			{
				tdpg_source.initialize_generic_members("sqlite");
			}
			else
			{
				tdpg_source.initialize_generic_members("sql_server");
			}
			tdpg_source.construct_connection_string(
				textBox_pms_frame_index_source_data_db_server.Text,
				"",
				"",
				textBox_pms_frame_index_source_data_db.Text
			);
			tdpg_source.open_conn_cmd(0);

			tdpg_source.cmd[0].CommandText = 
				(textBox_pms_frame_index_source_data_db_server.Text.Length == 0) ?
				utility.load_script(setting.script("query_jpi"))[0]
					.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_table.Text + "]")
					.Replace("[[road_id]]", the_road_id)
					.Replace("[[unique_run]]", ((aran_run)listBox_all_new_run.SelectedItem).unique_run)
				:
				utility.load_script(setting.script("query_jpi"))[0]
					.Replace("[[table_spec]]", "[" + textBox_pms_frame_index_source_data_db.Text + "].[" + textBox_pms_frame_index_source_data_db_schema.Text + "].[" + textBox_pms_frame_index_source_data_table.Text + "]")
					.Replace("[[road_id]]", the_road_id)
					.Replace("[[unique_run]]", ((aran_run)listBox_all_new_run.SelectedItem).unique_run)
				;
			tdpg_source.reader[0] = tdpg_source.cmd[0].ExecuteReader();
			StreamWriter sw_jpi = new StreamWriter(Path.Combine(textBox_access_jpi_path.Text, textBox_access_jpi_file.Text));
			sw_jpi.Write(new char[] { (char)0, (char)6 });
			while (tdpg_source.reader[0].Read())
			{
				sw_jpi.Write(utility.get_string(tdpg_source.reader[0]["ROWImagePath"]).PadRight(260, (char)0));
				sw_jpi.Write(utility.get_string(tdpg_source.reader[0]["FrontLeftImagePath"]).PadRight(260, (char)0));
				sw_jpi.Write(utility.get_string(tdpg_source.reader[0]["FrontRightImagePath"]).PadRight(260, (char)0));
				sw_jpi.Write(utility.get_string(tdpg_source.reader[0]["RearLeftImagePath"]).PadRight(260, (char)0));
				sw_jpi.Write(utility.get_string(tdpg_source.reader[0]["RearRightImagePath"]).PadRight(260, (char)0));
				sw_jpi.Write(utility.get_string(tdpg_source.reader[0]["PavementImagePath"]).PadRight(260, (char)0));
			}
			sw_jpi.Close();
			tdpg_source.close_conn_cmd(0);

			show_status("JPI file created");
		}

		private void update_mmhis_main()
		{
			traffic_data_processing_general tdpg_target = new traffic_data_processing_general();
			tdpg_target.initialize_generic_members("access");
			tdpg_target.construct_connection_string(
				"",
				"",
				"",
				setting.current_mmhis_db
			);
			tdpg_target.open_conn_cmd(0);
			tdpg_target.cmd[0].CommandText =
				"insert into [Arkansas_Master] ([RouteNumber], [Direction], [DatabaseFileName], [VideoFilePath], [Year]) values ('" +
				the_route + "', " +
				((mmhis_direction.Length==0)? "NULL" : "'" + mmhis_direction + "'") +
				", '" +
				Path.GetFileName(textBox_access_target_db_pathname.Text) + "', '" +
				textBox_access_jpi_path.Text + "', '" +
				the_year + "')";
			tdpg_target.cmd[0].ExecuteNonQuery();
			tdpg_target.close_conn_cmd(0);

			show_status("MMHIS main table updated");
		}

		private void listBox_all_new_run_SelectedIndexChanged(object sender, EventArgs e)
		{
			show_selected_run(listBox_all_new_run);
		}

		private void button_take_the_selected_run_Click(object sender, EventArgs e)
		{
			string the_road_id_save = the_road_id = ((aran_run)listBox_all_new_run.SelectedItem).road_id;
			string[] road_id_parts = the_road_id.Split(new char[] { 'x' });
			if (road_id_parts.Length != 4)
			{
				MessageBox.Show(
					"The RoadID selected is not valid.",
					"MMHIS Authoring"
				);
				return;
			}
			the_county = road_id_parts[0];
			the_route = road_id_parts[1];
			string road_id_direction_save = road_id_direction = road_id_parts[3].ToUpper();
			if (
				(
					(road_id_direction != "A")
					&&
					(road_id_direction != "B")
				)
			)
			{
				MessageBox.Show(
					"The direction in RoadID is invalid.",
					"MMHIS Authoring"
				);
				return;
			}
			// search road_id direction to mmhis direction conversion table to find mmhis direction
			find_mmhis_direction_from_road_id_direction();
			// if not found, prompt user to enter one, in which case update the above table
			if (mmhis_direction == "?")
			{
				enter_mmhis_direction emd = new enter_mmhis_direction();
				if (emd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					mmhis_direction = emd.the_selected_direction.Substring(0, 1);
				}
				else
				{
					mmhis_direction = "";
				}
			}

			// enter route-direction, year, section, road_id to appropriate boxes
			comboBox_route.Text = the_route + (mmhis_direction.Length > 0 ? " " + mmhis_direction : "");
			the_year = ((aran_run)listBox_all_new_run.SelectedItem).the_date;
			enter_mmhis_year emy = new enter_mmhis_year();
			emy.the_year = the_year;
			if (emy.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				the_year = emy.the_year;
			}
			comboBox_year.Text = the_year;
			comboBox_section.Text = road_id_parts[2];
			textBox_road_id.Text = the_road_id_save;
			find_jpi_filename();
		}

		private void textBox_road_id_TextChanged(object sender, EventArgs e)
		{
			the_road_id = textBox_road_id.Text;

			road_id_direction = "?";
			string[] road_id_parts = the_road_id.Split(new char[] { 'x' });
			if (road_id_parts.Length != 4)
			{
				return;
			}
			road_id_direction = road_id_parts[3].ToUpper();
			if (
				(
					(road_id_direction != "A")
					&&
					(road_id_direction != "B")
				)
			)
			{
				road_id_direction = "?";
			}
		}

		private void button_refine_list_with_condition_Click(object sender, EventArgs e)
		{
			populate_new_runs(listBox_all_new_run);
		}

		public void show_line(List<PointLatLng> points)
		{
			foreach (window_detail the_window_detail in list_window_detail)
			{
				if (the_window_detail.window_type == "map")
				{
					((Map)the_window_detail.the_window).show_line(points);
				}
			}
		}

		public void show_point(PointLatLng point)
		{
			foreach (window_detail the_window_detail in list_window_detail)
			{
				if (the_window_detail.window_type == "map")
				{
					((Map)(the_window_detail.the_window)).show_point(point);
				}
			}
		}

		public void show_line_of_interest_segment(
			double logmeter_0,
			double logmeter_1,
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			List<PointLatLng> points = new List<PointLatLng>();
			System.Data.DataRowCollection drc = null;
			try
			{
				drc = tdpg_in_mem.ds[0].Tables[0].Rows;
			}
			catch
			{
				return;
			}

			if (logmeter_0 < 0 && logmeter_1 < 0)
			{
				show_line_of_interest(null);
			}

			int logmeter_0_frame_number = find_frame_number_for_logmeter(logmeter_0, tdpg_in_mem);
			int logmeter_1_frame_number = find_frame_number_for_logmeter(logmeter_1, tdpg_in_mem);
			int logmeter_0_frame_number_sorted = Math.Min(logmeter_0_frame_number, logmeter_1_frame_number);
			int logmeter_1_frame_number_sorted = Math.Max(logmeter_0_frame_number, logmeter_1_frame_number);

			for(int i = logmeter_0_frame_number_sorted; i <= logmeter_1_frame_number_sorted; i++)
			{
				points.Add(
					new PointLatLng(
						utility.get_double(drc[i]["latitude"]),
						utility.get_double(drc[i]["longitude"])
					)
				);
			}
			show_line_of_interest(points);
		}

		public void show_line_of_interest(List<PointLatLng> points)
		{
			foreach (window_detail the_window_detail in list_window_detail)
			{
				if (the_window_detail.window_type == "map")
				{
					((Map)the_window_detail.the_window).show_line_of_interest(points);
				}
			}
		}

		public void show_point_of_interest(PointLatLng point)
		{
			foreach (window_detail the_window_detail in list_window_detail)
			{
				if (the_window_detail.window_type == "map")
				{
					((Map)(the_window_detail.the_window)).show_point_of_interest(point);
				}
			}
		}

		private void create_sqlite_in_memory_db(
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			try
			{
				tdpg_in_mem.close_conn_cmd(0);
			}
			catch { }
			try
			{
				tdpg_in_mem.close_conn_cmd(1);
			}
			catch { }
			tdpg_in_mem.initialize_generic_members("sqlite");
			tdpg_in_mem.construct_connection_string(
				"",
				"",
				"",
				//"c:\\workahtd\\in_mem_" + System.Guid.NewGuid().ToString("N") + ".db"
				":memory:"
			);
			tdpg_in_mem.open_conn_cmd(0);
			try
			{
				foreach (string s in utility.load_script(setting.script("create_table_mmhis_damu_sqlite")))
				{
					tdpg_in_mem.cmd[0].CommandText = s;
					tdpg_in_mem.cmd[0].ExecuteNonQuery();
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 4533      " + error_message);
			}
			try
			{
				foreach (string s in utility.load_script(setting.script("create_table_mmhis_dian_sqlite")))
				{
					tdpg_in_mem.cmd[0].CommandText = s;
					tdpg_in_mem.cmd[0].ExecuteNonQuery();
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 4546      " + error_message);
				Console.WriteLine(tdpg_in_mem.cmd[0].CommandText);
			}
			try
			{
				foreach (string s in utility.load_script(setting.script("create_table_mmhis_duan_sqlite")))
				{
					tdpg_in_mem.cmd[0].CommandText = s;
					tdpg_in_mem.cmd[0].ExecuteNonQuery();
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 4559      " + error_message);
			}
			try
			{
				foreach (string s in utility.load_script(setting.script("create_table_mmhis_fen_sqlite")))
				{
					tdpg_in_mem.cmd[0].CommandText = s;
					tdpg_in_mem.cmd[0].ExecuteNonQuery();
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 4572    " + error_message);
			}
			try
			{
				foreach (string s in utility.load_script(setting.script("create_table_mmhis_data_field_sqlite")))
				{
					tdpg_in_mem.cmd[0].CommandText = s;
					tdpg_in_mem.cmd[0].ExecuteNonQuery();
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 4585     " + error_message);
			}
			try
			{
				foreach (string s in utility.load_script(setting.script("create_table_mmhis_window_sqlite")))
				{
					tdpg_in_mem.cmd[0].CommandText = s;
					tdpg_in_mem.cmd[0].ExecuteNonQuery();
				}
				//tdpg_in_mem.close_conn_cmd(0);
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 4599      " + error_message);
			}
		}

		private void button_preview_load_data_Click(object sender, EventArgs e)
		{
			trackBar_play_control.Value = 0;

			create_sqlite_in_memory_db(tdpg_in_memory);
			if (mmhis_db_source == "Microsoft Access")
			{
				int frame_loading_order;

				// Convert from legacy MMHIS Access DB to sqlite DB in memory
				load_section_from_microsoft_access_to_in_mem_sqlite(
					textBox_access_target_db_pathname.Text,
					Path.Combine(
						textBox_access_jpi_path.Text,
						textBox_access_jpi_file.Text
					),
					the_county,
					the_route,
					the_section,
					mmhis_direction,
					road_id_direction,
					the_year,
					out frame_loading_order,
					the_note,
					textBox_access_target_db_accident_table.Text,
					textBox_access_target_db_bridge_table.Text,
					textBox_access_target_db_frame_index_table.Text,
					textBox_access_target_db_job_record_table.Text,
					textBox_access_target_db_pms_table.Text,
					textBox_access_target_db_roadway_table.Text,
					out top_level_uuid
				);

				prepare_section_for_show(top_level_uuid, frame_loading_order);

				update_all_views(top_level_uuid);

				button_pause.Text = "Play";

				if (the_note.Length > 0)
				{
					MessageBox.Show(the_note);
				}
			}
		}

		// Use mmhis_dian table as logmeter for frames. They are kept in a DataTable in tdpg_in_mem.
		// tdpg_in_mem's DataTable is kept alive for the data display.
		private void prepare_section_for_show(
			string top_level_uuid,
			int frame_loading_order,
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			Console.WriteLine();
			Console.WriteLine("Prepare section for show...");
			tdpg_in_mem.create_query_snap_shot(
				0,
				utility.load_script(setting.script("prepare_section_for_playback"))[0]
					.Replace("[[mmhis_dian_table]]", "mmhis_dian")
					.Replace("[[frame_loading_order]]", (frame_loading_order > 0) ? "" : "DESC")
					.Replace("[[the_up_uuid]]", top_level_uuid),
				"mmhis_dian"
			);

			try
			{
				// Display the logmile range using the label
				label_current_logmile.Text =
					"Logmile ("
					+
					(
						utility.get_double(
							tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[0]["logmeter_0"]
						) / 1609.344
					).ToString("F3")
					+
					" - "
					+
					(
						utility.get_double(
							tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[
								tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows.Count - 1
							]["logmeter_0"]
						) / 1609.344
					).ToString("F3")
					+
					"):";
			}
			catch
			{
				label_current_logmile.Text = "Logmile:";
			}

			double logmeter, latitude, longitude;
			find_logmeter_latitude_longitude_for_frame_number(
				hScrollBar_play_position.Value,
				out logmeter,
				out latitude,
				out longitude
			);
			textBox_current_logmile.Text = (logmeter / 1609.344).ToString("F3");

			show_whole_section_on_window();

			hScrollBar_play_position.Value = 0;
			hScrollBar_play_position.Minimum = 0;
			hScrollBar_play_position.Maximum = tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows.Count - 1;

			Console.WriteLine("Finished prepare section for show.");
		}

		private int find_frame_number_for_logmeter(
			double logmeter,
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			double min_dist = 1000000.0;
			int frame_number = 0;
			for (int i = 0; i < tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows.Count; i++)
			{
				if (
					min_dist > Math.Abs(
						utility.get_double(tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[i]["logmeter_0"])
						-
						logmeter
					)
				)
				{
					min_dist = Math.Abs(
						utility.get_double(tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[i]["logmeter_0"])
						-
						logmeter
					);
					frame_number = i;
				}
			}
			return frame_number;
		}

		private void find_logmeter_latitude_longitude_for_frame_number(
			int frame_number,
			out double logmeter,
			out double latitude,
			out double longitude,
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			if (frame_number < 0 || frame_number >= tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows.Count)
			{
				logmeter = 0.0;
				latitude = 0.0;
				longitude = 0.0;
				return;
			}
			logmeter = utility.get_double(tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[frame_number]["logmeter_0"]);
			latitude = utility.get_double(tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[frame_number]["latitude"]);
			longitude = utility.get_double(tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[frame_number]["longitude"]);
		}

		private void find_logmeter_framenumber_for_gps(
			double latitude,
			double longitude,
			out double logmeter,
			out int frame_number,
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			logmeter = -1.0;
			frame_number = -1;
			double min_dist = 1000000.0;
			for (int i = 0; i < tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows.Count; i++)
			{
				double current_dist = db_utility.utility.find_earth_surface_distance(
					latitude,
					longitude,
					utility.get_double(tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[i]["latitude"]),
					utility.get_double(tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[i]["longitude"])
				);
				if (					min_dist > current_dist				)
				{
					min_dist = current_dist;
					logmeter = utility.get_double(tdpg_in_mem.ds[0].Tables["mmhis_dian"].Rows[i]["logmeter_0"]);
					frame_number = i;
				}
			}

			if (min_dist > 100)
			{
				logmeter = -1.0;
				frame_number = -1;
			}
		}

		private void show_whole_section_on_window(
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			Console.WriteLine("Loading whole section to window...");
			show_loaded_section_on_map(tdpg_in_mem, 1);
			show_data_table_on_window(tdpg_in_mem);
			Console.WriteLine("Done loading whole section to window.");
		}

		// the value of skip_points should be saved in the settings db
		private void show_loaded_section_on_map(
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null,
			int skip_points = 1
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			try
			{
				List<PointLatLng> points = new List<PointLatLng>();
				int skip = 0;
				foreach(DataRow dr in tdpg_in_mem.ds[0].Tables[0].Rows)
				{
					if (skip++ % skip_points == 0)
					{
						double latitude = utility.get_double(dr["latitude"], -1);
						double longitude = utility.get_double(dr["longitude"], -1);

						// igonre bad gps point
						if (latitude > 20 && longitude < -80)
						{
							points.Add(new PointLatLng(latitude, longitude));
						}
					}
				}
				show_line(points);
				points.Clear();
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 4742         " + error_message);
			}
		}

		private void show_data_table_on_window(
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			string field_category = "";
			string field_name = "";
			foreach (window_detail the_window_detail in list_window_detail)
			{
				if (the_window_detail.window_type == "data_table")
				{
					// In this case window_specifier stores the field_category for the data to
					// be shown in the objectlistview.
					// The format of the window_specifier is "data_table_<field_category>".
					// For example, "data_table_j_" means to show job record table.
					if (the_window_detail.window_specifier.Length < ("data_table_?").Length)
					{
						// window_specifier does not have the correct form
						continue;
					}

					field_category = the_window_detail.window_specifier.Substring(("data_table_").Length, 1);

					data_table_view dtv = (data_table_view)(the_window_detail.the_window);

					// retrieve all field names (columns) so that we can construct the columns
					// of the objectlistview

					// Note that we can also add columns to the objectlistview not by using this
					// query but when we actually retrieve data. The first round when data
					// is retrieved (for the first record), the columns can be constructed.
					// Columns constructed that way preserves the original column order. It also
					// saves some time because the following query will not be needed.

					// Using the following query, the column order will be alphanumerical.
					// Some people like this order because when reading the table columns can
					// be easily found.

					if (
						!tdpg_in_mem.create_query_snap_shot(
							2,
							"SELECT a.field_name " +
							"FROM mmhis_fen a INNER JOIN mmhis_duan b ON a.lu = b.ld " +
							"WHERE b.lu = '" + top_level_uuid + "' AND a.field_category = '" + field_category +
							"' GROUP BY a.field_name",
							"fen",
							0
						)
					)
					{
						continue;
					}

					// save field_name to the list for use later
					List<string> field_names = new List<string>();

					dtv.objectListView_data_table1.AllColumns.Clear();
					dtv.objectListView_data_table1.Columns.Clear();

					// add columns to the objectlistview
					BrightIdeasSoftware.OLVColumn olvColumn;

					foreach (DataRow dr in tdpg_in_mem.ds[2].Tables[0].Rows)
					{
						field_name = utility.get_string(dr["field_name"]);

						olvColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
						olvColumn.AspectName = field_name;
						olvColumn.MinimumWidth = 10;
						olvColumn.Sortable = false;
						olvColumn.Searchable = true;
						olvColumn.UseFiltering = false;
						olvColumn.Text = field_name;
						dtv.objectListView_data_table1.AllColumns.Add(olvColumn);
						dtv.objectListView_data_table1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvColumn });

						field_names.Add(field_name);
					}

					if (field_names.Count == 0)
					{
						continue;
						// There is nothing in this window
						// Proceed to the next window
					}

					// retrieve records and add them to list_data_record
					tdpg_in_mem.create_query_snap_shot(
						2,
						"SELECT a.field_category, a.field_name, a.field_value, b.logmeter_0, b.logmeter_1, b.latitude, b.longitude, b.ld " +
						"FROM mmhis_fen a INNER JOIN mmhis_duan b ON a.lu = b.ld " +
						"WHERE b.lu = '" + top_level_uuid + "' AND a.field_category = '" + field_category +
						"' ORDER BY b.ld",
						"fen",
						0
					);

					Dictionary<string, string> the_record = null;
					dtv.list_data_table.Clear();
					string current_uuid = "";
					string new_uuid = "";
					double logmeter_0, logmeter_0_loaded, logmeter_1, logmeter_1_loaded;
					foreach (DataRow dr in tdpg_in_mem.ds[2].Tables[0].Rows)
					{
						field_name = utility.get_string(dr["field_name"]);

						new_uuid = utility.get_string(dr["ld"]);
						if (new_uuid != current_uuid)
						{
							// This means we just finished a record.
							// This record needs to be added to the list.
							if (the_record != null)
							{
								dtv.list_data_table.Add(the_record);
							}

							// Then start a new record.
							current_uuid = new_uuid;
							the_record = new Dictionary<string, string>();

							// make sure all fields exist
							foreach (string fn in field_names)
							{
								the_record.Add(fn, "");
							}

							// We need some info on logmeters and GPS
							logmeter_0 = logmeter_0_loaded = utility.get_double(dr["logmeter_0"], -10000.0);
							logmeter_1 = logmeter_1_loaded = utility.get_double(dr["logmeter_1"], -10000.0);

							the_record.Add("logmeter_0", logmeter_0.ToString("F13"));
							the_record.Add("logmeter_1", logmeter_1.ToString("F13"));
							the_record.Add("latitude", utility.get_double(dr["latitude"]).ToString("F13"));
							the_record.Add("longitude", utility.get_double(dr["longitude"]).ToString("F13"));

						}
						the_record[field_name] =
							get_value(
								utility.get_string(dr["field_category"]) +
								"_" +
								utility.get_string(dr["field_name"]),
								utility.get_string(dr["field_value"])
							);
					}

					// add the last record
					if (the_record != null)
					{
						dtv.list_data_table.Add(the_record);
					}

					// The records in dtv.list_data_table are ordered by b.ld. We want to show the records
					// in logmeter order.
					data_table_record_comparer comparer = new data_table_record_comparer();  
					dtv.list_data_table.Sort(comparer);

					// show records in objectlistview
					dtv.objectListView_data_table1.SetObjects(dtv.list_data_table);
					dtv.objectListView_data_table1.RefreshObjects(dtv.list_data_table);
				}
			}
		}

		// Copies only one section from MMHIS Access DB to a local
		// work Access file in the temp_folder. The pathname is returned.
		private string copy_section_in_access_to_local(
			string source_db_pathname,
			string short_section,
			string accident_table,
			string bridge_table,
			string frame_index_table,
			string job_record_table,
			string pms_table,
			string roadway_table
		)
		{
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_0 = new traffic_data_processing_general();
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_1 = new traffic_data_processing_general();
			
			string work_file_pathname = "";

			work_file_pathname =
				"MMHIS_" +
				the_county + "_" +
				the_route + "_" +
				the_section + "_" +
				mmhis_direction + "_" +
				System.Guid.NewGuid().ToString("N") +
				".mdb";
			work_file_pathname = Path.Combine(
				setting.temp_folder,
				work_file_pathname
			);

			// create the above file
			db_utility.utility.create_access_db(work_file_pathname, true);

			//	textBox_access_target_db_pms_table.Text
			//	textBox_access_target_db_frame_index_table.Text
			//	textBox_access_target_db_accident_table.Text
			//	textBox_access_target_db_job_record_table.Text
			//	textBox_access_target_db_roadway_table.Text
			//	textBox_access_target_db_bridge_table.Text
			//	
			//	the_section as short_section in pms and frameindex tables
			//	three_character_section in accident table
			//	the_section as short_section in jobrecord table
			//	the_section as short_section in roadway table
			//	the_section as short_section in bridge table
			// to the above file
			tdpg_0.initialize_generic_members("access");
			tdpg_0.construct_connection_string(
				"",
				"",
				"",
				source_db_pathname
			);
			tdpg_0.open_conn_cmd(0);

			tdpg_1.initialize_generic_members("access");
			tdpg_1.construct_connection_string(
				"",
				"",
				"",
				work_file_pathname
			);
			tdpg_1.open_conn_cmd(0);
			string create_table_sql = "";

			// Accident
			db_utility.utility.copy_table(
				tdpg_0.conn[0],
				tdpg_1.conn[0],
				"SELECT * FROM [" + accident_table + "] WHERE " + utility.section_clause("Section", short_section),
				"",
				"",
				accident_table,
				ref create_table_sql,
				"access"
			);

			// Bridge
			db_utility.utility.copy_table(
				tdpg_0.conn[0],
				tdpg_1.conn[0],
				"SELECT * FROM [" + bridge_table + "] WHERE " + utility.section_clause("Section", short_section),
				"",
				"",
				bridge_table,
				ref create_table_sql,
				"access"
			);

			// Frame Index
			db_utility.utility.copy_table(
				tdpg_0.conn[0],
				tdpg_1.conn[0],
				"SELECT * FROM [" + frame_index_table + "] WHERE " + utility.section_clause("Section", short_section),
				"",
				"",
				frame_index_table,
				ref create_table_sql,
				"access"
			);

			// Job Record
			db_utility.utility.copy_table(
				tdpg_0.conn[0],
				tdpg_1.conn[0],
				"SELECT * FROM [" + job_record_table + "] WHERE " + utility.section_clause("Section", short_section),
				"",
				"",
				job_record_table,
				ref create_table_sql,
				"access"
			);

			// PMS
			db_utility.utility.copy_table(
				tdpg_0.conn[0],
				tdpg_1.conn[0],
				"SELECT * FROM [" + pms_table + "] WHERE " + utility.section_clause("Section", short_section),
				"",
				"",
				pms_table,
				ref create_table_sql,
				"access"
			);

			// Roadway
			db_utility.utility.copy_table(
				tdpg_0.conn[0],
				tdpg_1.conn[0],
				"SELECT * FROM [" + roadway_table + "] WHERE " + utility.section_clause("Section", short_section),
				"",
				"",
				roadway_table,
				ref create_table_sql,
				"access"
			);

			// create indexes for the above tables
			tdpg_1.open_conn_cmd(0);
			tdpg_1.cmd[0].CommandText = "create index i1 on [" + textBox_access_target_db_accident_table.Text + "] ([LogMile])";
			tdpg_1.cmd[0].ExecuteNonQuery();
			tdpg_1.cmd[0].CommandText = "create index i1 on [" + textBox_access_target_db_bridge_table.Text + "] ([BeginningLogMile], [EndingLogMile])";
			tdpg_1.cmd[0].ExecuteNonQuery();

			// Some old data don't have pms and frameindex table with matching logmiles.
			// We have to round the logmiles to make them match.
			tdpg_1.cmd[0].CommandText = "update [" + textBox_access_target_db_pms_table.Text + "] set [LogMile] = Round([LogMile], 1)";
			tdpg_1.cmd[0].ExecuteNonQuery();
			tdpg_1.cmd[0].CommandText = "update [" + textBox_access_target_db_frame_index_table.Text + "] set [LogMile] = Round([LogMile], 1)";
			tdpg_1.cmd[0].ExecuteNonQuery();

			tdpg_1.cmd[0].CommandText = "create index i1 on [" + textBox_access_target_db_frame_index_table.Text + "] ([LogMile])";
			tdpg_1.cmd[0].ExecuteNonQuery();
			tdpg_1.cmd[0].CommandText = "create index i1 on [" + textBox_access_target_db_job_record_table.Text + "] ([BeginningLogMile])";
			tdpg_1.cmd[0].ExecuteNonQuery();
			tdpg_1.cmd[0].CommandText = "create index i1 on [" + textBox_access_target_db_pms_table.Text + "] ([LogMile])";
			tdpg_1.cmd[0].ExecuteNonQuery();
			tdpg_1.cmd[0].CommandText = "create index i1 on [" + textBox_access_target_db_roadway_table.Text + "] ([BeginningLogMile], [EndingLogMile])";
			tdpg_1.cmd[0].ExecuteNonQuery();

			tdpg_0.close_conn_cmd(0);
			tdpg_1.close_conn_cmd(0);

			Console.WriteLine("Finished copying access db");

			return work_file_pathname;
		}

		// The following function converts legacy MMHIS database to sqlite
		// database in memory
		private string top_level_uuid = "";
		private void load_section_from_microsoft_access_to_in_mem_sqlite(
			string source_db_pathname,
			string jpi_pathname,
			string the_county,
			string the_route,
			string the_section,
			string mmhis_direction,
			string road_id_direction,
			string the_year,
			out int the_frame_loading_order,
			string the_note,
			string accident_table,
			string bridge_table,
			string frame_index_table,
			string job_record_table,
			string pms_table,
			string roadway_table,
			out string top_level_uuid,
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg_in_mem = null
		)
		{
			if (tdpg_in_mem == null)
			{
				tdpg_in_mem = tdpg_in_memory;
			}

			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new traffic_data_processing_general();

			string work_file_pathname = copy_section_in_access_to_local(
				source_db_pathname,
				the_section,
				accident_table,
				bridge_table,
				frame_index_table,
				job_record_table,
				pms_table,
				roadway_table
			);

			Console.WriteLine();
			Console.WriteLine("Loading access to sqlite in memory db...");

			tdpg.initialize_generic_members("access");
			tdpg.construct_connection_string(
				"",
				"",
				"",
				work_file_pathname
//source_db_pathname
			);
			tdpg.open_conn_cmd(0);

			ArrayList al_camera_view_pathname = null;
			load_jpi_file_into_memory(
				jpi_pathname,
				ref al_camera_view_pathname
			);

			top_level_uuid = System.Guid.NewGuid().ToString("N");

			// in order to get the frame_loading_order, we have to do this first

			////////////////////////////////////////////////////////////
			// write
			//    lu
			//    logmeter_0
			//    latitude
			//    longitude
			// from
			//    pms table
			// and
			//    ld
			// to
			//    mmhis_dian

			////////////////////////////////////////////////////////////
			// write
			//    lu
			//    LogMile (convert to mile) and all other fields from pms table
			//    start frame number  and end frame number for all views of video frames
			// and
			//    field_category "p" and field_name
			//    -and-
			//    field_category "f" and field_name designating frames (i.e., fl, f, fr, rl, rr, p)
			// from
			//    pms and frameindex tables
			// to
			//    mmhis_fen
			Console.WriteLine();
			Console.WriteLine("Loading PMS / FrameIndex...");
			int frame_loading_order;
			load_access_table_to_sqlite_tables(
				tdpg.conn[0],
				tdpg_in_mem.conn[0],
				ref al_camera_view_pathname,
				out frame_loading_order,
				utility.load_script(setting.script("load_dian_from_access_pms_frameindex"))[0]
					.Replace("[[table_name_1]]", pms_table)
					.Replace("[[table_name_2]]", frame_index_table)
					.Replace("[[short_section]]", the_section),
				"p",
				"f",
				top_level_uuid,
				"mmhis_dian",
				"mmhis_fen",
				"logmeter_0",
				"",
				"latitude",
				"longitude"
			);

			the_frame_loading_order = frame_loading_order;

			////////////////////////////////////////////////////////////
			// write
			//    the_county
			//    the_route
			//    the_section
			//    mmhis_direction
			//    road_id_direction
			//    the_year
			//    the_system (extract from access file name)
			//    common_path_for_image
			//    frame_loading_order
			//    the_note
			//    ld
			//    time_stamp
			//    computer_host_name
			//    user_id
			// to
			//    mmhis_damu
			string common_path_for_image = ""; // not used for now
			string the_system = Path.GetFileNameWithoutExtension(source_db_pathname);
			if (the_system.IndexOf('-') - 4 > 0)
			{
				the_system = the_system.Substring(4, the_system.IndexOf('-') - 4);
			}
			else
			{
				the_system = "unknown";
			}
			tdpg_in_mem.cmd[0].CommandText =
				string.Format(
					"insert into [mmhis_damu] " +
					"([{0}]" +
					",[{1}]" +
					",[{2}]" +
					",[{3}]" +
					",[{4}]" +
					",[{5}]" +
					",[{6}]" +
					",[{7}]" +
					",[{8}]" +
					",[{9}]" +
					",[{10}]" +
					",[{11}]" +
					",[{12}]" +
					",[{13}]" +
					") values " +
					"(@{0}" +
					",@{1}" +
					",@{2}" +
					",@{3}" +
					",@{4}" +
					",@{5}" +
					",@{6}" +
					",@{7}" +
					",@{8}" +
					",@{9}" +
					",@{10}" +
					",@{11}" +
					",@{12}" +
					",@{13}" +
					")",
					"county",
					"route",
					"section",
					"mmhis_direction",
					"arnold_direction",
					"the_year",
					"the_system",
					"common_path_for_image",
					"frame_loading_order",
					"note",
					"ld",
					"time_stamp",
					"computer_host_name",
					"user_id"
				);

			IDbDataParameter target_parameter = null;

			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@county";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = the_county;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);

			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@route";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = the_route;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);

			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@section";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = the_section;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);

			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@mmhis_direction";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = mmhis_direction;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);

			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@arnold_direction";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = road_id_direction;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);
	
			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@the_year";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = the_year;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);
			
			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@the_system";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = the_system;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);
			
			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@common_path_for_image";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = common_path_for_image;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);

			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@frame_loading_order";
			target_parameter.DbType = DbType.Int32;
			target_parameter.Value = frame_loading_order;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);
			
			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@note";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = the_note;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);
			
			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@ld";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = top_level_uuid;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);
			
			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@time_stamp";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = DateTime.Now.ToString("yyyyMMdd HHmmssfff");
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);
			
			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@computer_host_name";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = setting.machine_name;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);
			
			target_parameter = tdpg_in_mem.cmd[0].CreateParameter();
			target_parameter.ParameterName = "@user_id";
			target_parameter.DbType = DbType.String;
			target_parameter.Value = setting.user_id;
			tdpg_in_mem.cmd[0].Parameters.Add(target_parameter);
	
			tdpg_in_mem.cmd[0].ExecuteNonQuery();

			tdpg_in_mem.cmd[0].Parameters.Clear();
			
			////////////////////////////////////////////////////////////
			// write
			//    lu
			//    logmeter_0
			//    logmeter_1 (null)
			//    latitude
			//    longitude
			// from
			//    accdient table
			// and
			//    ld
			// to
			//    mmhis_duan

			////////////////////////////////////////////////////////////
			// write
			//    lu
			//    all fields
			// and
			//    field_category "a" and field_name
			// from
			//    accident table (query to find the cloest pms logmile that's within 30 meters)
			// to
			//    mmhis_fen

			////////////////////////////////////////////////////////////
			// for accident, data is written to mmhis_duan with logmeter_1 being null.
			// logically it is dian data but because it mostly does not match
			// pms (and frame index) points, mmhis_duan is used instead.
			Console.WriteLine();
			Console.WriteLine("Loading accident...");
			load_access_table_to_sqlite_tables(
				tdpg.conn[0],
				tdpg_in_mem.conn[0],
				ref al_camera_view_pathname,
				out frame_loading_order,
				utility.load_script(setting.script("load_duan_from_access_accident"))[0]
					.Replace("[[table_name_1]]", accident_table)
					.Replace("[[three_character_section]]", utility.three_character_section(the_section))
					.Replace("[[short_section]]", the_section),
				"a",
				"",
				top_level_uuid,
				"mmhis_duan",
				"mmhis_fen",
				"logmeter_0"
			);

			////////////////////////////////////////////////////////////
			// write
			//    lu
			//    logmeter_0 (BeginningLogMile converted to meters)
			//    logmeter_1 (BeginningLogMile + ProjectLength converted to  meters)
			//    latitude
			//    longitude
			//    ld
			// from
			//    JobRecord table
			// to
			//    mmhis_duan

			////////////////////////////////////////////////////////////
			// write 
			//    lu
			//    all fields
			// and
			//    field_category "j" and field_name
			// from
			//    JobRecord table
			// to
			//    mmhis_fen
			Console.WriteLine();
			Console.WriteLine("Loading JobRecords...");
			load_access_table_to_sqlite_tables(
				tdpg.conn[0],
				tdpg_in_mem.conn[0],
				ref al_camera_view_pathname,
				out frame_loading_order,
				utility.load_script(setting.script("load_duan_from_access_jobrecord"))[0]
					.Replace("[[table_name_1]]", job_record_table)
					.Replace("[[short_section]]", the_section),
				"j",
				"",
				top_level_uuid,
				"mmhis_duan",
				"mmhis_fen",
				"logmeter_0",
				"logmeter_1"
			);

			////////////////////////////////////////////////////////////
			// write
			//    lu 
			//    logmeter_0 (BeginningLogMile converted to meters)
			//    logmeter_1 (EndingLogMile converted to meters)
			//    latitude
			//    longitude
			//    ld
			// from
			//    roadway table
			// to
			//    mmhis_duan

			////////////////////////////////////////////////////////////
			// write
			//    lu
			//    all fields
 			// and
			//    field_category "r" and field_name
			// from
			//    roadway table
			// to
			//    mmhis_fen
			Console.WriteLine();
			Console.WriteLine("Loading roadway inventory...");
			load_access_table_to_sqlite_tables(
				tdpg.conn[0],
				tdpg_in_mem.conn[0],
				ref al_camera_view_pathname,
				out frame_loading_order,
				utility.load_script(setting.script("load_duan_from_access_roadway"))[0]
					.Replace("[[table_name_1]]", roadway_table)
					.Replace("[[short_section]]", the_section),
				"r",
				"",
				top_level_uuid,
				"mmhis_duan",
				"mmhis_fen",
				"logmeter_0",
				"logmeter_1"
			);

			////////////////////////////////////////////////////////////
			// write
			//    lu 
			//    logmeter_0 (BeginningLogMile converted to meters)
			//    logmeter_1 (EndingLogMile converted to meters)
			//    latitude
			//    longitude
			//    ld
			// from
			//    bridge table
			// to
			//    mmhis_duan

			////////////////////////////////////////////////////////////
			// write
			//    lu
			//    all fields
			// and
			//    field_category "b" and field_name
			// from
			//    bridge table
			// to
			//    mmhis_fen
			Console.WriteLine();
			Console.WriteLine("Loading bridge...");
			load_access_table_to_sqlite_tables(
				tdpg.conn[0],
				tdpg_in_mem.conn[0],
				ref al_camera_view_pathname,
				out frame_loading_order,
				utility.load_script(setting.script("load_duan_from_access_bridge"))[0]
					.Replace("[[table_name_1]]", bridge_table)
					.Replace("[[short_section]]", the_section),
				"b",
				"",
				top_level_uuid,
				"mmhis_duan",
				"mmhis_fen",
				"logmeter_0",
				"logmeter_1"
			);

			//try
			//{
			//	File.Delete(work_file_pathname);
			//}
			//catch { }

			Console.WriteLine("Finished loading access to sqlite in memory db.");
		}

		// This will generate mmhis_dian (or mmhis_duan) record and its linked mmhis_fen records
		// The returned frame_loading_order is only valid when loading PMS and FrameIndex tables.
		private bool load_access_table_to_sqlite_tables(
			IDbConnection source_conn,
			IDbConnection target_conn,
			ref ArrayList al_camera_view_pathname,
			out int frame_loading_order, // Only used when PMS and FrameIndex are loaded.
			String source_sql,
			string field_category_0,
			string field_category_1,
			string top_level_uuid,
			string target_dianduan_table,
			string target_fen_table,
			string logmeter_0_field_name = "logmeter_0",
			string logmeter_1_field_name = "",
			string latitude_field_name = "latitude",
			string longitude_field_name = "longitude"
		)
		{
			bool b_return = true;
			IDbCommand source_cmd = source_conn.CreateCommand();
			IDbCommand target_cmd = null;
			source_cmd.CommandText = source_sql;
			frame_loading_order = 1;
			try
			{
				if (source_conn.State != ConnectionState.Open)
				{
					source_conn.Open();
				}
				if (target_conn.State != ConnectionState.Open)
				{
					target_conn.Open();
				}

				IDataReader source_reader = null;

				source_cmd.CommandTimeout = 0;

				#region find frame_loading_order
				try
				{
					source_reader = source_cmd.ExecuteReader();
					double logmeter_start = -1.0;
					double logmeter_end = -1.0;
					while (source_reader.Read())
					{
						if (logmeter_start < 0.0)
						{
							logmeter_start = utility.get_double(source_reader["no_category_" + logmeter_0_field_name]);
						}
						logmeter_end = utility.get_double(source_reader["no_category_" + logmeter_0_field_name]);
					}
					frame_loading_order = (logmeter_start < logmeter_end) ? 1 : -1;
				}
				catch (Exception eee)
				{
					frame_loading_order = 1;
					Console.WriteLine(current_function_name() + "(): 5466      " + eee.Message);
				}
				finally
				{
					source_reader.Close();
				}
				#endregion

				try
				{
					source_reader = source_cmd.ExecuteReader();
				}
				catch (Exception eee)
				{
					Console.WriteLine(source_sql);
					Console.WriteLine(current_function_name() + "(): 5480      " + eee.Message);
					Console.WriteLine();
					return false;
				}

				#region retrieve field names from source quary and store them in column_names
				DataTable source_schema_table = source_reader.GetSchemaTable();
				List<string> column_names = new List<string>();
				string str_column_name = "";
				int i_expression_column_count = 0;
				foreach (DataRow row in source_schema_table.Rows)
				{
					#region retrieve column name => str_column_name
					try // row["ColumnName"] can be null
					{
						str_column_name = utility.get_string(row["ColumnName"]);
					}
					catch
					{
						try // row["BaseColumnName"] can be null
						{
							str_column_name = utility.get_string(row["BaseColumnName"]);
						}
						catch
						{
							if (Convert.ToBoolean(row["IsExpression"]))
							{
								str_column_name = "expression_" + i_expression_column_count.ToString();
							}
							else
							{
								str_column_name = "noname_" + i_expression_column_count.ToString();

								Console.WriteLine(
									"name of column " + utility.get_string(row["ColumnOrdinal"]) +
									" (zero-based) cannot be determined. " +
									str_column_name + " is used.");
								Console.WriteLine();
							}
						}
						i_expression_column_count++;
					}
					#endregion
					column_names.Add(str_column_name);
				}
				#endregion

				target_cmd = target_conn.CreateCommand();

				#region create four parameters for target_cmd: lu, field_category, field_name, and field_value
				IDbDataParameter target_parameter = target_cmd.CreateParameter();
				target_parameter.ParameterName = "@parameter_for_lu";
				target_parameter.DbType = DbType.String;
				target_cmd.Parameters.Add(target_parameter);
				target_parameter = target_cmd.CreateParameter();
				target_parameter.ParameterName = "@parameter_for_field_category";
				target_parameter.DbType = DbType.String;
				target_cmd.Parameters.Add(target_parameter);
				target_parameter = target_cmd.CreateParameter();
				target_parameter.ParameterName = "@parameter_for_field_name";
				target_parameter.DbType = DbType.String;
				target_cmd.Parameters.Add(target_parameter);
				target_parameter = target_cmd.CreateParameter();
				target_parameter.ParameterName = "@parameter_for_field_value";
				target_parameter.DbType = DbType.String;
				target_cmd.Parameters.Add(target_parameter);
				#endregion

				IDbTransaction target_transaction = target_conn.BeginTransaction();
				target_cmd.Transaction = target_transaction;
				while (source_reader.Read())
				{
					string ld = "";

					#region find frame number range and store in frame_number_0 and frame_number_1 (for repeating records)
					// for older mmhis data many frames fit into one pms record.
					// in this case we have to repeat the pms record for each frame
					// it is necessary to find out the frame range
					int frame_number_0 = 0;
					int frame_number_1 = 1;
					// to guarantee one loop
					// For loading PMS and FrameIndex tables, the frame number range is guaranteed to be found, so the frame_number_1 is no worry here.
					// However, for loading other tables (Accident, JobRecord, RoadLog, Bridge, etc.), a field with "frame_to" in the value will NOT
					// be found. Later, we need the record to appear in the mmhis_fen table once, we have to make frame_number_1 = 1 to guarantee that.

					// Search the columns for the first frame number range column
					// and find the frame number range.
					foreach (string this_column in column_names)
					{
						if (
							(source_reader[this_column] != DBNull.Value)
							&&
							(utility.get_string(source_reader[this_column]).Trim().Length > 0)
						)
						{
							string field_value = utility.get_string(source_reader[this_column]);

							// for video frames, the format will be like "21 frame_to 24", representing
							// a range of frames
							if (field_value.Contains("frame_to"))
							{
								string[] frames = null;
								frames = field_value.Split(new char[] { ' ' });
								if (frames.Length > 0)
								{
									frame_number_0 = utility.get_int(frames[0], -1);
								}
								if (frames.Length > 2)
								{
									frame_number_1 = utility.get_int(frames[2], -1);
								}

								// There are a few other fields containing the frame number range.
								// But they should have the same range. We don't need to look at them anymore.
								break;
							}
						}
					}
					#endregion

					// For the case one PMS record corresponds a range of frames, the PMS
					// record has to be repeated for each frame.
					// For other tables, because we set frame_number_0 = 0 and frame_number_1 = 1, one pass is guaranteed.
					for (int i_frame_number = frame_number_0; i_frame_number < frame_number_1; i_frame_number++)
					{
						// ld = "link_down"
						ld = System.Guid.NewGuid().ToString("N");

						#region add record to dian/duan table for logmeter_0, logmeter_1, latitude, longitude
						target_cmd.CommandText =
							"insert into [" + target_dianduan_table + "] (lu, ld, [" + logmeter_0_field_name + "]";
						if (logmeter_1_field_name.Length > 0)
						{
							target_cmd.CommandText += ", [" + logmeter_1_field_name + "]";
						}
						if (latitude_field_name.Length > 0)
						{
							target_cmd.CommandText += ", [" + latitude_field_name + "]";
						}
						if (longitude_field_name.Length > 0)
						{
							target_cmd.CommandText += ", [" + longitude_field_name + "]";
						}
						target_cmd.CommandText +=
							") values ('" +
							top_level_uuid + "', '" +
							ld + "', " +
							// for the case multiple frames correspond to one record
							// we change the logmeter a bit to differenciate them
							(
								utility.get_double(source_reader["no_category_" + logmeter_0_field_name]) +
								// each frame adds/subtract (depending on frame_loading_order)
								// 0.5 meters to the logmeter
								frame_loading_order * 0.5 * (i_frame_number - frame_number_0)
							)
							.ToString("F3");
						if (logmeter_1_field_name.Length > 0)
						{
							target_cmd.CommandText += ", " +
								utility.get_double(source_reader["no_category_" + logmeter_1_field_name]).ToString("F3");
						}
						if (latitude_field_name.Length > 0)
						{
							object the_field = null;
							try
							{
								the_field = source_reader["no_category_" + latitude_field_name];
							}
							catch
							{
								// This is the case that the latitude is a field queried out by a '*' in the field list,
								// or the field does not exist at all.
								try { the_field = source_reader[latitude_field_name]; }
								catch { }
							}
							target_cmd.CommandText += ", " +
								utility.get_double(the_field, -10000.0).ToString("F13");
						}
						if (longitude_field_name.Length > 0)
						{
							object the_field = null;
							try
							{
								the_field = source_reader["no_category_" + longitude_field_name];
							}
							catch
							{
								// This is the case that the longitude is a field queried out by a '*' in the field list,
								// or the field does not exist at all.
								try { the_field = source_reader[longitude_field_name]; }
								catch { }
							}
							target_cmd.CommandText += ", " +
								utility.get_double(the_field, -10000.0).ToString("F13");
						}
						target_cmd.CommandText += ")";
						target_cmd.ExecuteNonQuery();
						#endregion

						// retrieve the value for each field from the source and insert a record to
						// target mmhis_fen_table
						target_cmd.CommandText =
							"insert into [" + target_fen_table +
							"] (lu, field_category, field_name, field_value) values " +
							"(@parameter_for_lu, @parameter_for_field_category, @parameter_for_field_name, @parameter_for_field_value)";
						((IDbDataParameter)(target_cmd.Parameters["@parameter_for_lu"])).Value = ld;
						foreach (string this_column in column_names)
						{
							string field_category = "";
							string field_name = "";
							// since the query contain * as part of the field list, we
							// need a way to tell which fields need a prefix to form 
							// field specifiers
							if (this_column.Contains("no_category_"))
							{
								field_category = "";
								field_name = this_column.Replace("no_category_", "");
							}
							else if (this_column.Contains("field_category_0_"))
							{
								field_category = field_category_0;
								field_name = this_column.Replace("field_category_0_", "");
							}
							else if (this_column.Contains("field_category_1_"))
							{
								field_category = field_category_1;
								field_name = this_column.Replace("field_category_1_", "");
							}
							else
							{
								// for columns queried out by using '*'
								field_category = field_category_0;
								field_name = this_column;
							}

							if (
								(
									(field_name == logmeter_0_field_name)
									||
									(field_name == logmeter_1_field_name)
									||
									(field_name == latitude_field_name)
									||
									(field_name == longitude_field_name)
								)
								&&
								(field_category == "")
							)
							{
								// skip these because they are for upper level table (mmhis_dian or mmhis_duan)
								continue;
							}

							if (
								(source_reader[this_column] != DBNull.Value)
								&&
								(utility.get_string(source_reader[this_column]).Trim().Length > 0)
							)
							{
								string field_value = utility.get_string(source_reader[this_column]);

								if (field_value.Contains("frame_to"))
								{
									// here we add special treatment for frame index
									// we need to replace the frame number with the real
									// pathname of the frame file
									switch (field_name)
									{
										case "fl":
											field_value = get_frame(i_frame_number, 1, ref al_camera_view_pathname);
											break;
										case "f":
											field_value = get_frame(i_frame_number, 0, ref al_camera_view_pathname);
											break;
										case "fr":
											field_value = get_frame(i_frame_number, 2, ref al_camera_view_pathname);
											break;
										case "rl":
											field_value = get_frame(i_frame_number, 3, ref al_camera_view_pathname);
											break;
										case "rr":
											field_value = get_frame(i_frame_number, 4, ref al_camera_view_pathname);
											break;
										case "p":
											field_value = get_frame(i_frame_number, 5, ref al_camera_view_pathname);
											break;
										default:
											// just keep the current value
											break;
									}
								}
								((IDbDataParameter)(target_cmd.Parameters["@parameter_for_field_value"])).Value =
									field_value;
								((IDbDataParameter)(target_cmd.Parameters["@parameter_for_field_category"])).Value =
									field_category;
								((IDbDataParameter)(target_cmd.Parameters["@parameter_for_field_name"])).Value =
									field_name;
								try
								{
									target_cmd.ExecuteNonQuery();
								}
								catch (Exception exc2)
								{
									error_message = exc2.Message;
									Console.WriteLine(current_function_name() + "(): 5486      " + error_message);
									Console.WriteLine();
								}
							}
						}
					}
				}
				try
				{
					target_transaction.Commit();
				}
				catch (Exception exc2)
				{
					error_message = exc2.Message;
					Console.WriteLine(current_function_name() + "(): 5504      " + error_message);
					Console.WriteLine();
					b_return = false;
				}
			}
			catch (Exception exc)
			{
				error_message = exc.Message;
				Console.WriteLine(current_function_name() + "(): 5512      " + error_message);
				Console.WriteLine();
				b_return = false;
			}
			finally
			{
				// do not close the target connection because it is in memory db
				//try
				//{
				//	target_cmd.Cancel();//so that closing the connection later will not take a long time on large tables
				//	target_conn.Close();
				//}
				//catch { }
				try
				{
					source_cmd.Cancel();//so that closing the connection later will not take a long time on large tables
					source_conn.Close();
				}
				catch { }
			}

			return b_return;
		}

		private string get_frame(int frame_number, int view, ref ArrayList al_camera_view_pathname)
		{
			if (frame_number >= 0 && frame_number < al_camera_view_pathname.Count)
			{
				return ((string[])(al_camera_view_pathname[frame_number]))[view];
			}
			return "";
		}

		private void load_jpi_file_into_memory(
			string jpi_file_pathname,
			ref ArrayList al_camera_view_pathname
		)
		{
			int number_of_cameras = 0;
			al_camera_view_pathname = new ArrayList();

			FileStream fs_jpi = null;
			BinaryReader br_jpi = null;

			fs_jpi = new FileStream(jpi_file_pathname, FileMode.Open, FileAccess.Read, FileShare.Read);
			br_jpi = new BinaryReader(fs_jpi, new ASCIIEncoding());
			int jpi_file_length = (int)(fs_jpi.Length);

			string[] pathname = new string[] { "", "", "", "", "", "" };
			char[] whole_file_in_char = br_jpi.ReadChars(jpi_file_length);
			br_jpi.Close();
			fs_jpi.Close();

			string whole_file = new string(whole_file_in_char, 0, jpi_file_length);

			if (whole_file[0] != (char)0)
			{
				number_of_cameras = 1;
			}
			else
			{
				number_of_cameras = (int)(whole_file[1]);
				whole_file = whole_file.Substring(2);
			}
			int k = 0;
			for (int i = 0; i < whole_file.Length; i += 260 * number_of_cameras)
			{
				pathname = new string[] { "", "", "", "", "", "" };
				for (int j = 0; j < number_of_cameras; j++)
				{
					if ((number_of_cameras == 5) && j == 4)
					{
						//always put pavement view in the last position
						k = 5;
					}
					else
					{
						k = j;
					}

					// For older jpi files, there are junk after the image pathname.
					// Therefore, we can't use TrimEnd() function to trim the '\0' characters out.
					// We need to clean it out manually.
					int index_for_zero_char = 0;
					for (index_for_zero_char = 0; index_for_zero_char < 260; index_for_zero_char++)
					{
						if (whole_file.Substring(i + 260 * j + index_for_zero_char, 1)[0] == '\0')
						{
							break;
						}
					}
					pathname[k] = whole_file.Substring(i + 260 * j, index_for_zero_char);

					// vu put double backslash in the middle of the path. we need to solve that problem
					// but somehow the image can be loaded even without the fix!!!!!?????
					if (pathname[k].StartsWith("\\\\"))
					{
						pathname[k] = "\\" + pathname[k].Replace("\\\\", "\\");
					}
					else
					{
						pathname[k] = pathname[k].Replace("\\\\", "\\");
					}
				}
				al_camera_view_pathname.Add(pathname);
			}
		}

		private void hScrollBar_play_position_ValueChanged(object sender, EventArgs e)
		{
			update_all_views(top_level_uuid);
			double logmeter, latitude, longitude;
			find_logmeter_latitude_longitude_for_frame_number(
				hScrollBar_play_position.Value,
				out logmeter,
				out latitude,
				out longitude
			);
			textBox_current_logmile.Text = (logmeter / 1609.344).ToString("F3");
		}

		private void trackBar_play_control_ValueChanged(object sender, EventArgs e)
		{
			if (trackBar_play_control.Value != 0)
			{
				timer3.Interval = 1000 - 10 * Math.Abs(trackBar_play_control.Value) * 10 + 10;
				timer3.Enabled = true;
				button_pause.Text = "Stop";
			}
			else
			{
				timer3.Enabled = false;
				button_pause.Text = "Play";

				if (hScrollBar_play_position.Value == hScrollBar_play_position.Minimum)
				{
					play_control_value_save = 10;
				}
				else if (hScrollBar_play_position.Value == hScrollBar_play_position.Maximum)
				{
					play_control_value_save = -10;
				}
				update_all_views(top_level_uuid);
			}
		}

		private int play_control_value_save = 10;
		private void button_pause_Click(object sender, EventArgs e)
		{
			if (trackBar_play_control.Value != 0)
			{
				play_control_value_save = trackBar_play_control.Value;
				trackBar_play_control.Value = 0;
			}
			else
			{
				trackBar_play_control.Value = play_control_value_save;
			}
		}

		private void textBox_current_logmile_Enter(object sender, EventArgs e)
		{
			stop_video();
		}

		private void stop_video()
		{
			if (trackBar_play_control.Value != 0)
			{
				play_control_value_save = trackBar_play_control.Value;
				trackBar_play_control.Value = 0;
			}
		}

		private void textBox_current_logmile_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				do_go_to_logmeter();
			}
		}

		private void textBox_current_logmile_Leave(object sender, EventArgs e)
		{
			do_go_to_logmeter();
		}

		private void do_go_to_logmeter(double logmeter = -1.0)
		{
			if (logmeter < 0)
			{
				logmeter = Convert.ToDouble(textBox_current_logmile.Text) * 1609.344;
			}
			hScrollBar_play_position.Value = find_frame_number_for_logmeter(logmeter);
			update_all_views(top_level_uuid);
		}

		public void go_to_logmeter(double logmeter)
		{
			stop_video();
			textBox_current_logmile.Text = (logmeter / 1609.344).ToString("F3");

			do_go_to_logmeter(logmeter);
		}

		public void go_to_gps(double latitude, double longitude)
		{
			stop_video();
			double logmeter;
			int frame_number;
			find_logmeter_framenumber_for_gps(
				latitude,
				longitude,
				out logmeter,
				out frame_number
			);
			if (frame_number >= 0)
			{
				hScrollBar_play_position.Value = frame_number;
				update_all_views(top_level_uuid);
			}
		}

		bool the_control_text_changed = false;
		private void control_text_changed(object sender, EventArgs e)
		{
			if (form_loaded)
			{
				the_control_text_changed = true;
				setting.dictionary_control_text[((Control)sender).Name] = ((Control)sender).Text;

				set_image_path_replace();
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (
				the_control_text_changed
				&&
				(
					MessageBox.Show(
						"Save control contents?",
						"MMHIS",
						MessageBoxButtons.YesNo
					) == System.Windows.Forms.DialogResult.Yes
				)
			)
			{
				update_dictionary_control_text();
				setting.do_save_dictionary(setting.control_text_user_setting_pathname, setting.dictionary_control_text);
			}
		}

		private void button_preview_query_search_execute_Click(object sender, EventArgs e)
		{
			// clear the list
			// search mmhis_fen for the item
			// use lu to find mmhis_duan record
			// use lu to find mmhis_damu record
			// populate list with field name, field value, route, section info ordered by time (desc)
		}
	}
}
