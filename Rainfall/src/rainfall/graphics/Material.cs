using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class Material
	{
		internal IntPtr handle;


		internal Material(IntPtr handle)
		{
			this.handle = handle;
		}

		public Material(uint color = 0xFFFFFFFF, float metallicFactor = 0.0f, float roughnessFactor = 1.0f, Vector3 emissiveColor = default, float emissiveStrength = 0.0f, Texture diffuse = null, Texture normal = null, Texture roughness = null, Texture metallic = null, Texture emissive = null, Texture height = null)
		{
			handle = Material_CreateDeferred(color, metallicFactor, roughnessFactor, emissiveColor, emissiveStrength,
				diffuse != null ? diffuse.handle : ushort.MaxValue,
				normal != null ? normal.handle : ushort.MaxValue,
				roughness != null ? roughness.handle : ushort.MaxValue,
				metallic != null ? metallic.handle : ushort.MaxValue,
				emissive != null ? emissive.handle : ushort.MaxValue,
				height != null ? height.handle : ushort.MaxValue);
		}

		public Material(Shader shader, bool isForward = false, Vector4 data0 = default, Vector4 data1 = default, Vector4 data2 = default, Vector4 data3 = default, Texture texture0 = null, Texture texture1 = null, Texture texture2 = null, Texture texture3 = null, Texture texture4 = null, Texture texture5 = null)
		{
			handle = Material_Create(shader.handle, (byte)(isForward ? 1 : 0),
				data0, data1, data2, data3,
				texture0 != null ? texture0.handle : ushort.MaxValue,
				texture1 != null ? texture1.handle : ushort.MaxValue,
				texture2 != null ? texture2.handle : ushort.MaxValue,
				texture3 != null ? texture3.handle : ushort.MaxValue,
				texture4 != null ? texture4.handle : ushort.MaxValue,
				texture5 != null ? texture5.handle : ushort.MaxValue);
		}

		public unsafe void destroy()
		{
			Material_Destroy(handle);
		}

		public void setData(int idx, Vector4 v)
		{
			Material_SetData(handle, idx, v);
		}

		public void setTexture(int idx, Texture texture)
		{
			Material_SetTexture(handle, idx, texture.handle);
		}

		public void setTexture(int idx, Cubemap cubemap)
		{
			Material_SetTexture(handle, idx, cubemap.handle);
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr Material_Create(IntPtr shader, byte isForward, Vector4 data0, Vector4 data1, Vector4 data2, Vector4 data3, ushort texture0, ushort texture1, ushort texture2, ushort texture3, ushort texture4, ushort texture5);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr Material_CreateDeferred(uint color, float metallicFactor, float roughnessFactor, Vector3 emissiveColor, float emissiveStrength, ushort diffuse, ushort normal, ushort roughness, ushort metallic, ushort emissive, ushort height);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void Material_Destroy(IntPtr data);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void Material_SetData(IntPtr material, int idx, Vector4 v);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void Material_SetTexture(IntPtr material, int idx, ushort texture);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Material_GetForData(MaterialData* materialData);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Material_GetDefault();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe void Material_CreateMaterialsForScene(SceneData* scene);
	}
}
