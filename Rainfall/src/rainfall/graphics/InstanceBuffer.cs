using Rainfall.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct InstanceBufferData
	{
		public IntPtr data;             //!< Pointer to data.
		public UInt32 size;             //!< Data size.
		public UInt32 offset;           //!< Offset in vertex buffer.
		public UInt32 num;              //!< Number of instances.
		public UInt16 stride;           //!< Vertex buffer stride.
		public UInt16 handle;           //!< Vertex buffer object handle.


		public unsafe T* getData<T>() where T : struct
		{
			void* ptr = (void*)data;
			return (T*)ptr;
		}

		public unsafe void write<T>(T value, int offset = 0)
		{
			GCHandle dataHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
			Unsafe.CopyBlock((byte*)data.ToPointer() + offset, dataHandle.AddrOfPinnedObject().ToPointer(), (uint)Unsafe.SizeOf<T>());
			dataHandle.Free();
		}

		public unsafe void write<T>(T[] values, int offset = 0)
		{
			GCHandle dataHandle = GCHandle.Alloc(values, GCHandleType.Pinned);
			Unsafe.CopyBlock((byte*)data.ToPointer() + offset, dataHandle.AddrOfPinnedObject().ToPointer(), (uint)(Unsafe.SizeOf<T>() * values.Length));
			dataHandle.Free();
		}

		public unsafe void write<T>(Span<T> values, int offset = 0, int count = 0)
		{
			fixed (void* ptr = values)
			{
				if (count == 0)
					count = values.Length;
				Unsafe.CopyBlock((byte*)data.ToPointer() + offset, ptr, (uint)(Unsafe.SizeOf<T>() * count));
			}
			/*
			GCHandle dataHandle = GCHandle.Alloc(values, GCHandleType.Pinned);
			Unsafe.CopyBlock((byte*)data.ToPointer() + offset, dataHandle.AddrOfPinnedObject().ToPointer(), (uint)(Unsafe.SizeOf<T>() * values.Length));
			dataHandle.Free();
			*/
		}
	}
}
