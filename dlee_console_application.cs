using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace dlee_console_application
{
	class console_visibility
	{
		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		static bool b_visible;
		static public bool visible
		{
			get
			{
				return b_visible;
			}
			set
			{
				try
				{
					b_visible = value;
					string str_old_title = Console.Title;
					DateTime dt_now = DateTime.Now;
					string str_new_title = "dlee_console" + dt_now.ToString("yyyyMMddHHmmssfff");

					//try a few times before give up
					for (int i = 0; i < 10; i++)
					{
						//use this title so that it is almost guaranteed unique
						Console.Title = str_new_title;
						System.Threading.Thread.Sleep(10);
						IntPtr hWnd = FindWindow(null, str_new_title);
						if (hWnd != IntPtr.Zero)
						{
							ShowWindow(hWnd, b_visible ? 1 : 0);
							break;
						}
					}

					//restore the old title
					Console.Title = str_old_title;
				}
				catch (Exception exc)
				{
					Console.WriteLine(exc.Message);
				}
			}
		}
	}
}
