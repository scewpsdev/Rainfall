using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public enum NFDResult
	{
		NFD_ERROR,       /* programmatic error */
		NFD_OKAY,        /* user pressed okay, or successful return */
		NFD_CANCEL       /* user pressed cancel */
	}

	public static class NFD
	{
		/* single file open dialog */
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe NFDResult NFD_OpenDialog(string filterList, string defaultPath, byte** outPath);

		/* save dialog */
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe NFDResult NFD_SaveDialog(string filterList, string defaultPath, byte** outPath);

		/* select folder dialog */
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe NFDResult NFD_PickFolder(string defaultPath, byte** outPath);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void NFDi_Free(byte* ptr);
	}
}
