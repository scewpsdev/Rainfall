using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class VideoMemory
	{
		internal IntPtr memoryHandle;
		internal IntPtr dataPtr;
		internal int dataSize;


		internal VideoMemory(IntPtr memoryHandle, IntPtr dataPtr, int dataSize)
		{
			this.memoryHandle = memoryHandle;
			this.dataPtr = dataPtr;
			this.dataSize = dataSize;
		}

		public unsafe void* data
		{
			get => (void*)dataPtr;
		}

		public int size
		{
			get => dataSize;
		}
	}
}
