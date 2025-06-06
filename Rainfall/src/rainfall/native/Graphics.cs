using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public enum BufferFlags : UInt16
	{
		None = 0x0000,
		ComputeRead = 0x0100,
		ComputeWrite = 0x0200,
		DrawIndirect = 0x0400,
		AllowResize = 0x0800,
		Index32 = 0x1000,
	}

	public enum ComputeAccess : int
	{
		Read,      //!< Read
		Write,     //!< Write
		ReadWrite, //!< Read and write

		Count
	}

	public enum ViewMode : int
	{
		Default,         //!< Default sort order.
		Sequential,      //!< Sort in the same order in which submit calls were called.
		DepthAscending,  //!< Sort draw call depth in ascending order.
		DepthDescending, //!< Sort draw call depth in descending order.

		Count
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct RenderStats
	{
		public long cpuTimeFrame;               //!< CPU time between two `bgfx::frame` calls.
		public long cpuTimeBegin;               //!< Render thread CPU submit begin time.
		public long cpuTimeEnd;                 //!< Render thread CPU submit end time.
		public long cpuTimerFreq;               //!< CPU timer frequency. Timestamps-per-second

		public long gpuTimeBegin;               //!< GPU frame begin time.
		public long gpuTimeEnd;                 //!< GPU frame end time.
		public long gpuTimerFreq;               //!< GPU timer frequency.

		long waitRender;                 //!< Time spent waiting for render backend thread to finish issuing
										 //!  draw commands to underlying graphics API.
		long waitSubmit;                 //!< Time spent waiting for submit thread to advance to next frame.

		public uint numDraw;                   //!< Number of draw calls submitted.
		public uint numCompute;                //!< Number of compute calls submitted.
		public uint numBlit;                   //!< Number of blit calls submitted.
		public uint maxGpuLatency;             //!< GPU driver latency.
		public uint gpuFrameNum;

		ushort _numDynamicIndexBuffers;    //!< Number of used dynamic index buffers.
		ushort _numDynamicVertexBuffers;   //!< Number of used dynamic vertex buffers.
		ushort _numFrameBuffers;           //!< Number of used frame buffers.
		ushort _numIndexBuffers;           //!< Number of used index buffers.
		ushort _numOcclusionQueries;       //!< Number of used occlusion queries.
		ushort _numPrograms;               //!< Number of used programs.
		ushort _numShaders;                //!< Number of used shaders.
		ushort _numTextures;               //!< Number of used textures.
		ushort _numUniforms;               //!< Number of used uniforms.
		ushort _numVertexBuffers;          //!< Number of used vertex buffers.
		ushort _numVertexLayouts;          //!< Number of used vertex layouts.

		public long textureMemoryUsed;          //!< Estimate of texture memory used.
		public long rtMemoryUsed;               //!< Estimate of render target memory used.
		public int transientVbUsed;            //!< Amount of transient vertex buffer used.
		public int transientIbUsed;            //!< Amount of transient index buffer used.

		fixed uint numPrims[5]; //!< Number of primitives rendered.

		public long gpuMemoryMax;               //!< Maximum available GPU memory for application.
		public long gpuMemoryUsed;              //!< Amount of GPU memory used by the application.

		ushort width;                     //!< Backbuffer width in pixels.
		ushort height;                    //!< Backbuffer height in pixels.
		ushort textWidth;                 //!< Debug text width in characters.
		ushort textHeight;                //!< Debug text height in characters.

		ushort numViews;                //!< Number of view stats.
		ViewStats* viewStats;               //!< Array of View stats.

		byte numEncoders;          //!< Number of encoders used during frame.
		IntPtr encoderStats;         //!< Array of encoder stats.

		public float cpuTime { get => (cpuTimeEnd - cpuTimeBegin) / (float)cpuTimerFreq; }
		public float gpuTime { get => (gpuTimeEnd - gpuTimeBegin) / (float)gpuTimerFreq; }
		public int numRenderTargets { get => _numFrameBuffers; }
		public int numShaders { get => _numPrograms; }
		public int numTextures { get => _numTextures; }
		public int numTriangles { get => (int)numPrims[0]; }

		public float getCpuTime(ushort view)
		{
			for (int i = 0; i < numViews; i++)
			{
				if (viewStats[i].view == view)
				{
					return (viewStats[i].cpuTimeEnd - viewStats[i].cpuTimeBegin) / (float)cpuTimerFreq;
				}
			}
			return 0.0f;
		}

		public float getGpuTime(ushort view)
		{
			for (int i = 0; i < numViews; i++)
			{
				if (viewStats[i].view == view)
				{
					return (viewStats[i].gpuTimeEnd - viewStats[i].gpuTimeBegin) / (float)gpuTimerFreq;
				}
			}
			return 0.0f;
		}

		public float getCumulativeGpuTime(ushort view, int count)
		{
			float result = 0.0f;
			for (ushort v = view; v < view + count; v++)
				result += getGpuTime(v);
			return result;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ViewStats
	{
		internal fixed byte name[256];      //!< View name.
		internal ushort view;           //!< View id.
		internal long cpuTimeBegin;   //!< CPU (submit) begin time.
		internal long cpuTimeEnd;     //!< CPU (submit) end time.
		internal long gpuTimeBegin;   //!< GPU begin time.
		internal long gpuTimeEnd;     //!< GPU end time.
	}

	namespace Native
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct TransientVertexBufferData
		{
			internal IntPtr data;                      //!< Pointer to data.
			internal UInt32 size;                      //!< Data size.
			internal UInt32 startVertex;               //!< First vertex.
			internal UInt16 stride;                    //!< Vertex stride.
			internal UInt16 handle;                    //!< Vertex buffer handle.
			internal UInt16 layoutHandle;              //!< Vertex layout handle.
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct TransientIndexBufferData
		{
			internal IntPtr data;            //!< Pointer to data.
			internal UInt32 size;            //!< Data size.
			internal UInt32 startIndex;      //!< First index.
			internal UInt16 handle;          //!< Index buffer handle.
			internal byte isIndex16;         //!< Index buffer format is 16-bits if true, otherwise it is 32-bit.
		}


		internal static unsafe class Graphics
		{
			internal unsafe delegate void MemoryReleaseCallback_t(void* ptr, void* userPtr);


			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe void* Graphics_AllocateNativeMemory(int size);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe void Graphics_FreeNativeMemory(void* ptr);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr Graphics_AllocateVideoMemory(int size, out IntPtr memoryHandle);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe IntPtr Graphics_CreateVideoMemoryRef(int size, void* data, MemoryReleaseCallback_t releaseCallback);


			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe ushort Graphics_CreateVertexBuffer(IntPtr memoryHandle, VertexElement* layoutElements, int layoutElementsCount, BufferFlags flags);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_DestroyVertexBuffer(ushort buffer);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateDynamicVertexBuffer(VertexElement* layoutElements, int layoutElementsCount, int vertexCount, BufferFlags flags);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateDynamicVertexBufferFromMemory(IntPtr memoryHandle, VertexElement* layoutElements, int layoutElementsCount, BufferFlags flags);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_DestroyDynamicVertexBuffer(ushort buffer);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_UpdateDynamicVertexBuffer(ushort buffer, int startVertex, IntPtr memoryHandle);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe byte Graphics_CreateTransientVertexBuffer(VertexElement* layoutElements, int layoutElementsCount, int vertexCount, out TransientVertexBufferData buffer);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateIndexBuffer(IntPtr memoryHandle, BufferFlags flags);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_DestroyIndexBuffer(ushort buffer);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateDynamicIndexBuffer(int indexCount, BufferFlags flags);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_DestroyDynamicIndexBuffer(ushort buffer);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern TransientIndexBufferData Graphics_CreateTransientIndexBuffer(int indexCount, bool index32);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern byte Graphics_CreateInstanceBuffer(int count, int stride, out InstanceBufferData buffer);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateTextureMutable(int width, int height, TextureFormat format, ulong flags, out TextureInfo info);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateTextureMutableR(BackbufferRatio ratio, byte hasMips, TextureFormat format, ulong flags, out TextureInfo info);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetTextureData(ushort texture, int x, int y, int width, int height, IntPtr memory);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateTextureImmutable(int width, int height, TextureFormat format, ulong flags, IntPtr memory, out TextureInfo info);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateTextureFromMemory(IntPtr memoryHandle, ulong flags, out TextureInfo info);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateTexture3DImmutable(int width, int height, int depth, TextureFormat format, ulong flags, IntPtr memory, out TextureInfo info);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateTexture3DMutable(int width, int height, int depth, byte hasMips, TextureFormat format, ulong flags, out TextureInfo info);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetTexture3DData(ushort texture, int mip, int x, int y, int z, int width, int height, int depth, IntPtr memory);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateCubemap(int size, TextureFormat format, byte mipmaps, ulong flags, out TextureInfo info);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_DestroyTexture(ushort texture);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr Graphics_CreateShader(IntPtr vertexMemory, IntPtr fragmentMemory);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe ushort Graphics_ShaderGetUniform(IntPtr shader, byte* name, UniformType type, int num);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_ShaderGetUniform(IntPtr shader, [MarshalAs(UnmanagedType.LPStr)] string name, UniformType type, int num);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern ushort Graphics_CreateRenderTarget(int numAttachments, ref RenderTargetAttachment attachmentInfo, [MarshalAs(UnmanagedType.LPArray)] ushort[] textures, ref TextureInfo textureInfos);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_DestroyRenderTarget(ushort renderTarget);


			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_ResetState();

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetBlendState(BlendState blendState);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetDepthTest(DepthTest depthTest);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetDepthWrite(byte write);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetCullState(CullState cullState);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetPrimitiveType(PrimitiveType primitiveType);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetVertexBuffer(ushort handle);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetDynamicVertexBuffer(ushort handle);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetTransientVertexBuffer(ref TransientVertexBufferData buffer);
			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetTransientVertexBuffer(IntPtr buffer);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetIndexBuffer(ushort handle);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetDynamicIndexBuffer(ushort handle);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetTransientIndexBuffer(ref TransientIndexBufferData buffer);
			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetTransientIndexBuffer(IntPtr buffer);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetInstanceBuffer(ref InstanceBufferData buffer);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetInstanceBufferN(ref InstanceBufferData buffer, int offset, int count);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetComputeBuffer(int stage, ushort handle, ComputeAccess access);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe void Graphics_SetUniform(ushort handle, void* value, int num);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetTexture(ushort sampler, int unit, ushort texture, uint flags);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetComputeTexture(int stage, ushort texture, int mip, ComputeAccess access);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetViewMode(int pass, int viewMode);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetRenderTarget(int pass, ushort renderTarget, int width, int height);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetRenderTargetR(int pass, ushort renderTarget, BackbufferRatio ratio);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_ClearRenderTarget(int pass, ushort renderTarget, byte hasRGB, byte hasDepth, uint rgba, float depth);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetTransform(int pass, ref Matrix transform);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_SetViewTransform(int pass, ref Matrix projection, ref Matrix view);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_Draw(int pass, IntPtr shader);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe void Graphics_DrawText(int pass, int x, int y, float z, float scale, int viewportHeight, byte* text, int offset, int count, IntPtr font, uint color, IntPtr batch);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe void Graphics_DrawText(int pass, int x, int y, float z, float scale, int viewportHeight, [MarshalAs(UnmanagedType.LPStr)] string text, int offset, int count, IntPtr font, uint color, IntPtr batch);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern unsafe void Graphics_DrawDebugText(int x, int y, byte color, byte* text);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_DrawDebugText(int x, int y, byte color, [MarshalAs(UnmanagedType.LPStr)] string text);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_GetDebugTextSize(out int width, out int height);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_ComputeDispatch(int pass, IntPtr shader, int numX, int numY, int numZ);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_Blit(int pass, ushort dst, ushort src);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_BlitEx(int pass, ushort dst, int dstMip, int dstX, int dstY, int dstZ, ushort src, int srcMip, int srcX, int srcY, int srcZ, int width, int height, int depth);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_CompleteFrame();

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void Graphics_GetRenderStats(out RenderStats renderStats);

			[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
			internal static extern int Graphics_DrawDebugInfo(int x, int y, byte color);
		}
	}
}
