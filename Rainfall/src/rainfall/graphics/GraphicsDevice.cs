using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rainfall
{
	public enum BlendState
	{
		Default,
		Alpha,
		Additive,
	}

	public enum DepthTest
	{
		None,
		Less,
		LEqual,
		Equal,
		GEqual,
		Greater,
		NotEqual,
		Never,
		Always,
	}

	public enum CullState
	{
		None,
		ClockWise,
		CounterClockWise,

		Count
	}

	public class GraphicsDevice
	{
		const ushort INVALID_HANDLE = 0xffff;


		internal int currentPass = 0;


		public unsafe void* allocNativeMemory(int size)
		{
			return Native.Graphics.Graphics_AllocateNativeMemory(size);
		}

		public unsafe void freeNativeMemory(void* ptr)
		{
			Native.Graphics.Graphics_FreeNativeMemory(ptr);
		}

		public VideoMemory createVideoMemory(int size)
		{
			IntPtr dataPtr = Native.Graphics.Graphics_AllocateVideoMemory(size, out IntPtr memoryHandle);
			return new VideoMemory(memoryHandle, dataPtr, size);
		}

		public unsafe VideoMemory createVideoMemory(void* data, int length)
		{
			IntPtr dstPtr = Native.Graphics.Graphics_AllocateVideoMemory(length, out IntPtr memoryHandle);
			Unsafe.CopyBlock((void*)dstPtr, data, (uint)length);

			return new VideoMemory(memoryHandle, dstPtr, length);
		}

		public VideoMemory createVideoMemory(Span<byte> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
					return createVideoMemory(dataPtr, sizeof(byte) * data.Length);
			}
		}

		public VideoMemory createVideoMemory(Span<int> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
					return createVideoMemory(dataPtr, sizeof(int) * data.Length);
			}
		}

		public VideoMemory createVideoMemory(Span<short> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
					return createVideoMemory(dataPtr, sizeof(short) * data.Length);
			}
		}

		public VideoMemory createVideoMemory(Span<float> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
					return createVideoMemory(dataPtr, sizeof(float) * data.Length);
			}
		}

		public VideoMemory createVideoMemory(Span<Half> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
					return createVideoMemory(dataPtr, sizeof(Half) * data.Length);
			}
		}

		public VideoMemory createVideoMemory(Span<Vector2> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
					return createVideoMemory(dataPtr, sizeof(Vector2) * data.Length);
			}
		}

		public VideoMemory createVideoMemory(Span<Vector3> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
					return createVideoMemory(dataPtr, sizeof(Vector3) * data.Length);
			}
		}

		public VideoMemory createVideoMemory(Span<Vector4> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
					return createVideoMemory(dataPtr, sizeof(Vector4) * data.Length);
			}
		}

		public VideoMemory createVideoMemory(Span<Matrix> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
					return createVideoMemory(dataPtr, sizeof(Matrix) * data.Length);
			}
		}

		public unsafe VideoMemory createVideoMemoryReference(void* data, int length)
		{
			Native.Graphics.Graphics_CreateVideoMemoryRef(length, data, null, out IntPtr memoryHandle);
			return new VideoMemory(memoryHandle, (IntPtr)data, length);
		}

		public VertexBuffer createVertexBuffer(VideoMemory memory, Span<VertexElement> layout, BufferFlags flags = BufferFlags.None)
		{
			unsafe
			{
				fixed (VertexElement* layoutPtr = layout)
				{
					ushort handle = Native.Graphics.Graphics_CreateVertexBuffer(memory.memoryHandle, layoutPtr, layout.Length, flags);
					return new VertexBuffer(handle);
				}
			}
		}

		public void destroyVertexBuffer(VertexBuffer buffer)
		{
			Native.Graphics.Graphics_DestroyVertexBuffer(buffer.handle);
		}

		public DynamicVertexBuffer createDynamicVertexBuffer(Span<VertexElement> layout, int vertexCount, BufferFlags flags = BufferFlags.None)
		{
			unsafe
			{
				fixed (VertexElement* layoutPtr = layout)
				{
					ushort handle = Native.Graphics.Graphics_CreateDynamicVertexBuffer(layoutPtr, layout.Length, vertexCount, flags);
					return new DynamicVertexBuffer(handle);
				}
			}
		}

		public void destroyDynamicVertexBuffer(DynamicVertexBuffer buffer)
		{
			Native.Graphics.Graphics_DestroyDynamicVertexBuffer(buffer.handle);
		}

		public TransientVertexBuffer createTransientVertexBuffer(Span<VertexElement> layout, int vertexCount)
		{
			unsafe
			{
				fixed (VertexElement* layoutPtr = layout)
				{
					if (Native.Graphics.Graphics_CreateTransientVertexBuffer(layoutPtr, layout.Length, vertexCount, out Native.TransientVertexBufferData buffer) != 0)
						return new TransientVertexBuffer(buffer);
					return null;
				}
			}
		}

		public IndexBuffer createIndexBuffer(VideoMemory memory, BufferFlags flags = BufferFlags.None)
		{
			ushort handle = Native.Graphics.Graphics_CreateIndexBuffer(memory.memoryHandle, flags);
			return new IndexBuffer(handle);
		}

		public void destroyIndexBuffer(IndexBuffer buffer)
		{
			Native.Graphics.Graphics_DestroyIndexBuffer(buffer.handle);
		}

		public DynamicIndexBuffer createDynamicIndexBuffer(int indexCount, BufferFlags flags = BufferFlags.None)
		{
			ushort handle = Native.Graphics.Graphics_CreateDynamicIndexBuffer(indexCount, flags);
			if (handle != ushort.MaxValue)
				return new DynamicIndexBuffer(handle);
			return null;
		}

		public void destroyDynamicIndexBuffer(DynamicIndexBuffer buffer)
		{
			Native.Graphics.Graphics_DestroyDynamicIndexBuffer(buffer.handle);
		}

		public TransientIndexBuffer createTransientIndexBuffer(int indexCount, bool index32)
		{
			Native.TransientIndexBufferData data = Native.Graphics.Graphics_CreateTransientIndexBuffer(indexCount, index32);
			if (data.data != IntPtr.Zero)
				return new TransientIndexBuffer(data);
			return null;
		}

		public bool createInstanceBuffer(int count, int stride, out InstanceBufferData buffer)
		{
			return Native.Graphics.Graphics_CreateInstanceBuffer(count, stride, out buffer) != 0;
		}

		public Texture createTexture(int width, int height, TextureFormat format, VideoMemory memory, ulong flags = 0)
		{
			ushort handle = Native.Graphics.Graphics_CreateTextureImmutable(width, height, format, flags, memory.memoryHandle, out TextureInfo info);
			if (handle != ushort.MaxValue)
				return new Texture(handle, info);
			return null;
		}

		public Texture createTexture(int width, int height, TextureFormat format, ulong flags = 0)
		{
			ushort handle = Native.Graphics.Graphics_CreateTextureMutable(width, height, format, flags, out TextureInfo info);
			if (handle != ushort.MaxValue)
				return new Texture(handle, info);
			return null;
		}

		public Texture createTexture(BackbufferRatio ratio, bool hasMips, TextureFormat format, ulong flags = 0)
		{
			ushort handle = Native.Graphics.Graphics_CreateTextureMutableR(ratio, (byte)(hasMips ? 1 : 0), format, flags, out TextureInfo info);
			if (handle != ushort.MaxValue)
				return new Texture(handle, info);
			return null;
		}

		public void setTextureData(Texture texture, int x, int y, int width, int height, VideoMemory memory)
		{
			Native.Graphics.Graphics_SetTextureData(texture.handle, x, y, width, height, memory.memoryHandle);
		}

		public void setTextureData(Texture texture, int x, int y, int width, int height, Span<uint> data)
		{
			unsafe
			{
				Debug.Assert(data.Length >= width * height);
				Debug.Assert(texture.info.format == TextureFormat.RGBA8 || texture.info.format == TextureFormat.BGRA8);
				fixed (void* dataPtr = data)
				{
					IntPtr copyPtr = Native.Graphics.Graphics_AllocateVideoMemory(sizeof(uint) * data.Length, out IntPtr memoryHandle);
					Unsafe.CopyBlock((void*)copyPtr, dataPtr, (uint)(sizeof(uint) * data.Length));
					//Native.Graphics.Graphics_CreateVideoMemoryRef(sizeof(uint) * data.Length, dataPtr, null, out IntPtr memoryHandle);
					Native.Graphics.Graphics_SetTextureData(texture.handle, x, y, width, height, memoryHandle);
				}
			}
		}

		public void setTextureData(Texture texture, int x, int y, int width, int height, Span<float> data)
		{
			unsafe
			{
				Debug.Assert(data.Length >= width * height);
				Debug.Assert(texture.info.format == TextureFormat.R32F);
				fixed (void* dataPtr = data)
				{
					Native.Graphics.Graphics_CreateVideoMemoryRef(sizeof(float) * data.Length, dataPtr, null, out IntPtr memoryHandle);
					Native.Graphics.Graphics_SetTextureData(texture.handle, x, y, width, height, memoryHandle);
				}
			}
		}

		public void setTextureData(Texture texture, int x, int y, int width, int height, Span<Vector4> data)
		{
			unsafe
			{
				Debug.Assert(data.Length >= width * height);
				Debug.Assert(texture.info.format == TextureFormat.RGBA32F);
				fixed (void* dataPtr = data)
				{
					Native.Graphics.Graphics_CreateVideoMemoryRef(sizeof(Vector4) * data.Length, dataPtr, null, out IntPtr memoryHandle);
					Native.Graphics.Graphics_SetTextureData(texture.handle, x, y, width, height, memoryHandle);
				}
			}
		}

		/*
		public void setTextureData<T>(Texture texture, int x, int y, int width, int height, T[] data) where T : struct
		{
			unsafe
			{
				Debug.Assert(data.Length == width * height);
				fixed (void* dataPtr = data)
				{
					Native.Graphics.Graphics_CreateVideoMemoryRef(Unsafe.SizeOf<T>() * data.Length, dataPtr, out IntPtr memoryHandle);
					Native.Graphics.Graphics_SetTextureData(texture.handle, x, y, width, height, memoryHandle);
				}
				GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
				IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
				Native.Graphics.Graphics_CreateVideoMemoryRef(Unsafe.SizeOf<T>() * data.Length, (void*)dataPtr, out IntPtr memoryHandle);
				Native.Graphics.Graphics_SetTextureData(texture.handle, x, y, width, height, memoryHandle);
				dataHandle.Free();
			}
		}
		*/

		public Texture createTexture(int width, int height, int depth, TextureFormat format, VideoMemory memory, ulong flags = 0)
		{
			ushort handle = Native.Graphics.Graphics_CreateTexture3DImmutable(width, height, depth, format, flags, memory.memoryHandle, out TextureInfo info);
			if (handle != ushort.MaxValue)
				return new Texture(handle, info);
			return null;
		}

		public Texture createTexture(int width, int height, int depth, bool hasMips, TextureFormat format, ulong flags = 0)
		{
			ushort handle = Native.Graphics.Graphics_CreateTexture3DMutable(width, height, depth, (byte)(hasMips ? 1 : 0), format, flags, out TextureInfo info);
			if (handle != ushort.MaxValue)
				return new Texture(handle, info);
			return null;
		}

		public void setTextureData(Texture texture, int mip, int x, int y, int z, int width, int height, int depth, VideoMemory memory)
		{
			Native.Graphics.Graphics_SetTexture3DData(texture.handle, mip, x, y, z, width, height, depth, memory.memoryHandle);
		}

		public void setTextureData(Texture texture, int mip, int x, int y, int z, int width, int height, int depth, Span<byte> data)
		{
			unsafe
			{
				fixed (void* dataPtr = data)
				{
					Native.Graphics.Graphics_CreateVideoMemoryRef(data.Length, dataPtr, null, out IntPtr memoryHandle);
					Native.Graphics.Graphics_SetTexture3DData(texture.handle, mip, x, y, z, width, height, depth, memoryHandle);
				}
			}
		}

		public void setTextureData(Texture texture, int mip, int x, int y, int z, int width, int height, int depth, Span<ushort> data)
		{
			unsafe
			{
				Debug.Assert(data.Length >= width * height * depth);
				Debug.Assert(texture.info.format >= TextureFormat.R16 && texture.info.format <= TextureFormat.R16S ||
					texture.info.format >= TextureFormat.RG8 && texture.info.format <= TextureFormat.RG8S);
				fixed (void* dataPtr = data)
				{
					Native.Graphics.Graphics_CreateVideoMemoryRef(sizeof(ushort) * data.Length, dataPtr, null, out IntPtr memoryHandle);
					Native.Graphics.Graphics_SetTexture3DData(texture.handle, mip, x, y, z, width, height, depth, memoryHandle);
				}
			}
		}

		public void setTextureData(Texture texture, int mip, int x, int y, int z, int width, int height, int depth, Span<uint> data)
		{
			unsafe
			{
				Debug.Assert(data.Length >= width * height * depth);
				Debug.Assert(texture.info.format == TextureFormat.RGBA8 || texture.info.format == TextureFormat.BGRA8);
				fixed (void* dataPtr = data)
				{
					Native.Graphics.Graphics_CreateVideoMemoryRef(sizeof(uint) * data.Length, dataPtr, null, out IntPtr memoryHandle);
					Native.Graphics.Graphics_SetTexture3DData(texture.handle, mip, x, y, z, width, height, depth, memoryHandle);
				}
			}
		}

		public void setTextureData(Texture texture, int mip, int x, int y, int z, int width, int height, int depth, Span<float> data)
		{
			unsafe
			{
				Debug.Assert(data.Length >= width * height * depth);
				Debug.Assert(texture.info.format == TextureFormat.R32F);
				fixed (void* dataPtr = data)
				{
					Native.Graphics.Graphics_CreateVideoMemoryRef(sizeof(float) * data.Length, dataPtr, null, out IntPtr memoryHandle);
					Native.Graphics.Graphics_SetTexture3DData(texture.handle, mip, x, y, z, width, height, depth, memoryHandle);
				}
			}
		}

		public void setTextureData(Texture texture, int mip, int x, int y, int z, int width, int height, int depth, Span<Vector4> data)
		{
			unsafe
			{
				Debug.Assert(data.Length >= width * height * depth);
				Debug.Assert(texture.info.format == TextureFormat.RGBA32F);
				fixed (void* dataPtr = data)
				{
					Native.Graphics.Graphics_CreateVideoMemoryRef(sizeof(Vector4) * data.Length, dataPtr, null, out IntPtr memoryHandle);
					Native.Graphics.Graphics_SetTexture3DData(texture.handle, mip, x, y, z, width, height, depth, memoryHandle);
				}
			}
		}

		public void destroyTexture(Texture texture)
		{
			Native.Graphics.Graphics_DestroyTexture(texture.handle);
		}

		public Cubemap createCubemap(int size, TextureFormat format, ulong flags = 0)
		{
			ushort handle = Native.Graphics.Graphics_CreateCubemap(size, format, flags, out TextureInfo info);
			if (handle != ushort.MaxValue)
				return new Cubemap(handle, info);
			return null;
		}

		public void destroyCubemap(Cubemap cubemap)
		{
			Native.Graphics.Graphics_DestroyTexture(cubemap.handle);
		}

		public RenderTarget createRenderTarget(RenderTargetAttachment[] attachments)
		{
			int numAttachments = attachments.Length;
			ushort[] textureIDs = new ushort[numAttachments];
			TextureInfo[] textureInfos = new TextureInfo[numAttachments];
			Texture[] textures = new Texture[numAttachments];
			ushort handle = Native.Graphics.Graphics_CreateRenderTarget(numAttachments, ref attachments[0], ref textureInfos[0], textureIDs);

			if (handle != ushort.MaxValue)
			{
				bool hasRGB = false, hasDepth = false;
				for (int i = 0; i < numAttachments; i++)
				{
					textures[i] = new Texture(textureIDs[i], textureInfos[i]);
					if (attachments[i].format >= TextureFormat.D16F && attachments[i].format <= TextureFormat.D0S8)
						hasDepth = true;
					else
						hasRGB = true;
				}

				return new RenderTarget(handle, attachments, textures, hasRGB, hasDepth);
			}

			return null;
		}

		public void destroyRenderTarget(RenderTarget renderTarget)
		{
			Native.Graphics.Graphics_DestroyRenderTarget(renderTarget.handle);
		}

		public void destroyShader(Shader shader)
		{
			Native.Graphics.Graphics_DestroyShader(shader.handle);
		}


		public void resetState()
		{
			Native.Graphics.Graphics_ResetState();
		}

		public void setBlendState(BlendState blendState)
		{
			Native.Graphics.Graphics_SetBlendState(blendState);
		}

		public void setDepthTest(DepthTest depthTest)
		{
			Native.Graphics.Graphics_SetDepthTest(depthTest);
		}

		public void setCullState(CullState cullState)
		{
			Native.Graphics.Graphics_SetCullState(cullState);
		}

		public void setVertexBuffer(ushort handle)
		{
			Native.Graphics.Graphics_SetVertexBuffer(handle);
		}

		public void setVertexBuffer(VertexBuffer buffer)
		{
			Native.Graphics.Graphics_SetVertexBuffer(buffer != null ? buffer.handle : INVALID_HANDLE);
		}

		public void setVertexBuffer(DynamicVertexBuffer buffer)
		{
			Native.Graphics.Graphics_SetDynamicVertexBuffer(buffer != null ? buffer.handle : INVALID_HANDLE);
		}

		public void setVertexBuffer(TransientVertexBuffer buffer)
		{
			if (buffer != null)
				Native.Graphics.Graphics_SetTransientVertexBuffer(ref buffer.data);
			else
				Native.Graphics.Graphics_SetTransientVertexBuffer(IntPtr.Zero);
		}

		public void setIndexBuffer(ushort handle)
		{
			Native.Graphics.Graphics_SetIndexBuffer(handle);
		}

		public void setIndexBuffer(IndexBuffer buffer)
		{
			Native.Graphics.Graphics_SetIndexBuffer(buffer != null ? buffer.handle : INVALID_HANDLE);
		}

		public void setIndexBuffer(DynamicIndexBuffer buffer)
		{
			Native.Graphics.Graphics_SetDynamicIndexBuffer(buffer != null ? buffer.handle : INVALID_HANDLE);
		}

		public void setIndexBuffer(TransientIndexBuffer buffer)
		{
			if (buffer != null)
				Native.Graphics.Graphics_SetTransientIndexBuffer(ref buffer.data);
			else
				Native.Graphics.Graphics_SetTransientIndexBuffer(IntPtr.Zero);
		}

		public void setInstanceBuffer(InstanceBufferData buffer)
		{
			Native.Graphics.Graphics_SetInstanceBuffer(ref buffer);
		}

		public void setInstanceBuffer(InstanceBufferData buffer, int offset, int count)
		{
			Native.Graphics.Graphics_SetInstanceBufferN(ref buffer, offset, count);
		}

		public void setComputeBuffer(int stage, VertexBuffer buffer, ComputeAccess access)
		{
			Native.Graphics.Graphics_SetComputeBuffer(stage, buffer.handle, access);
		}

		/*
		public void setUniform<T>(ushort handle, T value, int num = 1)
		{
			GCHandle dataHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
			Native.Graphics.Graphics_SetUniform(handle, dataHandle.AddrOfPinnedObject(), num);
			dataHandle.Free();
		}

		public void setUniform<T>(ushort handle, Span<T> values) where T : struct
		{
			unsafe
			{
				fixed (T* data = values)
					Native.Graphics.Graphics_SetUniform(handle, (IntPtr)data, values.Length);
			}
		}
		*/

		public void setUniform(ushort handle, Vector4 v)
		{
			unsafe
			{
				Native.Graphics.Graphics_SetUniform(handle, &v, 1);
			}
		}

		public void setUniform(ushort handle, Vector3 v)
		{
			unsafe
			{
				Vector4 v4 = new Vector4(v, 0.0f);
				Native.Graphics.Graphics_SetUniform(handle, &v4, 1);
			}
		}

		public void setUniform(ushort handle, Span<Vector4> v)
		{
			unsafe
			{
				fixed (Vector4* data = v)
					Native.Graphics.Graphics_SetUniform(handle, data, v.Length);
			}
		}

		public void setUniform(ushort handle, Matrix m)
		{
			unsafe
			{
				Native.Graphics.Graphics_SetUniform(handle, &m, 1);
			}
		}

		public void setUniform(ushort handle, Span<Matrix> m)
		{
			unsafe
			{
				fixed (Matrix* data = m)
					Native.Graphics.Graphics_SetUniform(handle, data, m.Length);
			}
		}

		public void setUniform(Shader shader, byte[] name, Vector4 v)
		{
			setUniform(shader.getUniform(name, UniformType.Vector4), v);
		}

		public void setUniform(Shader shader, Span<byte> name, Vector4 v)
		{
			setUniform(shader.getUniform(name, UniformType.Vector4), v);
		}

		public void setUniform(Shader shader, string name, Vector4 v)
		{
			setUniform(shader.getUniform(name, UniformType.Vector4), v);
		}

		public void setUniform(Shader shader, string name, Vector3 v)
		{
			setUniform(shader.getUniform(name, UniformType.Vector4), v);
		}

		public void setUniform(Shader shader, byte[] name, Matrix m)
		{
			setUniform(shader.getUniform(name, UniformType.Matrix4), m);
		}

		public void setUniform(Shader shader, Span<byte> name, Matrix m)
		{
			setUniform(shader.getUniform(name, UniformType.Matrix4), m);
		}

		public void setUniform(Shader shader, string name, Matrix m)
		{
			setUniform(shader.getUniform(name, UniformType.Matrix4), m);
		}

		public void setTexture(ushort sampler, int unit, Texture texture, uint flags = uint.MaxValue)
		{
			Native.Graphics.Graphics_SetTexture(sampler, unit, texture.handle, flags);
		}

		public void setTexture(Shader shader, byte[] name, int unit, Texture texture, uint flags = uint.MaxValue)
		{
			Native.Graphics.Graphics_SetTexture(shader.getUniform(name, UniformType.Sampler), unit, texture.handle, flags);
		}

		public void setTexture(Shader shader, Span<byte> name, int unit, Texture texture, uint flags = uint.MaxValue)
		{
			Native.Graphics.Graphics_SetTexture(shader.getUniform(name, UniformType.Sampler), unit, texture.handle, flags);
		}

		public void setTexture(Shader shader, string name, int unit, Texture texture, uint flags = uint.MaxValue)
		{
			Native.Graphics.Graphics_SetTexture(shader.getUniform(name, UniformType.Sampler), unit, texture.handle, flags);
		}

		public void setTexture(ushort sampler, int unit, Cubemap texture, uint flags = uint.MaxValue)
		{
			Native.Graphics.Graphics_SetTexture(sampler, unit, texture.handle, flags);
		}

		public void setTexture(Shader shader, byte[] name, int unit, Cubemap texture, uint flags = uint.MaxValue)
		{
			Native.Graphics.Graphics_SetTexture(shader.getUniform(name, UniformType.Sampler), unit, texture.handle, flags);
		}

		public void setTexture(Shader shader, Span<byte> name, int unit, Cubemap texture, uint flags = uint.MaxValue)
		{
			Native.Graphics.Graphics_SetTexture(shader.getUniform(name, UniformType.Sampler), unit, texture.handle, flags);
		}

		public void setTexture(Shader shader, string name, int unit, Cubemap texture, uint flags = uint.MaxValue)
		{
			Native.Graphics.Graphics_SetTexture(shader.getUniform(name, UniformType.Sampler), unit, texture.handle, flags);
		}

		public void setComputeTexture(int stage, Texture texture, int mip, ComputeAccess access)
		{
			Native.Graphics.Graphics_SetComputeTexture(stage, texture.handle, mip, access);
		}

		public void setPass(int pass)
		{
			currentPass = pass;
		}

		public void nextPass()
		{
			currentPass++;
		}

		public void setRenderTarget(RenderTarget renderTarget, uint rgba = 0x0, float depth = 1.0f)
		{
			if (renderTarget != null)
			{
				if (renderTarget.ratio != BackbufferRatio.Count)
					Native.Graphics.Graphics_SetRenderTargetR(currentPass, renderTarget.handle, renderTarget.ratio, renderTarget.hasRGB ? (byte)1 : (byte)0, renderTarget.hasDepth ? (byte)1 : (byte)0, rgba, depth);
				else
					Native.Graphics.Graphics_SetRenderTarget(currentPass, renderTarget.handle, renderTarget.width, renderTarget.height, renderTarget.hasRGB ? (byte)1 : (byte)0, renderTarget.hasDepth ? (byte)1 : (byte)0, rgba, depth);
			}
			else
			{
				Native.Graphics.Graphics_SetRenderTargetR(currentPass, INVALID_HANDLE, BackbufferRatio.Equal, 1, 1, rgba, depth);
			}
		}

		public void setTransform(Matrix transform)
		{
			Native.Graphics.Graphics_SetTransform(currentPass, ref transform);
		}

		public void setViewTransform(Matrix projection, Matrix view)
		{
			Native.Graphics.Graphics_SetViewTransform(currentPass, ref projection, ref view);
		}

		public void draw(Shader shader)
		{
			Native.Graphics.Graphics_Draw(currentPass, shader.handle);
		}

		public unsafe void drawText(int x, int y, float z, float scale, byte* text, int offset, int count, Font font, uint color, SpriteBatch batch)
		{
			Native.Graphics.Graphics_DrawText(currentPass, x, y, z, scale, text, offset, count, font.handle, color, batch.handle);
		}

		public unsafe void drawText(int x, int y, float z, float scale, byte* text, int length, Font font, uint color, SpriteBatch batch)
		{
			drawText(x, y, z, scale, text, 0, length, font, color, batch);
		}

		public unsafe void drawText(int x, int y, float z, float scale, Span<byte> text, int offset, int count, Font font, uint color, SpriteBatch batch)
		{
			fixed (byte* textPtr = text)
				Native.Graphics.Graphics_DrawText(currentPass, x, y, z, scale, textPtr, offset, count, font.handle, color, batch.handle);
		}

		public void drawText(int x, int y, float z, float scale, Span<byte> text, int length, Font font, uint color, SpriteBatch batch)
		{
			drawText(x, y, z, scale, text, 0, length, font, color, batch);
		}

		public void drawText(int x, int y, float z, float scale, string text, int offset, int count, Font font, uint color, SpriteBatch batch)
		{
			Native.Graphics.Graphics_DrawText(currentPass, x, y, z, scale, text, offset, count, font.handle, color, batch.handle);
		}

		public void drawText(int x, int y, float z, float scale, string text, Font font, uint color, SpriteBatch batch)
		{
			drawText(x, y, z, scale, text, 0, text.Length, font, color, batch);
		}

		public void computeDispatch(Shader shader, int numX, int numY, int numZ)
		{
			Native.Graphics.Graphics_ComputeDispatch(currentPass, shader.handle, numX, numY, numZ);
		}

		public void blit(Texture dst, Texture src)
		{
			Native.Graphics.Graphics_Blit(currentPass, dst.handle, src.handle);
		}

		public void blit(Texture dst, int dstMip, int dstX, int dstY, int dstZ, Texture src, int srcMip, int srcX, int srcY, int srcZ, int width, int height, int depth)
		{
			Native.Graphics.Graphics_BlitEx(currentPass, dst.handle, dstMip, dstX, dstY, dstZ, src.handle, srcMip, srcX, srcY, srcZ, width, height, depth);
		}

		public void completeFrame()
		{
			Native.Graphics.Graphics_CompleteFrame();
		}

		public RenderStats getRenderStats()
		{
			RenderStats stats;
			Native.Graphics.Graphics_GetRenderStats(out stats);
			return stats;
		}
	}
}
