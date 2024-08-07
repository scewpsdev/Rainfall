using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class SpriteBatch
	{
		GraphicsDevice graphics;

		internal IntPtr handle;


		public SpriteBatch(GraphicsDevice graphics)
		{
			this.graphics = graphics;

			handle = Native.SpriteBatch.SpriteBatch_Create();
		}

		public void destroy()
		{
			Native.SpriteBatch.SpriteBatch_Destroy(handle);
		}

		public void begin(int numDrawCommands)
		{
			Native.SpriteBatch.SpriteBatch_Begin(handle, numDrawCommands);
		}

		public void end()
		{
			Native.SpriteBatch.SpriteBatch_End(handle);
		}

		public int getNumDrawCalls()
		{
			return Native.SpriteBatch.SpriteBatch_GetNumDrawCalls(handle);
		}

		public void submitDrawCall(int idx, Shader shader)
		{
			Native.SpriteBatch.SpriteBatch_SubmitDrawCall(handle, idx, graphics.currentPass, shader.handle);
		}

		public void drawVertical(float x, float y, float z, float width, float height, float rotation, Vector2 rotationCenter, Texture texture, uint textureFlags, float u0, float v0, float u1, float v1, bool flipX, bool flipY, Vector4 color, Vector3 normal0, Vector3 normal1, Vector3 normal2, Vector3 normal3)
		{
			float x0 = -rotationCenter.x;
			float y0 = -rotationCenter.y;
			float x1 = -rotationCenter.x + width;
			float y1 = -rotationCenter.y + height;

			Vector3 vertex0 = new Vector3(x, y, z);
			Vector3 vertex1 = new Vector3(x, y, z);
			Vector3 vertex2 = new Vector3(x, y, z);
			Vector3 vertex3 = new Vector3(x, y, z);

			Vector2 offset0 = new Vector2(x0, y0);
			Vector2 offset1 = new Vector2(x1, y0);
			Vector2 offset2 = new Vector2(x1, y1);
			Vector2 offset3 = new Vector2(x0, y1);

			float s = MathF.Sin(rotation);
			float c = MathF.Cos(rotation);
			offset0 = new Vector2(c * offset0.x - s * offset0.y, s * offset0.x + c * offset0.y);
			offset1 = new Vector2(c * offset1.x - s * offset1.y, s * offset1.x + c * offset1.y);
			offset2 = new Vector2(c * offset2.x - s * offset2.y, s * offset2.x + c * offset2.y);
			offset3 = new Vector2(c * offset3.x - s * offset3.y, s * offset3.x + c * offset3.y);

			vertex0.xz += offset0;
			vertex1.xz += offset1;
			vertex2.xz += offset2;
			vertex3.xz += offset3;

			vertex0.xz += rotationCenter;
			vertex1.xz += rotationCenter;
			vertex2.xz += rotationCenter;
			vertex3.xz += rotationCenter;

			if (flipX)
			{
				float tmp = u0;
				u0 = u1;
				u1 = tmp;
			}
			if (flipY)
			{
				float tmp = v0;
				v0 = v1;
				v1 = tmp;
			}

			Vector2 uv0 = new Vector2(u0, v1);
			Vector2 uv1 = new Vector2(u1, v1);
			Vector2 uv2 = new Vector2(u1, v0);
			Vector2 uv3 = new Vector2(u0, v0);

			Native.SpriteBatch.SpriteBatch_Draw(handle, vertex0.x,
				vertex0.y,
				vertex0.z,
				vertex1.x,
				vertex1.y,
				vertex1.z,
				vertex2.x,
				vertex2.y,
				vertex2.z,
				vertex3.x,
				vertex3.y,
				vertex3.z,
				normal0.x,
				normal0.y,
				normal0.z,
				normal1.x,
				normal1.y,
				normal1.z,
				normal2.x,
				normal2.y,
				normal2.z,
				normal3.x,
				normal3.y,
				normal3.z,
				uv0.x,
				uv0.y,
				uv1.x,
				uv1.y,
				uv2.x,
				uv2.y,
				uv3.x,
				uv3.y,
				color.x,
				color.y,
				color.z,
				color.w,
				1.0f,
				texture != null ? texture.handle : ushort.MaxValue, textureFlags);
		}

		public void drawVertical(float x, float y, float z, float width, float height, float rotation, Texture texture, uint textureFlags, float u0, float v0, float u1, float v1, Vector4 color)
		{
			drawVertical(x, y, z, width, height, rotation, new Vector2(width, height) * 0.5f, texture, textureFlags, u0, v0, u1, v1, false, false, color, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero);
		}

		public void draw(float x, float y, float z, float width, float height, float rotation, Vector2 rotationCenter, Texture texture, uint textureFlags, float u0, float v0, float u1, float v1, bool flipX, bool flipY, Vector4 color, float mask, Vector3 normal0, Vector3 normal1, Vector3 normal2, Vector3 normal3)
		{
			float x0 = -rotationCenter.x;
			float y0 = -rotationCenter.y;
			float x1 = -rotationCenter.x + width;
			float y1 = -rotationCenter.y + height;

			Vector3 vertex0 = new Vector3(x, y, z);
			Vector3 vertex1 = new Vector3(x, y, z);
			Vector3 vertex2 = new Vector3(x, y, z);
			Vector3 vertex3 = new Vector3(x, y, z);

			Vector2 offset0 = new Vector2(x0, y0);
			Vector2 offset1 = new Vector2(x1, y0);
			Vector2 offset2 = new Vector2(x1, y1);
			Vector2 offset3 = new Vector2(x0, y1);

			float s = MathF.Sin(rotation);
			float c = MathF.Cos(rotation);
			offset0 = new Vector2(c * offset0.x - s * offset0.y, s * offset0.x + c * offset0.y);
			offset1 = new Vector2(c * offset1.x - s * offset1.y, s * offset1.x + c * offset1.y);
			offset2 = new Vector2(c * offset2.x - s * offset2.y, s * offset2.x + c * offset2.y);
			offset3 = new Vector2(c * offset3.x - s * offset3.y, s * offset3.x + c * offset3.y);

			vertex0.xy += offset0;
			vertex1.xy += offset1;
			vertex2.xy += offset2;
			vertex3.xy += offset3;

			vertex0.xy += rotationCenter;
			vertex1.xy += rotationCenter;
			vertex2.xy += rotationCenter;
			vertex3.xy += rotationCenter;

			if (flipX)
			{
				float tmp = u0;
				u0 = u1;
				u1 = tmp;
			}
			if (flipY)
			{
				float tmp = v0;
				v0 = v1;
				v1 = tmp;
			}

			Vector2 uv0 = new Vector2(u0, v1);
			Vector2 uv1 = new Vector2(u1, v1);
			Vector2 uv2 = new Vector2(u1, v0);
			Vector2 uv3 = new Vector2(u0, v0);

			Native.SpriteBatch.SpriteBatch_Draw(handle, vertex0.x,
				vertex0.y,
				vertex0.z,
				vertex1.x,
				vertex1.y,
				vertex1.z,
				vertex2.x,
				vertex2.y,
				vertex2.z,
				vertex3.x,
				vertex3.y,
				vertex3.z,
				normal0.x,
				normal0.y,
				normal0.z,
				normal1.x,
				normal1.y,
				normal1.z,
				normal2.x,
				normal2.y,
				normal2.z,
				normal3.x,
				normal3.y,
				normal3.z,
				uv0.x,
				uv0.y,
				uv1.x,
				uv1.y,
				uv2.x,
				uv2.y,
				uv3.x,
				uv3.y,
				color.x,
				color.y,
				color.z,
				color.w,
				mask,
				texture != null ? texture.handle : ushort.MaxValue, textureFlags);
		}

		public void draw(float x, float y, float z, float width, float height, float rotation, Vector2 rotationCenter, bool horizontal, Texture texture, uint textureFlags, float u0, float v0, float u1, float v1, bool flipX, bool flipY, Vector4 color, float mask, Vector3 normal0, Vector3 normal1, Vector3 normal2, Vector3 normal3)
		{
			float x0 = -rotationCenter.x;
			float y0 = -rotationCenter.y;
			float x1 = -rotationCenter.x + width;
			float y1 = -rotationCenter.y + height;

			Vector3 vertex0 = new Vector3(x, y, z);
			Vector3 vertex1 = new Vector3(x, y, z);
			Vector3 vertex2 = new Vector3(x, y, z);
			Vector3 vertex3 = new Vector3(x, y, z);

			Vector2 offset0 = new Vector2(x0, y0);
			Vector2 offset1 = new Vector2(x1, y0);
			Vector2 offset2 = new Vector2(x1, y1);
			Vector2 offset3 = new Vector2(x0, y1);

			float s = MathF.Sin(rotation);
			float c = MathF.Cos(rotation);
			offset0 = new Vector2(c * offset0.x - s * offset0.y, s * offset0.x + c * offset0.y);
			offset1 = new Vector2(c * offset1.x - s * offset1.y, s * offset1.x + c * offset1.y);
			offset2 = new Vector2(c * offset2.x - s * offset2.y, s * offset2.x + c * offset2.y);
			offset3 = new Vector2(c * offset3.x - s * offset3.y, s * offset3.x + c * offset3.y);

			if (horizontal)
			{
				vertex0.x += offset0.x;
				vertex1.x += offset1.x;
				vertex2.x += offset2.x;
				vertex3.x += offset3.x;
				vertex0.z += offset0.y;
				vertex1.z += offset1.y;
				vertex2.z += offset2.y;
				vertex3.z += offset3.y;

				vertex0.x += rotationCenter.x;
				vertex1.x += rotationCenter.x;
				vertex2.x += rotationCenter.x;
				vertex3.x += rotationCenter.x;
				vertex0.z += rotationCenter.y;
				vertex1.z += rotationCenter.y;
				vertex2.z += rotationCenter.y;
				vertex3.z += rotationCenter.y;
			}
			else
			{
				vertex0.xy += offset0;
				vertex1.xy += offset1;
				vertex2.xy += offset2;
				vertex3.xy += offset3;

				vertex0.xy += rotationCenter;
				vertex1.xy += rotationCenter;
				vertex2.xy += rotationCenter;
				vertex3.xy += rotationCenter;
			}

			/*
			bool hasTransform = false;
			Matrix transform = Matrix.Identity;
			if (hasTransform)
			{
				vertex0 = transform * vertex0;
				vertex1 = transform * vertex1;
				vertex2 = transform * vertex2;
				vertex3 = transform * vertex3;
			}
			*/

			if (flipX)
			{
				float tmp = u0;
				u0 = u1;
				u1 = tmp;
			}
			if (flipY)
			{
				float tmp = v0;
				v0 = v1;
				v1 = tmp;
			}

			Vector2 uv0 = new Vector2(u0, v1);
			Vector2 uv1 = new Vector2(u1, v1);
			Vector2 uv2 = new Vector2(u1, v0);
			Vector2 uv3 = new Vector2(u0, v0);

			Native.SpriteBatch.SpriteBatch_Draw(handle,
				vertex0.x,
				vertex0.y,
				vertex0.z,
				vertex1.x,
				vertex1.y,
				vertex1.z,
				vertex2.x,
				vertex2.y,
				vertex2.z,
				vertex3.x,
				vertex3.y,
				vertex3.z,
				normal0.x,
				normal0.y,
				normal0.z,
				normal1.x,
				normal1.y,
				normal1.z,
				normal2.x,
				normal2.y,
				normal2.z,
				normal3.x,
				normal3.y,
				normal3.z,
				uv0.x,
				uv0.y,
				uv1.x,
				uv1.y,
				uv2.x,
				uv2.y,
				uv3.x,
				uv3.y,
				color.x,
				color.y,
				color.z,
				color.w,
				mask,
				texture != null ? texture.handle : ushort.MaxValue, textureFlags);
		}

		public void draw(float x, float y, float z, float width, float height, float rotation, Texture texture, uint textureFlags, float u0, float v0, float u1, float v1, Vector4 color, float mask)
		{
			draw(x, y, z, width, height, rotation, new Vector2(width, height) * 0.5f, texture, textureFlags, u0, v0, u1, v1, false, false, color, mask, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero);
		}

		public void drawBillboard(float x, float y, float z, float width, float height, float rotation, Texture texture, uint textureFlags, float u0, float v0, float u1, float v1, Vector4 color, float mask)
		{
			Vector3 vertex0 = new Vector3(x, y, z);
			Vector3 vertex1 = new Vector3(x + width, y, z);
			Vector3 vertex2 = new Vector3(x + width, y + height, z);
			Vector3 vertex3 = new Vector3(x, y + height, z);

			float hw = 0.5f * width;
			float hh = 0.5f * height;
			float s = MathF.Sin(rotation);
			float c = MathF.Cos(rotation);
			Vector3 normal0 = new Vector3(c * -hw + s * hh, s * -hw - c * hh, 0.0f);
			Vector3 normal1 = new Vector3(c * hw + s * hh, s * hw - c * hh, 0.0f);
			Vector3 normal2 = new Vector3(c * hw - s * hh, s * hw + c * hh, 0.0f);
			Vector3 normal3 = new Vector3(c * -hw - s * hh, s * -hw + c * hh, 0.0f);

			Vector2 uv0 = new Vector2(u0, v1);
			Vector2 uv1 = new Vector2(u1, v1);
			Vector2 uv2 = new Vector2(u1, v0);
			Vector2 uv3 = new Vector2(u0, v0);

			Native.SpriteBatch.SpriteBatch_Draw(handle,
				vertex0.x,
				vertex0.y,
				vertex0.z,
				vertex1.x,
				vertex1.y,
				vertex1.z,
				vertex2.x,
				vertex2.y,
				vertex2.z,
				vertex3.x,
				vertex3.y,
				vertex3.z,
				normal0.x,
				normal0.y,
				normal0.z,
				normal1.x,
				normal1.y,
				normal1.z,
				normal2.x,
				normal2.y,
				normal2.z,
				normal3.x,
				normal3.y,
				normal3.z,
				uv0.x,
				uv0.y,
				uv1.x,
				uv1.y,
				uv2.x,
				uv2.y,
				uv3.x,
				uv3.y,
				color.x,
				color.y,
				color.z,
				color.w,
				mask,
				texture != null ? texture.handle : ushort.MaxValue, textureFlags);
		}

		public void draw(float width, float height, Matrix transform, Texture texture, uint textureFlags, float u0, float v0, float u1, float v1, Vector4 color, float mask)
		{
			float x0 = -0.5f * width;
			float x1 = 0.5f * width;
			float y0 = -0.5f * height;
			float y1 = 0.5f * height;

			Vector3 vertex0 = new Vector3(x0, y0, 0);
			Vector3 vertex1 = new Vector3(x1, y0, 0);
			Vector3 vertex2 = new Vector3(x1, y1, 0);
			Vector3 vertex3 = new Vector3(x0, y1, 0);

			vertex0 = transform * vertex0;
			vertex1 = transform * vertex1;
			vertex2 = transform * vertex2;
			vertex3 = transform * vertex3;

			Vector2 uv0 = new Vector2(u0, v1);
			Vector2 uv1 = new Vector2(u1, v1);
			Vector2 uv2 = new Vector2(u1, v0);
			Vector2 uv3 = new Vector2(u0, v0);

			Vector3 normal0 = Vector3.Zero;
			Vector3 normal1 = Vector3.Zero;
			Vector3 normal2 = Vector3.Zero;
			Vector3 normal3 = Vector3.Zero;

			Native.SpriteBatch.SpriteBatch_Draw(handle, vertex0.x,
				vertex0.y,
				vertex0.z,
				vertex1.x,
				vertex1.y,
				vertex1.z,
				vertex2.x,
				vertex2.y,
				vertex2.z,
				vertex3.x,
				vertex3.y,
				vertex3.z,
				normal0.x,
				normal0.y,
				normal0.z,
				normal1.x,
				normal1.y,
				normal1.z,
				normal2.x,
				normal2.y,
				normal2.z,
				normal3.x,
				normal3.y,
				normal3.z,
				uv0.x,
				uv0.y,
				uv1.x,
				uv1.y,
				uv2.x,
				uv2.y,
				uv3.x,
				uv3.y,
				color.x,
				color.y,
				color.z,
				color.w,
				mask,
				texture != null ? texture.handle : ushort.MaxValue, textureFlags);
		}
	}
}
