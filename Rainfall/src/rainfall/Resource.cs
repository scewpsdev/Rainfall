using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public unsafe struct ImageData
	{
		public uint* data;
		public int size;

		public TextureFormat format;
		public int width, height;
	}

	public static class Resource
	{
		static Dictionary<string, string> texts = new Dictionary<string, string>();
		static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
		static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
		static Dictionary<string, Cubemap> cubemaps = new Dictionary<string, Cubemap>();
		static Dictionary<string, IntPtr> scenes = new Dictionary<string, IntPtr>();
		static Dictionary<string, FontData> fonts = new Dictionary<string, FontData>();
		static Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();


		public static string ReadText(string path)
		{
			return File.ReadAllText(path + ".bin");
		}

		internal static Shader CreateShader(string vertexPath, string fragmentPath)
		{
			Span<byte> vertexPtr = stackalloc byte[256];
			StringUtils.WriteString(vertexPtr, vertexPath);
			StringUtils.AppendString(vertexPtr, ".bin");

			Span<byte> fragmentPtr = stackalloc byte[256];
			StringUtils.WriteString(fragmentPtr, fragmentPath);
			StringUtils.AppendString(fragmentPtr, ".bin");

			unsafe
			{
				fixed (byte* vertexPtrData = vertexPtr, fragmentPtrData = fragmentPtr)
				{
					IntPtr handle = Native.Resource.Resource_CreateShader(vertexPtrData, fragmentPtrData);
					if (handle != IntPtr.Zero)
						return new Shader(handle);
					return null;
				}
			}
		}

		internal static Shader CreateShader(string computePath)
		{
			Span<byte> computePtr = stackalloc byte[256];
			StringUtils.WriteString(computePtr, computePath);
			StringUtils.AppendString(computePtr, ".bin");

			unsafe
			{
				fixed (byte* computePtrData = computePtr)
				{
					IntPtr handle = Native.Resource.Resource_CreateShaderCompute(computePtrData);
					if (handle != IntPtr.Zero)
						return new Shader(handle);
					return null;
				}
			}
		}

		internal static Texture CreateTexture(string path, ulong flags)
		{
			Span<byte> pathPtr = stackalloc byte[256];
			StringUtils.WriteString(pathPtr, path);
			StringUtils.AppendString(pathPtr, ".bin");

			unsafe
			{
				fixed (byte* pathPtrData = pathPtr)
				{
					ushort handle = Native.Resource.Resource_CreateTexture2DFromFile(pathPtrData, flags, out TextureInfo info);
					if (handle != ushort.MaxValue)
						return new Texture(handle, info);
					return null;
				}
			}
		}

		/*
		internal static Cubemap CreateCubemap(int size, TextureFormat format, string[] sides)
		{
			Debug.Assert(sides.Length == 6);
			string[] sides_ = new string[6];
			for (int i = 0; i < 6; i++)
				sides_[i] = sides[i] + ".bin";
			ushort handle = Native.Resource.Resource_CreateCubemapFromFiles(size, format, sides_);
			if (handle != ushort.MaxValue)
				return new Cubemap(handle, size, format);
			return null;
		}
		*/

		internal static Cubemap CreateCubemap(string path, ulong flags)
		{
			Span<byte> pathPtr = stackalloc byte[256];
			StringUtils.WriteString(pathPtr, path);
			StringUtils.AppendString(pathPtr, ".bin");

			unsafe
			{
				fixed (byte* pathPtrData = pathPtr)
				{
					ushort handle = Native.Resource.Resource_CreateCubemapFromFile(pathPtrData, flags, out TextureInfo info);
					if (handle != ushort.MaxValue)
						return new Cubemap(handle, info);
					return null;
				}
			}
		}

		internal static IntPtr CreateScene(string path, ulong textureFlags)
		{
			Span<byte> pathPtr = stackalloc byte[256];
			StringUtils.WriteString(pathPtr, path);
			StringUtils.AppendString(pathPtr, ".bin");

			unsafe
			{
				fixed (byte* pathPtrData = pathPtr)
				{
					IntPtr handle = Native.Resource.Resource_CreateSceneDataFromFile(pathPtrData, textureFlags);
					return handle;
				}
			}
		}

		internal static Model CreateModel(IntPtr scene)
		{
			IntPtr handle = Native.Resource.Resource_CreateModelFromSceneData(scene);
			if (handle != IntPtr.Zero)
				return new Model(handle);
			return null;
		}

		internal static FontData CreateFontData(string path)
		{
			Span<byte> pathPtr = stackalloc byte[256];
			StringUtils.WriteString(pathPtr, path);
			StringUtils.AppendString(pathPtr, ".bin");

			unsafe
			{
				fixed (byte* pathPtrData = pathPtr)
				{
					IntPtr handle = Native.Resource.Resource_CreateFontDataFromFile(pathPtrData);
					if (handle != IntPtr.Zero)
						return new FontData(handle);
					return null;
				}
			}
		}

		internal static Sound CreateSound(string path)
		{
			Span<byte> pathPtr = stackalloc byte[256];
			StringUtils.WriteString(pathPtr, path);
			StringUtils.AppendString(pathPtr, ".bin");

			unsafe
			{
				fixed (byte* pathPtrData = pathPtr)
				{
					IntPtr handle = Native.Resource.Resource_CreateSoundFromFile(pathPtrData, out float duration);
					if (handle != IntPtr.Zero)
						return new Sound(handle, duration);
					return null;
				}
			}
		}

		public static string GetText(string path)
		{
			if (texts.ContainsKey(path))
				return texts[path];
			string text = ReadText(path);
			texts.Add(path, text);
			return text;
		}

		public static Shader GetShader(string vertexPath, string fragmentPath)
		{
			string combinedPath = vertexPath + "|" + fragmentPath;
			if (shaders.ContainsKey(combinedPath))
				return shaders[combinedPath];
			Shader shader = CreateShader(vertexPath, fragmentPath);
			shaders.Add(combinedPath, shader);
			return shader;
		}

		public static Shader GetShader(string computePath)
		{
			if (shaders.ContainsKey(computePath))
				return shaders[computePath];
			Shader shader = CreateShader(computePath);
			shaders.Add(computePath, shader);
			return shader;
		}

		public static byte[] ReadImage(string path, out TextureInfo info)
		{
			Span<byte> pathPtr = stackalloc byte[256];
			StringUtils.WriteString(pathPtr, path);
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

		public static Texture GetTexture(string path, ulong flags = 0)
		{
			if (textures.ContainsKey(path))
				return textures[path];
			Texture texture = CreateTexture(path, flags);
			textures.Add(path, texture);
			return texture;
		}

		public static Texture GetTexture(string path, bool linear)
		{
			return GetTexture(path, linear ? 0 : (uint)SamplerFlags.Point);
		}

		/*
		public static Cubemap GetCubemap(int size, TextureFormat format, string[] sides)
		{
			if (cubemaps.ContainsKey(sides[0]))
				return cubemaps[sides[0]];
			Cubemap cubemap = CreateCubemap(size, format, sides);
			cubemaps.Add(sides[0], cubemap);
			return cubemap;
		}
		*/

		public static Cubemap GetCubemap(string path, ulong flags = 0)
		{
			if (cubemaps.ContainsKey(path))
				return cubemaps[path];
			Cubemap cubemap = CreateCubemap(path, flags);
			cubemaps.Add(path, cubemap);
			return cubemap;
		}

		public static Model GetModel(string path, ulong textureFlags = 0)
		{
			if (scenes.ContainsKey(path))
			{
				IntPtr scene = scenes[path];
				return CreateModel(scene);
			}
			else
			{
				IntPtr scene = CreateScene(path, textureFlags);
				if (scene != IntPtr.Zero)
				{
					scenes.Add(path, scene);
					return CreateModel(scene);
				}
				return null;
			}
		}

		public static FontData GetFontData(string path)
		{
			if (fonts.ContainsKey(path))
				return fonts[path];
			FontData data = CreateFontData(path);
			fonts.Add(path, data);
			return data;
		}

		public static Sound GetSound(string path)
		{
			if (sounds.ContainsKey(path))
				return sounds[path];
			Sound sound = CreateSound(path);
			sounds.Add(path, sound);
			return sound;
		}
	}
}
