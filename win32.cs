using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace dlee_win32
{
	class win32
	{
		[DllImport("User32.dll")]
		public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey); // Keys enumeration

		[DllImport("User32.dll")]
		public static extern short GetAsyncKeyState(System.Int32 vKey);

	}
}
