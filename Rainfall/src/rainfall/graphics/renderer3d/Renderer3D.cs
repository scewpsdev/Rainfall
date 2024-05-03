using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;


namespace Rainfall
{
	public static class Renderer
	{
		public static GraphicsDevice graphics { get; private set; }

		static Vector3 cameraPosition;
		static Quaternion cameraRotation;
		public static Matrix pv;

		public static Vector3 fogColor = new Vector3(1.0f);
		public static float fogIntensity = 0.0f;

		public static Vector3 vignetteColor = new Vector3(0.0f);
		public static float vignetteFalloff = 0.0f; // default value: 0.37f

		public static int meshRenderCounter = 0;
		public static int meshCulledCounter = 0;

		public static bool simplifiedLighting = false;
		public static bool ambientOcclusionEnabled = true;
		public static bool bloomEnabled = true;


		public static void Init(int width, int height, GraphicsDevice graphics)
		{
			Renderer.graphics = graphics;

			Renderer3D_Init(width, height);

			GUI.Init(graphics);
		}

		public static void Resize(int width, int height)
		{
			Renderer3D_Resize(width, height);
		}

		public static void Terminate()
		{
			Renderer3D_Terminate();
		}

		public static unsafe void DrawMesh(MeshData* mesh, Material material, Matrix transform, Animator animator = null)
		{
			Renderer3D_DrawMesh(mesh, transform, material.handle, animator != null ? animator.handle : IntPtr.Zero);
		}

		public static unsafe void DrawMesh(Model model, int meshID, Matrix transform, Animator animator = null)
		{
			MeshData* mesh = model.getMeshData(meshID);
			IntPtr material = mesh->materialID != -1 ? Material.Material_GetForData(model.getMaterialData(mesh->materialID)) : IntPtr.Zero;
			Renderer3D_DrawMesh(mesh, transform, material, animator != null ? animator.handle : IntPtr.Zero);
		}

		public static unsafe void DrawModel(Model model, Matrix transform, Animator animator = null)
		{
			Renderer3D_DrawScene(model.scene, transform, animator != null ? animator.handle : IntPtr.Zero);
		}

		public static void DrawSky(Cubemap skybox, float intensity, Quaternion rotation)
		{
			Renderer3D_DrawSky(skybox.handle, intensity, rotation);
		}

		public static void DrawEnvironmentMap(Cubemap environmentMap, float intensity)
		{
			Renderer3D_DrawEnvironmentMap(environmentMap.handle, intensity);
		}

		public static void DrawEnvironmentMapMask(Vector3 position, Vector3 size, float falloff)
		{
			Renderer3D_DrawEnvironmentMapMask(position, size, falloff);
		}

		public static void DrawModelStaticInstanced_(Model model, Matrix transform)
		{
			// not supported atm, dont use
			Debug.Assert(false);
		}

		public static void DrawSubModelStaticInstanced_(Model model, int meshID, Matrix transform)
		{
			// not supported atm, dont use
			Debug.Assert(false);
		}

		public static void DrawTerrain(Texture heightmap, Texture normalmap, Texture splatMap, Matrix transform, Texture diffuse0, Texture diffuse1, Texture diffuse2, Texture diffuse3)
		{
			// not supported atm, dont use
			Debug.Assert(false);
		}

		public static void DrawLeaves(Model model, int meshID, Matrix transform)
		{
			// not supported atm, dont use
			Debug.Assert(false);
		}

		public static void DrawLight(Vector3 position, Vector3 color)
		{
			Renderer3D_DrawPointLight(position, color);
		}

		public static void DrawPointLight(PointLight light, Matrix transform)
		{
			DrawLight(transform * light.offset, light.color);
		}

		public static void DrawDirectionalLight(DirectionalLight light)
		{
			Renderer3D_DrawDirectionalLight(light.direction, light.color);
		}

		public static void DrawReflectionProbe(ReflectionProbe reflectionProbe)
		{
			// TODO implement
		}

		public static void DrawWater(Vector3 position, float size)
		{
			// not supported atm, dont use
			Debug.Assert(false);
		}

		public static void DrawWater(Vector3 position, Model model)
		{
			// not supported atm, dont use
			Debug.Assert(false);
		}

		public static unsafe void DrawParticleSystem(ParticleSystem particleSystem)
		{
			Renderer3D_DrawParticleSystem(particleSystem.handle);
		}

		public static void DrawGrassPatch(/*Terrain terrain, Vector2 position*/)
		{
			// not supported atm, dont use
			Debug.Assert(false);
		}

		public static void Begin()
		{
			Renderer3D_Begin();
		}

		public static void SetCamera(Vector3 position, Quaternion rotation, Matrix projection)
		{
			cameraPosition = position;
			cameraRotation = rotation;

			pv = projection * Matrix.CreateTransform(position, rotation).inverted;

			Renderer3D_SetCamera(position, rotation, projection);
		}

		public static void End()
		{
			meshRenderCounter = 0;
			meshCulledCounter = 0;

			Renderer3D_End();

			GUI.Draw(94);
		}

		public static int DrawDebugStats(int x, int y, byte color)
		{
			return Renderer3D_DrawDebugStats(x, y, color);
		}


		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_Init(int width, int height);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_Resize(int width, int height);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_Terminate();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_SetCamera(Vector3 position, Quaternion rotation, Matrix proj);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern unsafe static void Renderer3D_DrawMesh(MeshData* mesh, Matrix transform, IntPtr material, IntPtr animation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern unsafe static void Renderer3D_DrawScene(SceneData* scene, Matrix transform, IntPtr animation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawPointLight(Vector3 position, Vector3 color);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawDirectionalLight(Vector3 direction, Vector3 color);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static unsafe void Renderer3D_DrawParticleSystem(ParticleSystemData* particleSystem);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawSky(ushort sky, float intensity, Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawEnvironmentMap(ushort environmentMap, float intensity);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawEnvironmentMapMask(Vector3 position, Vector3 size, float falloff);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_Begin();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_End();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static int Renderer3D_DrawDebugStats(int x, int y, byte color);
	}
}
