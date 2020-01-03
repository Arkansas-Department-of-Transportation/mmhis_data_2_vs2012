using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;

namespace mmhis_data_2_vs2012
{
	class setting
	{
		//public static string temp_folder = Path.GetTempPath();
		// -or-
		public static string temp_folder = @"c:\workahtd";

		public static string startup_path = System.Windows.Forms.Application.StartupPath;

//		public static string source_user_setting_folder = "";
		public static string program_setting_folder = "";
		public static string program_script_folder = "";

		public static string user_setting_folder = "";

		public static string user_id = Environment.UserName;
		public static string machine_name = Environment.MachineName;

		// sqlite files
//		public static string source_window_and_data_field_user_setting_pathname = "";
//		public static string source_control_text_user_setting_pathname = "";
		public static string pointer_program_setting_pathname = "";
		public static string data_field_format_program_setting_pathname = "";

		public static string window_and_data_field_user_setting_pathname = "";
		public static string control_text_user_setting_pathname = "";

		public static Dictionary<string, string> dictionary_pointer = new Dictionary<string, string>();
		public static Dictionary<string, string> dictionary_control_text = new Dictionary<string, string>();
		public static Dictionary<string, data_field_format> dictionary_data_field_format = new Dictionary<string, data_field_format>();

		public static string current_mmhis_db = "";

		// these are for performance purposes only
		public static string play_mmhis_data_frameindex_only_script = "";
		public static string play_mmhis_data_all_script = "";

		public static bool prepare_user_setting_file()
		{
			bool b_return = false;

//			source_user_setting_folder = Path.Combine(startup_path, @"mmhis\setting\user_setting_template");
			program_setting_folder = Path.Combine(temp_folder, @"mmhis\setting\program_setting");
			program_script_folder = Path.Combine(temp_folder, @"mmhis\setting\program_script");

			user_setting_folder = Path.Combine(temp_folder, @"mmhis\setting\" + user_id);

			// sqlite files
//			source_window_and_data_field_user_setting_pathname = Path.Combine(source_user_setting_folder, "mmhis_window_and_data_field_user_setting");
//			source_control_text_user_setting_pathname = Path.Combine(source_user_setting_folder, "mmhis_control_text_user_setting");
			pointer_program_setting_pathname = Path.Combine(program_setting_folder, "mmhis_pointer_program_setting");
			data_field_format_program_setting_pathname = Path.Combine(program_setting_folder, "mmhis_data_field_format_program_setting");

			window_and_data_field_user_setting_pathname = Path.Combine(user_setting_folder, "mmhis_window_and_data_field_user_setting");
			control_text_user_setting_pathname = Path.Combine(user_setting_folder, "mmhis_control_text_user_setting");

			b_return = load_pointer_and_control_text();
			if (b_return)
			{
				play_mmhis_data_frameindex_only_script = utility.load_script(script("play_mmhis_data_frameindex_only"))[0];
				play_mmhis_data_all_script = utility.load_script(script("play_mmhis_data_all"))[0];

				tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new tech_serv_data_processing_modules.traffic_data_processing_general();
				try
				{
					tdpg.initialize_generic_members("sqlite");
					tdpg.construct_connection_string("", "", "", window_and_data_field_user_setting_pathname);
					tdpg.open_conn_cmd(0);
					tdpg.cmd[0].CommandText = utility.load_script(setting.script("check_table_validity_mmhis_data_field"))[0];
					tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
					tdpg.reader[0].Close();
					tdpg.cmd[0].CommandText = utility.load_script(setting.script("check_table_validity_mmhis_window"))[0];
					tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
					tdpg.reader[0].Close();
				}
				catch (Exception eee)
				{
					Console.WriteLine(eee.Message);
					tdpg.close_conn_cmd(0);
					b_return = false;
					//				Directory.CreateDirectory(user_setting_folder);
					//				File.Copy(source_window_and_data_field_user_setting_pathname, window_and_data_field_user_setting_pathname, true);
				}
			}

			if (b_return)
			{
				b_return = load_data_field_format("english");
			}

			return b_return;
		}

		public static void set_control_text(Control c)
		{
			string the_text;
			if (
				// this call will handle the case of non-existant key
				setting.dictionary_control_text.TryGetValue(
					c.Name,
					out the_text
				)
			)
			{
				c.Text = the_text;
			}
		}

		public static void update_dictionary_control_text(Control c)
		{
			string the_text;
			if (
				// this call will handle the case of non-existant key
				setting.dictionary_control_text.TryGetValue(
					c.Name,
					out the_text
				)
			)
			{
				setting.dictionary_control_text[c.Name] = c.Text;
			}
		}

		// load into "dictionary_data_field_format"
		public static bool load_data_field_format(
			string unit_system // can only be "english" or "metric"
		)
		{
			bool b_return = false;

			dictionary_data_field_format.Clear();
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new tech_serv_data_processing_modules.traffic_data_processing_general();
			tdpg.initialize_generic_members("sqlite");
			try
			{
					tdpg.construct_connection_string("", "", "", setting.data_field_format_program_setting_pathname);
					tdpg.open_conn_cmd(0);
					tdpg.cmd[0].CommandText = "select * from mmhis_data_field_format";
				tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
				while (tdpg.reader[0].Read())
				{
					data_field_format the_data_field_format = new data_field_format();
					the_data_field_format.field_specifier = tdpg.reader[0]["field_specifier"].ToString();
					the_data_field_format.caption_suggestion = tdpg.reader[0]["caption_suggestion"].ToString();
					the_data_field_format.value_format = tdpg.reader[0]["value_format_" + unit_system].ToString();
					the_data_field_format.unit = tdpg.reader[0]["unit_" + unit_system].ToString();
					the_data_field_format.conversion_factor = utility.get_double(tdpg.reader[0]["conversion_factor_" + unit_system], 1.0);
					dictionary_data_field_format.Add(the_data_field_format.field_specifier, the_data_field_format);
				}
				b_return = true;
			}
			catch
			{
				b_return = false;
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

			return b_return;
		}

		public static string script(string key)
		{
			return Path.Combine(
				setting.program_script_folder,
				setting.dictionary_pointer[key]
			);
		}

		// load contents in the "dictionary" table in db into d
		public static bool do_load_dictionary(
			string db,
			Dictionary<string, string> d
		)
		{
			bool b_return = false;
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new tech_serv_data_processing_modules.traffic_data_processing_general();
			try
			{
				tdpg.initialize_generic_members("sqlite");
				tdpg.construct_connection_string("", "", "", db);
				tdpg.open_conn_cmd(0);
				tdpg.cmd[0].CommandText = "select key, value from dictionary";
				tdpg.reader[0] = tdpg.cmd[0].ExecuteReader();
				d.Clear();
				while (tdpg.reader[0].Read())
				{
					d.Add(
						tdpg.reader[0]["key"].ToString(),
						tdpg.reader[0]["value"].ToString()
					);
				}
				b_return = true;
			}
			catch
			{
				b_return = false;
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
			return b_return;
		}

		// save contents in d into the "dictionary" table in db
		public static bool do_save_dictionary(
			string db,
			Dictionary<string, string> d
		)
		{
			bool b_return = false;
			string the_value = null;
			tech_serv_data_processing_modules.traffic_data_processing_general tdpg = new tech_serv_data_processing_modules.traffic_data_processing_general();
			try
			{
				tdpg.initialize_generic_members("sqlite");
				tdpg.construct_connection_string("", "", "", db);
				tdpg.open_conn_cmd(0);
				tdpg.cmd[0].CommandText = "update dictionary set value=@value, time_stamp=@time_stamp where key=@key and value<>@value";
				((SQLiteCommand)tdpg.cmd[0]).Parameters.Add("key", System.Data.DbType.String);
				((SQLiteCommand)tdpg.cmd[0]).Parameters.Add("value", System.Data.DbType.String);
				((SQLiteCommand)tdpg.cmd[0]).Parameters.Add("time_stamp", System.Data.DbType.Int64);
				foreach (string key in d.Keys)
				{
					if (d.TryGetValue(key, out the_value))
					{
						((SQLiteCommand)tdpg.cmd[0]).Parameters["key"].Value = key;
						((SQLiteCommand)tdpg.cmd[0]).Parameters["value"].Value = the_value;
						((SQLiteCommand)tdpg.cmd[0]).Parameters["time_stamp"].Value = DateTime.Now.Ticks;
						tdpg.cmd[0].ExecuteNonQuery();
					}
				}
				b_return = true;
			}
			catch
			{
				b_return = false;
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
			return b_return;
		}

		public static bool load_pointer_and_control_text()
		{
			bool b_return = do_load_dictionary(pointer_program_setting_pathname, dictionary_pointer);
			if (b_return)
			{
				b_return = do_load_dictionary(control_text_user_setting_pathname, dictionary_control_text);
				if (!b_return)
				{
//					Directory.CreateDirectory(user_setting_folder);
//					try
//					{
//						File.Copy(source_control_text_user_setting_pathname, control_text_user_setting_pathname, true);
//					}
//					catch { }
//					b_return = do_load_dictionary(control_text_user_setting_pathname, dictionary_control_text);
				}
			}

			// load from table "window_state"

			return b_return;
		}

		public void save_control_text()
		{
			// save to table "control_text"

			// save to table "window_state"
		}
	}

	class utility
	{
		#region data retrievers with default value
		public static bool get_boolean_from_int(object a, bool default_value = false)
		{
			try
			{
				default_value = (Convert.ToInt32(a) == 0) ? false : true;
			}
			catch { }
			return default_value;
		}

		public static int get_int(object a, int default_value = 0)
		{
			try
			{
				default_value = Convert.ToInt32(a);
			}
			catch { }
			return default_value;
		}

		public static float get_float(object a, float default_value = 10)
		{
			try
			{
				default_value = Convert.ToSingle(a);
			}
			catch { }
			return default_value;
		}

		public static double get_double(object a, double default_value = 0.0)
		{
			try
			{
				default_value = Convert.ToDouble(a);
			}
			catch { }
			return default_value;
		}

		public static string get_string(object a, string default_value = "")
		{
			try
			{
				default_value = a.ToString().Trim();
			}
			catch { }
			return default_value;
		}

		public static System.Drawing.Color get_color_from_string(object color_object)
		{
			try
			{
				string color_string = color_object.ToString();
				return
					System.Drawing.Color.FromArgb(
						Convert.ToInt32(color_string.Substring(4, 2), 16),
						Convert.ToInt32(color_string.Substring(2, 2), 16),
						Convert.ToInt32(color_string.Substring(0, 2), 16)
					);
			}
			catch
			{
				return System.Drawing.Color.Black;
			}
		}

		public static string get_string_from_color(System.Drawing.Color the_color)
		{
			try
			{
				return string.Format(
					"{0:X2}{1:X2}{2:X2}",
					the_color.B,
					the_color.G,
					the_color.R
				);
			}
			catch
			{
				return "000000";
			}
		}
		#endregion

		// guarantee the section number to be three characters wide (fixed width)
		public static string three_character_section(string possible_short_section)
		{
			possible_short_section = possible_short_section.Trim();
			if (possible_short_section.Length == 1)
			{
				return "0" + possible_short_section + "0";
			}
			else
				if (possible_short_section.Length == 2)
				{
					if (Char.IsDigit(possible_short_section, 1))
					{
						return possible_short_section + "0";
					}
					else
					{
						return "0" + possible_short_section;
					}
				}
				else
				{
					return possible_short_section;
				}
		}

		public static string section_clause(string field_name, string short_section)
		{
			return
				"([" + field_name + "] = '" + utility.three_character_section(short_section) +
				"' or [" + field_name + "] = '" + short_section + "')";
		}

		// guarantee the route number to be at least 5 characters wide, using leading zeros when necessary
		public static string five_digit_route(string possible_short_route)
		{
			// separate route number and direction
			possible_short_route = (possible_short_route.Trim().Split(null))[0]; // Split(null) splits using white space

			if (possible_short_route.Length >= 5)
			{
				return possible_short_route;
			}
			return new string('0', 5 - possible_short_route.Length) + possible_short_route;
		}

		public static string short_route(string possible_short_route)
		{
			possible_short_route = (possible_short_route.Trim().Split(null))[0]; // Split(null) splits using white space
			return possible_short_route;
		}

		// file can contain multiple statements separated by blank lines
		// comments started with // are removed
		public static string[] load_script(string pathname)
		{
			StreamReader sr = new StreamReader(pathname);
			string whole_file = sr.ReadToEnd();
			sr.Close();

			Regex r;
			r = new Regex("^ *//[^\r]*\r\n", RegexOptions.Multiline);
			// try to find comments start with two slashes and end at end of line and occupy a whole line

			//remove the above comments and leave no blank lines
			whole_file = r.Replace(whole_file, "");

			r = new Regex("//[^\r]*\r\n");
			// try to find comments start with two slashes and end at end of line
			// (these should be not occupying a whole line because those were removed in the previous step)

			//remove the above comments
			whole_file = r.Replace(whole_file, "\r\n");

			// statements are separated by \r\n\r\n
			string[] individual_statement = whole_file.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < individual_statement.Length; i++)
			{
				string[] individual_line = individual_statement[i].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < individual_line.Length; j++)
				{
					// remove each line's leading and trailing blanks
					individual_line[j] = individual_line[j].Trim();
				}

				// each statement becomes one long line
				individual_statement[i] = string.Join(" ", individual_line);
			}

			return individual_statement;
		}

		// fill the middle part of the rectangle using the color specified
		public static bool draw_color(
			System.Drawing.Graphics g,
			System.Drawing.Rectangle r,
			Object color_string_object
		)
		{
			if (color_string_object == null)
			{
				return false;
			}
			if (color_string_object.ToString().Length == 6)
			{
				try
				{
					g.FillRectangle(
						new System.Drawing.SolidBrush(
							System.Drawing.Color.White
						),
						r
					);
					if (r.Height < r.Width * 2)
					{
						r.X += Convert.ToInt32(r.Height / 4.0);
						r.Width -= Convert.ToInt32(r.Height / 1.4);
					}
					r.Y += Convert.ToInt32(r.Height / 4.0);
					r.Height = Convert.ToInt32(r.Height / 2.0);
					g.FillRectangle(
						new System.Drawing.SolidBrush(
							utility.get_color_from_string(
								color_string_object.ToString()
							)
						),
						r
					);
				}
				catch { }
				return true;
			}
			else
			{
				return false;
			}
		}

		// we use generic here so we can use different types
		// the type T is of IComparable, so .CompareTo can be used.
		public static string aspect_to_string<T>(object o, T critical_value) where T : IComparable
		{
			try
			{
				//T the_value = (T)Convert.ChangeType(o, typeof(T));
				T the_value = (T)o;
				if (the_value.CompareTo(critical_value) < 0)
				{
					return "";
				}
				else
				{
					return the_value.ToString();
				}
			}
			catch
			{
				return "";
			}
		}

		public IDbConnection configuration_connection()
		{
			// two levels to find configuration
			// local: c:\workAHTD\mmhis_configuration\mmhis_settings.db
			// with mmhis database either sqlite or sql server: together with mmhis' other tables (read-only)
			// On the above two levels, there are three tables: mmhis_data_field_format, mmhis_data_field, mmhis_window
			return null;
		}

		public static void empty_value_compound_component_text_from_dictionary_data_field_format()
		{
			foreach (string key in setting.dictionary_data_field_format.Keys)
			{
				setting.dictionary_data_field_format[key].value_compound_component_text = "";
			}
		}

		public static void get_data_field_value_compound_component_text(
			string field_specifier,
			string value_from_fen,
			string value_compound_component_text_already_found
		)
		{
			data_field_format the_data_field_format = null;
			double numeric_value_original = -1000000001.0; // guaranteed to be not a value from the db

			if (
				setting.dictionary_data_field_format.TryGetValue(
					field_specifier,
					out the_data_field_format
				)
			)
			{
				if (value_compound_component_text_already_found.Length == 0)
				{
					numeric_value_original = utility.get_double(value_from_fen, -1000000001.0);

					if (the_data_field_format.value_format.Length > 0)
					{
						if (numeric_value_original > -1000000000.0)
						{
							the_data_field_format.value_compound_component_text = string.Format(
								the_data_field_format.value_format,
								numeric_value_original * the_data_field_format.conversion_factor
							);
						}
						else
						{
							the_data_field_format.value_compound_component_text = string.Format(
								the_data_field_format.value_format,
								value_from_fen
							);
						}
					}
					else
					{
						the_data_field_format.value_compound_component_text = value_from_fen;
					}
				}
				else
				{
					the_data_field_format.value_compound_component_text = value_compound_component_text_already_found;
				}
			}
		}

		private static tech_serv_data_processing_modules.traffic_data_processing_general tdpg =
			new tech_serv_data_processing_modules.traffic_data_processing_general();
		private static bool sqlite_for_string_evaluation_ready = false;

		public static void prepare_sqlite_for_string_evaluation()
		{
			try
			{
				tdpg.initialize_generic_members("sqlite");
				tdpg.construct_connection_string("", "", "", ":memory:");
				tdpg.open_conn_cmd(0);
				sqlite_for_string_evaluation_ready = true;
			}
			catch
			{
				sqlite_for_string_evaluation_ready = false;
			}
		}

		// Since C# does not have an easy way to evaluate expressions
		// dynamically generated and stored in text strings, the
		// SQL SELECT statement is used to achieve this.
		public static string evaluate_value_format(
			string value_format
		)
		{
			string result = value_format;
			if (value_format.Length > 0)
			{
				if (sqlite_for_string_evaluation_ready)
				{
					tdpg.cmd[0].CommandText = "SELECT " + value_format;
					try
					{
						result = tdpg.cmd[0].ExecuteScalar().ToString();
					}
					catch
					{
						result = "";
					}
				}
				else
				{
					result = "";
				}
			}
			return result;
		}
	}

	class aran_run
	{
		public string road_id;
		public string unique_run;
		public string the_date;
		public double beginning_logmile;
		public double ending_logmile;

		public aran_run(string road_id, string unique_run, string the_date, double beginning_logmile, double ending_logmile)
		{
			this.road_id = road_id;
			this.unique_run = unique_run;
			this.the_date = the_date;
			this.beginning_logmile = beginning_logmile;
			this.ending_logmile = ending_logmile;
		}

		public override string ToString()
		{
			return
				unique_run + "\t" +
				road_id.PadRight(40, ' ').Replace('x', ' ') + "\t" +
				the_date + "\t" +
				beginning_logmile.ToString("F3").PadLeft(7, ' ') + " -> " + ending_logmile.ToString("F3").PadLeft(7, ' ') +
				" (" + (ending_logmile - beginning_logmile).ToString("F3").PadLeft(7, ' ') + " miles)";
		}
	}
	
	class arnold_point
	{
		public arnold_point(double logmeter, double latitude, double longitude)
		{
			this.logmeter = logmeter;
			this.latitude = latitude;
			this.longitude = longitude;
		}
		public double logmeter;
		public double latitude;
		public double longitude;
	}
	
	//class bridge
	//{
	//	public string bridge_id;
	//	public double logmile;
	//	public string description;
	//	public double latitude;
	//	public double longitude;

	//	public bridge(string bridge_id, double logmile, string description, double latitude, double longitude)
	//	{
	//		this.bridge_id = bridge_id;
	//		this.logmile = logmile;
	//		this.description = description;
	//		this.latitude = latitude;
	//		this.longitude = longitude;
	//	}

	//	public override string ToString()
	//	{
	//		return 
	//			bridge_id + "\t" +
	//			logmile.ToString("F3") + "\t" +
	//			description;
	//	}
	//}
	
	//class data_label
	//{
	//	public Label the_caption_label;
	//	public Label the_value_label;
	//	public Label the_unit_label;
	//	public bool updated;
	//	public data_label(
	//		Label the_caption_label,
	//		Label the_value_label,
	//		Label the_unit_label,
	//		bool updated = false)
	//	{
	//		this.the_caption_label = the_caption_label;
	//		this.the_value_label = the_value_label;
	//		this.the_unit_label = the_unit_label;
	//		this.updated = updated;
	//	}
	//}

	class data_field_format
	{
		public string field_specifier { set; get; }
		public string caption_suggestion { set; get; }
		public string value_format { set; get; }
		public string unit { set; get; }
		public double conversion_factor { set; get; }
		public string value_compound_component_text { set; get; }

		// The indexer is defined so that the properties can be accessed through the form
		//	object["uuid"]
		// instead of the form
		//	object.uuid
		// This is to aid programming in special occasions.
		//uses "System.Reflection" namespace
		public object this[string property_name]
		{
			get
			{
				PropertyInfo property =
					this.GetType().GetProperty(property_name);
				return property.GetValue(this, null);
			}
			set
			{
				PropertyInfo property =
					this.GetType().GetProperty(property_name);
				property.SetValue(this, value, null);
			}
		}
	}

	public class data_field_detail
	{
		public window_detail the_window_detail { set; get; }
		public Label the_caption_label { set; get; }
		public Label the_value_label { set; get; }
		public Label the_unit_label { set; get; }
		public bool updated { set; get; }

		public data_field_detail(
			string uuid,
			int display_order,
			string save_operation = "insert"
		)
		{
			this.uuid = uuid;
			this.save_operation = save_operation;
			this.display_order = display_order;
			field_specifier = System.Guid.NewGuid().ToString("N");
			window_specifier = System.Guid.NewGuid().ToString("N");
			the_window_detail = null;
			the_caption_label = null;
			the_value_label = null;
			the_unit_label = null;
		}

		// The indexer is defined so that the properties can be accessed through the form
		//	object["uuid"]
		// instead of the form
		//	object.uuid
		// This is to aid programming in special occasions.
		//uses "System.Reflection" namespace
		public object this[string property_name]
		{
			get
			{
				PropertyInfo property =
					this.GetType().GetProperty(property_name);
				return property.GetValue(this, null);
			}
			set
			{
				PropertyInfo property =
					this.GetType().GetProperty(property_name);
				property.SetValue(this, value, null);
			}
		}

		// These properties correspond to mmhis_data_field data table fields
		public string uuid { set; get; }
		public string user_id { set; get; }
		public string format_group_id { set; get; }
		public string field_specifier { set; get; }
		public string[] field_specifier_array { set; get; }
		public string window_specifier { set; get; }
		public string caption_text { set; get; }
		public int caption_position_x { set; get; }
		public int caption_position_y { set; get; }
		public string caption_font_name { set; get; }
		public double caption_font_size { set; get; }
		public bool caption_font_bold { set; get; }
		public bool caption_font_italic { set; get; }
		public bool caption_font_underline { set; get; }
		public string caption_color { set; get; }
		public string value_text { set; get; }
		public double value_conversion_factor { set; get; }
		public string value_format { set; get; }
		public string value_compound_component_text { set; get; }
		public bool value_evaluation_required { set; get; }
		public int value_position_x { set; get; }
		public int value_position_y { set; get; }
		public string value_font_name { set; get; }
		public double value_font_size { set; get; }
		public bool value_font_bold { set; get; }
		public bool value_font_italic { set; get; }
		public bool value_font_underline { set; get; }
		public string value_color { set; get; }
		public string unit_text { set; get; }
		public int unit_position_x { set; get; }
		public int unit_position_y { set; get; }
		public string unit_font_name { set; get; }
		public double unit_font_size { set; get; }
		public bool unit_font_bold { set; get; }
		public bool unit_font_italic { set; get; }
		public bool unit_font_underline { set; get; }
		public string unit_color { set; get; }
		public int display_order { set; get; }

		// This is to help manage in-place editing of the record.
		// This is not necessary if in-place editing is not used.
		public string save_operation { set; get; } //"insert" or "update"
	}

	public class window_detail
	{
		public window_detail(
			string uuid,
			string save_operation = "insert"
		)
		{
			this.uuid = uuid;
			this.save_operation = save_operation;
		}

		// The indexer is defined so that the properties can be accessed through the form
		//	object["uuid"]
		// instead of the form
		//	object.uuid
		// This is to aid programming in special occasions.
		//uses "System.Reflection" namespace
		public object this[string property_name]
		{
			get
			{
				PropertyInfo property =
					this.GetType().GetProperty(property_name);
				return property.GetValue(this, null);
			}
			set
			{
				PropertyInfo property =
					this.GetType().GetProperty(property_name);
				property.SetValue(this, value, null);
			}
		}

		//properties
		public string uuid { set; get; }
		public string user_id { set; get; }
		public string window_group_id { set; get; }
		public string window_specifier { set; get; }
		public bool window_visibility { set; get; }
		public string window_type { set; get; }
		public string caption_text { set; get; }
		public bool control_box { set; get; }
		public bool maximize_box { set; get; }
		public bool minimize_box { set; get; }
		public int opacity { set; get; }
		public bool top_most { set; get; }
		public bool show_icon { set; get; }
		public bool show_in_taskbar { set; get; }
		public string border_style { set; get; }
		public int left { set; get; }
		public int top { set; get; }
		public int width { set; get; }
		public int height { set; get; }
		public string background_color { set; get; }

		// These are backer members for the corresponding similarly named properties.
		// these are set to public so if needed we can get access
		// to them directly. that is necesary when we want to
		// save the values to db even when the window type is not "map"
		public string the_map_provider;
		public string the_map_line_color_0;
		public int the_map_line_width_0;
		public string the_map_line_color_1;
		public int the_map_line_width_1;
		public double the_map_initial_center_latitude;
		public double the_map_initial_center_longitude;
		public double the_map_initial_zoom_level;
		public int the_map_zoom_minimum;
		public int the_map_zoom_maximum;
		public bool the_map_center_current_location;

		// the following map related properties will return empty
		// when the window type is "map". this is necessary for
		// the objectlistview to show blank in the fields corresponding
		// to these properties when the window type is not "map"
		public string map_provider {
			set
			{
				the_map_provider = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_provider;
				}
				else
				{
					return ""; // to make objectlistview to show nothing
				}
			}
		}
		public string map_line_color_0 {
			set
			{
				the_map_line_color_0 = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_line_color_0;
				}
				else
				{
					return ""; // to make objectlistview to render nothing
				}
			}
		}
		public int map_line_width_0
		{
			set
			{
				the_map_line_width_0 = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_line_width_0;
				}
				else
				{
					return -1; // to make objectlistview to render nothing
				}
			}
		}
		public string map_line_color_1
		{
			set
			{
				the_map_line_color_1 = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_line_color_1;
				}
				else
				{
					return ""; // to make objectlistview to render nothing
				}
			}
		}
		public int map_line_width_1
		{
			set
			{
				the_map_line_width_1 = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_line_width_1;
				}
				else
				{
					return -1; // to make objectlistview to render nothing
				}
			}
		}
		public double map_initial_center_latitude
		{
			set
			{
				the_map_initial_center_latitude = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_initial_center_latitude;
				}
				else
				{
					return -1001.0; // to make objectlistview to render nothing
				}
			}
		}
		public double map_initial_center_longitude
		{
			set
			{
				the_map_initial_center_longitude = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_initial_center_longitude;
				}
				else
				{
					return -1001.0; // to make objectlistview to render nothing
				}
			}
		}
		public double map_initial_zoom_level
		{
			set
			{
				the_map_initial_zoom_level = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_initial_zoom_level;
				}
				else
				{
					return -1; // to make objectlistview to render nothing
				}
			}
		}
		public int map_zoom_minimum
		{
			set
			{
				the_map_zoom_minimum = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_zoom_minimum;
				}
				else
				{
					return -1; // to make objectlistview to render nothing
				}
			}
		}
		public int map_zoom_maximum
		{
			set
			{
				the_map_zoom_maximum = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_zoom_maximum;
				}
				else
				{
					return -1; // to make objectlistview to render nothing
				}
			}
		}
		public bool map_center_current_location
		{
			set
			{
				the_map_center_current_location = value;
			}
			get
			{
				if (window_type == "map")
				{
					return the_map_center_current_location;
				}
				else
				{
					return false;
					// this is the only one that can't be made showing blank for
					// window_type "map", because this is a checkbox column.
					// in this case we just show a checkbox with not check mark in it.
				}
			}
		}

		// This is to help manage in-place editing of the record.
		// This is not necessary if in-place editing is not used.
		public string save_operation { set; get; } //"insert" or "update"

		public the_sub_window the_window { set; get; }
	}

	public class data_table_record_comparer : IComparer<Dictionary<string, string>>
	{
		public int Compare(Dictionary<string, string> x, Dictionary<string, string> y)
		{
			int compare_logmeter_0 = 0;
			int compare_logmeter_1 = 0;
			if ((x["logmeter_0"].Length > 0) && (y["logmeter_0"].Length > 0))
			{
				try
				{
					compare_logmeter_0 = Convert.ToDouble(x["logmeter_0"]).CompareTo(Convert.ToDouble(y["logmeter_0"]));
				}
				catch { }
			}
			if (compare_logmeter_0 == 0)
			{
				if ((x["logmeter_1"].Length > 0) && (y["logmeter_1"].Length > 0))
				{
					try
					{
						compare_logmeter_1 = Convert.ToDouble(x["logmeter_1"]).CompareTo(Convert.ToDouble(y["logmeter_1"]));
					}
					catch { }
					return compare_logmeter_1;
				}
			}
			return compare_logmeter_0;
		}
	}
}
