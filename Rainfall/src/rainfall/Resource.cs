using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public unsafe struct ImageData
	{
		public IntPtr handle;

		public uint* data;
		public int size;

		public TextureFormat format;
		public int width, height;


		public void free()
		{
			Resource.Resource_FreeImage(handle);
		}
	}

	public static class Resource
	{
		public static string ASSET_DIRECTORY = "assets";

		static Dictionary<IntPtr, Shader> shaders = new Dictionary<nint, Shader>();
		static Dictionary<IntPtr, Texture> textures = new Dictionary<nint, Texture>();
		static Dictionary<IntPtr, Cubemap> cubemaps = new Dictionary<nint, Cubemap>();
		static Dictionary<IntPtr, Sound> sounds = new Dictionary<nint, Sound>();
		static Dictionary<IntPtr, FontData> fonts = new Dictionary<nint, FontData>();


		public static void LoadPackageHeader(string path)
		{
			Resource_LoadPackageHeader($"{ASSET_DIRECTORY}/{path}");
		}

		public static Shader GetShader(string vertexPath, string fragmentPath)
		{
			IntPtr resource = Resource_GetShader($"{ASSET_DIRECTORY}/{vertexPath}", $"{ASSET_DIRECTORY}/{fragmentPath}");
			if (resource != IntPtr.Zero)
			{
				if (shaders.TryGetValue(resource, out Shader shader))
					return shader;
				shader = new Shader(resource);
				shaders.Add(resource, shader);
				return shader;
			}
			return null;
		}

		public static Shader GetShader(string computePath)
		{
			IntPtr resource = Resource_GetShaderCompute($"{ASSET_DIRECTORY}/{computePath}");
			if (resource != IntPtr.Zero)
			{
				if (shaders.TryGetValue(resource, out Shader shader))
					return shader;
				shader = new Shader(resource);
				shaders.Add(resource, shader);
				return shader;
			}
			return null;
		}

		public static void FreeShader(Shader shader)
		{
			if (Resource_FreeShader(shader.resource) == 0)
				shaders.Remove(shader.resource);
		}

		public static Texture GetTexture(string path, ulong flags = 0, bool keepCpuData = false)
		{
			IntPtr resource = Resource_GetTexture($"{ASSET_DIRECTORY}/{path}", flags, 0, (byte)(keepCpuData ? 1 : 0));
			if (resource != IntPtr.Zero)
			{
				if (textures.TryGetValue(resource, out Texture texture))
					return texture;
				texture = new Texture(resource);
				textures.Add(resource, texture);
				return texture;
			}
			return null;
		}

		public static Texture GetTexture(string path, bool linear, bool keepCpuData = false)
		{
			return GetTexture(path, linear ? 0 : (uint)SamplerFlags.Point, keepCpuData);
		}

		public static void FreeTexture(Texture texture)
		{
			if (Resource_FreeTexture(texture.resource) == 0)
			{
				textures.Remove(texture.resource);
				texture.destroy();
			}
		}

		public static Cubemap GetCubemap(string path, ulong flags = 0, bool keepCpuData = false)
		{
			IntPtr resource = Resource_GetTexture($"{ASSET_DIRECTORY}/{path}", flags, 1, (byte)(keepCpuData ? 1 : 0));
			if (resource != IntPtr.Zero)
			{
				if (cubemaps.TryGetValue(resource, out Cubemap cubemap))
					return cubemap;
				cubemap = new Cubemap(resource);
				cubemaps.Add(resource, cubemap);
				return cubemap;
			}
			return null;
		}

		public static void FreeCubemap(Cubemap cubemap)
		{
			if (Resource_FreeTexture(cubemap.resource) == 0)
				cubemaps.Remove(cubemap.resource);
		}

		public static Model GetModel(string path, ulong textureFlags = 0)
		{
			IntPtr resource = Resource_GetScene($"{ASSET_DIRECTORY}/{path}", textureFlags);
			if (resource != IntPtr.Zero)
				return new Model(resource);
			return null;
		}

		public static Model GetModel(string path, bool linearTextures)
		{
			return GetModel(path, linearTextures ? 0 : (uint)SamplerFlags.Point);
		}

		public static void FreeModel(Model model)
		{
			Resource_FreeScene(model.resource);
		}

		public static Sound GetSound(string path)
		{
			IntPtr resource = Resource_GetSound($"{ASSET_DIRECTORY}/{path}");
			if (resource != IntPtr.Zero)
			{
				if (sounds.TryGetValue(resource, out Sound sound))
					return sound;
				sound = new Sound(resource);
				sounds.Add(resource, sound);
				return sound;
			}
			return null;
		}

		public static Sound[] GetSounds(string path, int count)
		{
			Sound[] sounds = new Sound[count];
			for (int i = 0; i < count; i++)
				sounds[i] = GetSound(path + (i + 1) + ".ogg");
			return sounds;
		}

		public static void FreeSound(Sound sound)
		{
			if (Resource_FreeSound(sound.resource) == 0)
				sounds.Remove(sound.resource);
		}

		public static unsafe string GetText(string path)
		{
			IntPtr resource = Resource_GetMisc($"{ASSET_DIRECTORY}/{path}");
			if (resource != IntPtr.Zero)
			{
				byte* data = Resource_MiscGetData(resource, out int size);
				string str = new string((sbyte*)data, 0, size);
				Resource_FreeMisc(resource);
				return str;
			}
			return null;
		}

		public static FontData GetFontData(string path)
		{
			IntPtr resource = Resource_GetMisc($"{ASSET_DIRECTORY}/{path}");
			if (resource != IntPtr.Zero)
			{
				if (fonts.TryGetValue(resource, out FontData fontData))
					return fontData;
				fontData = new FontData(resource);
				fonts.Add(resource, fontData);
				return fontData;
			}
			return null;
		}

		public static void FreeFontData(FontData fontData)
		{
			if (Resource_FreeMisc(fontData.resource) == 0)
				fonts.Remove(fontData.resource);
		}



		/*
		public static byte[] ReadImage(string path, out TextureInfo info)
		{
			Span<byte> pathPtr = stackalloc byte[256];
			StringUtils.WriteString(pathPtr, ASSET_DIRECTORY + "/");
			StringUtils.AppendString(pathPtr, path);
			StringUtils.AppendString(pathPtr, ".bin");

			unsafe
			{
				fixed (byte* pathPtrData = pathPtr)
				{
					IntPtr image = Native.Resource.Resource_ReadImageFromFile(pathPtrData, out info);
					IntPtr imageData = Native.Resource.Resource_ImageGetData(image);
					byte* imageDataPtr = (byte*)imageData;
					byte[] pixels = new byte[info.width * info.height * 4];
					Unsafe.CopyBlock(ref pixels[0], ref imageDataPtr[0], (uint)(info.width * info.height * 4));
					Native.Resource.Resource_FreeImage(image);
					return pixels;
				}
			}
		}

		public static uint[] ReadImagePixels(string path, out TextureInfo info)
		{
			byte[] data = ReadImage(path, out info);
			uint[] pixels = new uint[data.Length / 4];
			for (int i = 0; i < data.Length / 4; i++)
				pixels[i] = BitConverter.ToUInt32(data, i * 4);
			return pixels;
		}
		*/



		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Resource_LoadPackageHeader(string path);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_GetShader(string vertexPath, string fragmentPath);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_GetShaderCompute(string computePath);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Resource_FreeShader(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_ShaderGetHandle(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_GetTexture(string path, ulong flags, byte cubemap, byte keepCPUData);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Resource_FreeTexture(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ushort Resource_TextureGetHandle(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe TextureInfo* Resource_TextureGetInfo(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Resource_TextureGetImage(IntPtr resource, out ImageData imageData);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Resource_TextureFreeCPUData(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Resource_FreeImage(IntPtr handle);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_GetScene(string path, ulong textureFlags);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Resource_FreeScene(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe SceneData* Resource_SceneGetHandle(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_GetSound(string path);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Resource_FreeSound(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_SoundGetHandle(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern float Resource_SoundGetDuration(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_GetMisc(string path);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Resource_FreeMisc(IntPtr resource);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe byte* Resource_MiscGetData(IntPtr resource, out int size);




		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Resource_ReadImageFromFile(byte* path, out TextureInfo info);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_ImageGetData(IntPtr image);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe ushort Resource_CreateTexture2DFromFile(byte* path, ulong flags, out TextureInfo info);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe ushort Resource_CreateCubemapFromFile(byte* path, ulong flags, out TextureInfo info);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe SceneData* Resource_CreateSceneDataFromFile(byte* path, ulong textureFlags);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe void Resource_DestroySceneData(SceneData* scene);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Resource_CreateFontDataFromFile(byte* path);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_CreateFontFromData(IntPtr data, float size, byte antialiased);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Resource_FontMeasureText(IntPtr font, byte* text, int offset, int count);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int Resource_FontMeasureText(IntPtr font, [MarshalAs(UnmanagedType.LPStr)] string text, int offset, int length);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Resource_CreateSoundFromFile(byte* path, out float duration);
	}
}
