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
			handle = Material_Create(color, metallicFactor, roughnessFactor, emissiveColor, emissiveStrength,
				diffuse != null ? diffuse.handle : ushort.MaxValue,
				normal != null ? normal.handle : ushort.MaxValue,
				roughness != null ? roughness.handle : ushort.MaxValue,
				metallic != null ? metallic.handle : ushort.MaxValue,
				emissive != null ? emissive.handle : ushort.MaxValue,
				height != null ? height.handle : ushort.MaxValue);
		}

		public unsafe void destroy()
		{
			Material_Destroy(handle);
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr Material_Create(uint color, float metallicFactor, float roughnessFactor, Vector3 emissiveColor, float emissiveStrength, ushort diffuse, ushort normal, ushort roughness, ushort metallic, ushort emissive, ushort height);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void Material_Destroy(IntPtr data);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr Material_GetDefault();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe void Material_CreateMaterialsForScene(SceneData* scene);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Material_GetForData(MaterialData* materialData);
	}
}
