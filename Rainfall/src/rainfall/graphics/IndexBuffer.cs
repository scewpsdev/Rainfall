using Rainfall.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class IndexBuffer
	{
		public readonly ushort handle;

		internal IndexBuffer(ushort handle)
		{
			this.handle = handle;
		}
	}

	public class DynamicIndexBuffer
	{
		internal ushort handle;

		internal DynamicIndexBuffer(ushort handle)
		{
			this.handle = handle;
		}
	}

	public class TransientIndexBuffer
	{
		internal TransientIndexBufferData data;

		internal TransientIndexBuffer(TransientIndexBufferData data)
		{
			this.data = data;
		}
	}
}
