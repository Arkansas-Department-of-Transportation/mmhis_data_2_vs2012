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
	public partial class video_frame : the_sub_window
	{
		public System.Windows.Forms.PictureBox pictureBox_frame;
		public System.Windows.Forms.Label label_image_pathname;

		private string the_image_path_replace;
		public string image_path_replace {
			set
			{
				if (value != the_image_path_replace)
				{
					the_image_path_replace = value;
					show_image();
				}
			}
			get
			{
				return the_image_path_replace;
			}
		}
		private string the_image_path_replace_with;
		public string image_path_replace_with
		{
			set
			{
				if (value != the_image_path_replace_with)
				{
					the_image_path_replace_with = value;
					show_image();
				}
			}
			get
			{
				return the_image_path_replace_with;
			}
		}

		public video_frame()
		{
			InitializeComponent();
		}

		public void label_image_pathname_TextChanged(object sender, EventArgs e)
		{
			show_image();
		}

		public void show_image()
		{
			if (Visible)
			{
				try
				{
					if (image_path_replace.Length > 0)
					{
						pictureBox_frame.LoadAsync(
							label_image_pathname.Text.Replace(
								image_path_replace,
								image_path_replace_with
							)
						);
					}
					else
					{
						pictureBox_frame.LoadAsync(label_image_pathname.Text);
					}
					if (!pictureBox_frame.Visible)
					{
						pictureBox_frame.Visible = true;
					}
				}
				catch
				{
					try
					{
						pictureBox_frame.Visible = false;
					}
					catch { }
				}
			}
		}

		private void video_frame_MouseWheel(object sender, MouseEventArgs e)
		{
			int scroll_amount = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
			the_parent_form.change_frame(scroll_amount);
		}
	}
}
