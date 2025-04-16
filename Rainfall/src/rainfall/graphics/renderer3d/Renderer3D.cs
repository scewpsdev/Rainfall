using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;


namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RendererSettings
	{
		internal byte _showFrame = 1;
		public bool showFrame { get => _showFrame != 0; set { _showFrame = (byte)(value ? 1 : 0); } }

		internal byte _ssaoEnabled = 1;
		public bool ssaoEnabled { get => _ssaoEnabled != 0; set { _ssaoEnabled = (byte)(value ? 1 : 0); } }

		internal byte _bloomEnabled = 1;
		public bool bloomEnabled { get => _bloomEnabled != 0; set { _bloomEnabled = (byte)(value ? 1 : 0); } }
		public float bloomStrength = 0.1f;
		public float bloomFalloff = 5.0f;

		public float exposure = 1.0f;

		public Vector3 fogColor = Vector3.One;
		public float fogStrength = 0.0f;

		internal byte _vignetteEnabled = 1;
		public bool vignetteEnabled { get => _vignetteEnabled != 0; set { _vignetteEnabled = (byte)(value ? 1 : 0); } }
		public Vector4 vignetteColor = new Vector4(0, 0, 0, 1);
		public float vignetteFalloff = 0.12f;

		internal byte physicsDebugDraw = 0;

		public RendererSettings(int _)
		{
		}
	}

	public static unsafe class Renderer
	{
		public static GraphicsDevice graphics { get; private set; }

		static Vector3 cameraPosition;
		static Quaternion cameraRotation;
		public static Matrix pv;

		public static int meshRenderCounter = 0;
		public static int meshCulledCounter = 0;


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
			GUI.Terminate();

			Renderer3D_Terminate();
		}

		public static void SetSettings(RendererSettings settings)
		{
			Renderer3D_SetSettings(settings);
		}

		public static unsafe void DrawMesh(MeshData* mesh, Material material, Matrix transform, Animator animator = null, bool isOccluder = false)
		{
			Renderer3D_DrawMesh(mesh, transform, material.handle, animator != null ? animator.handle : IntPtr.Zero, (byte)(isOccluder ? 1 : 0));
		}

		public static unsafe void DrawMesh(Model model, int meshID, Material material, Matrix transform, Animator animator = null, bool isOccluder = false)
		{
			MeshData* mesh = model.getMeshData(meshID);
			IntPtr materialHandle = material != null ? material.handle : mesh->materialID != -1 ? Material.Material_GetForData(model.getMaterialData(mesh->materialID)) : Material.Material_GetDefault();
			if (mesh->node != null)
			{
				if (animator != null && mesh->node->parent->armatureID != -1)
				{
					transform = transform * animator.getNodeTransform(model.skeleton.getNode(mesh->node->id));
				}
				else
				{
					transform = transform * mesh->node->transform;
				}
			}
			Renderer3D_DrawMesh(mesh, transform, materialHandle, animator != null ? animator.handle : IntPtr.Zero, (byte)(isOccluder ? 1 : 0));
		}

		public static unsafe void DrawMesh(Model model, int meshID, Matrix transform, Animator animator = null, bool isOccluder = false)
		{
			DrawMesh(model, meshID, null, transform, animator, isOccluder);
		}

		public static void DrawModel(Model model, Matrix transform, Animator animator = null, bool isOccluder = false)
		{
			for (int i = 0; i < model.meshCount; i++)
			{
				DrawMesh(model, i, transform, animator, isOccluder);
			}
			//Renderer3D_DrawScene(model.scene, transform, animator != null ? animator.handle : IntPtr.Zero, (byte)(isOccluder ? 1 : 0));
		}

		public static void DrawModel(Model model, Matrix transform, Material material, Animator animator = null, bool isOccluder = false)
		{
			for (int i = 0; i < model.meshCount; i++)
			{
				DrawMesh(model, i, material, transform, animator, isOccluder);
			}
		}

		public static void DrawModel(Model model, Vector3 position, Quaternion rotation, Vector3 scale, Animator animator = null, bool isOccluder = false)
		{
			Matrix transform = Matrix.CreateTransform(position, rotation, scale);
			for (int i = 0; i < model.meshCount; i++)
			{
				DrawMesh(model, i, transform, animator, isOccluder);
			}
		}

		public static void DrawModel(Model model, Vector3 position, Quaternion rotation, Animator animator = null, bool isOccluder = false)
		{
			Matrix transform = Matrix.CreateTransform(position, rotation);
			for (int i = 0; i < model.meshCount; i++)
			{
				DrawMesh(model, i, transform, animator, isOccluder);
			}
		}

		public static void DrawCustomGeometry(Span<VertexBuffer> vertexBuffers, IndexBuffer indexBuffer, Matrix transform, Material material, PrimitiveType primitiveType = PrimitiveType.Triangle, BlendState blendState = BlendState.Default)
		{
			Span<ushort> vertexBufferHandles = stackalloc ushort[8];
			for (int i = 0; i < vertexBuffers.Length; i++)
				vertexBufferHandles[i] = vertexBuffers[i].handle;
			fixed (ushort* vertexBufferHandlesPtr = vertexBufferHandles)
				Renderer3D_DrawCustomGeometry(vertexBuffers.Length, vertexBufferHandlesPtr, 0, indexBuffer.handle, primitiveType, blendState, transform, material.handle);
		}

		public static void DrawCustomGeometry(Span<DynamicVertexBuffer> vertexBuffers, IndexBuffer indexBuffer, Matrix transform, Material material, PrimitiveType primitiveType = PrimitiveType.Triangle, BlendState blendState = BlendState.Default)
		{
			Span<ushort> vertexBufferHandles = stackalloc ushort[8];
			for (int i = 0; i < vertexBuffers.Length; i++)
				vertexBufferHandles[i] = vertexBuffers[i].handle;
			fixed (ushort* vertexBufferHandlesPtr = vertexBufferHandles)
				Renderer3D_DrawCustomGeometry(vertexBuffers.Length, vertexBufferHandlesPtr, 1, indexBuffer != null ? indexBuffer.handle : ushort.MaxValue, primitiveType, blendState, transform, material.handle);
		}

		public static void DrawCloth(Cloth cloth, MaterialData* materialData, Vector3 position, Quaternion rotation)
		{
			IntPtr material = Material.Material_GetForData(materialData);
			Renderer3D_DrawCloth(cloth.handle, material, position, rotation);
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

		public static void DrawReflectionProbe(Vector3 position, Vector3 size)
		{
			Renderer3D_DrawReflectionProbe(position, size);
		}

		public static void DrawReflectionProbe(ReflectionProbe reflectionProbe)
		{
			Renderer3D_DrawReflectionProbe(reflectionProbe.position, reflectionProbe.size);
		}

		public static void DrawDebugLine(Vector3 position0, Vector3 position1, uint color)
		{
			Renderer3D_DrawDebugLine(position0, position1, color);
		}

		public static void DrawDebugBox(Vector3 size, Matrix transform, uint color)
		{
			Vector3 vertex0 = transform * (0.5f * new Vector3(-size.x, -size.y, -size.z));
			Vector3 vertex1 = transform * (0.5f * new Vector3(size.x, -size.y, -size.z));
			Vector3 vertex2 = transform * (0.5f * new Vector3(size.x, -size.y, size.z));
			Vector3 vertex3 = transform * (0.5f * new Vector3(-size.x, -size.y, size.z));
			Vector3 vertex4 = transform * (0.5f * new Vector3(-size.x, size.y, -size.z));
			Vector3 vertex5 = transform * (0.5f * new Vector3(size.x, size.y, -size.z));
			Vector3 vertex6 = transform * (0.5f * new Vector3(size.x, size.y, size.z));
			Vector3 vertex7 = transform * (0.5f * new Vector3(-size.x, size.y, size.z));

			DrawDebugLine(vertex0, vertex1, color);
			DrawDebugLine(vertex1, vertex2, color);
			DrawDebugLine(vertex2, vertex3, color);
			DrawDebugLine(vertex3, vertex0, color);

			DrawDebugLine(vertex4, vertex5, color);
			DrawDebugLine(vertex5, vertex6, color);
			DrawDebugLine(vertex6, vertex7, color);
			DrawDebugLine(vertex7, vertex4, color);

			DrawDebugLine(vertex0, vertex4, color);
			DrawDebugLine(vertex1, vertex5, color);
			DrawDebugLine(vertex2, vertex6, color);
			DrawDebugLine(vertex3, vertex7, color);
		}

		public static void DrawDebugSphere(float radius, Matrix transform, uint color)
		{
			int segmentCount = 32;
			for (int j = 0; j < 3; j++)
			{
				Quaternion ringRot = j == 0 ? Quaternion.Identity : j == 1 ? Quaternion.FromAxisAngle(Vector3.UnitX, MathF.PI * 0.5f) : Quaternion.FromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f);

				for (int k = 0; k < segmentCount; k++)
				{
					Vector3 vertex0 = transform * (ringRot * Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
					Vector3 vertex1 = transform * (ringRot * Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

					DrawDebugLine(vertex0, vertex1, color);
				}
			}
		}

		public static void DrawDebugCapsule(float radius, float height, Matrix transform, uint color)
		{
			int segmentCount = 32;

			// top ring
			for (int k = 0; k < segmentCount; k++)
			{
				Vector3 vertex0 = transform * (new Vector3(0.0f, height * 0.5f - radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
				Vector3 vertex1 = transform * (new Vector3(0.0f, height * 0.5f - radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

				DrawDebugLine(vertex0, vertex1, color);
			}

			// bottom ring
			for (int k = 0; k < segmentCount; k++)
			{
				Vector3 vertex0 = transform * (new Vector3(0.0f, height * -0.5f + radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
				Vector3 vertex1 = transform * (new Vector3(0.0f, height * -0.5f + radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

				DrawDebugLine(vertex0, vertex1, color);
			}

			// vertical ring 1
			for (int k = 0; k < segmentCount; k++)
			{
				Quaternion ringRot = Quaternion.FromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f);

				Vector3 vertex0 = transform * ((k < segmentCount / 2 ? 1 : -1) * new Vector3(0.0f, height * 0.5f - radius, 0.0f) + ringRot * Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
				Vector3 vertex1 = transform * ((k < segmentCount / 2 ? 1 : -1) * new Vector3(0.0f, height * 0.5f - radius, 0.0f) + ringRot * Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

				DrawDebugLine(vertex0, vertex1, color);
			}

			// vertical ring 2
			for (int k = 0; k < segmentCount; k++)
			{
				Quaternion ringRot = Quaternion.FromAxisAngle(Vector3.UnitY, MathF.PI * 0.5f) * Quaternion.FromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f);

				Vector3 vertex0 = transform * ((k < segmentCount / 2 ? 1 : -1) * new Vector3(0.0f, height * 0.5f - radius, 0.0f) + ringRot * Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
				Vector3 vertex1 = transform * ((k < segmentCount / 2 ? 1 : -1) * new Vector3(0.0f, height * 0.5f - radius, 0.0f) + ringRot * Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

				DrawDebugLine(vertex0, vertex1, color);
			}

			// vertical lines
			for (int k = 0; k < 4; k++)
			{
				Vector3 vertex0 = transform * (new Vector3(0.0f, -height * 0.5f + radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, k / 4.0f * MathF.PI * 2) * new Vector3(0.0f, 0.0f, radius));
				Vector3 vertex1 = transform * (new Vector3(0.0f, height * 0.5f - radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, k / 4.0f * MathF.PI * 2) * new Vector3(0.0f, 0.0f, radius));

				DrawDebugLine(vertex0, vertex1, color);
			}
		}

		public static void DrawDebugCollider(SceneFormat.ColliderData collider, Matrix transform, uint color)
		{
			if (collider.type == SceneFormat.ColliderType.Box)
			{
				DrawDebugBox(collider.size, transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
			}
			else if (collider.type == SceneFormat.ColliderType.Sphere)
			{
				DrawDebugSphere(collider.radius, transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
			}
			else if (collider.type == SceneFormat.ColliderType.Capsule)
			{
				DrawDebugCapsule(collider.radius, collider.height, transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
			}
			else if (collider.type == SceneFormat.ColliderType.Mesh || collider.type == SceneFormat.ColliderType.ConvexMesh)
			{
				if (collider.meshCollider != null)
				{
					DrawDebugBox(collider.meshCollider.boundingBox.size, transform * Matrix.CreateTranslation(collider.meshCollider.boundingBox.center + collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
					//DrawDebugSphere(collider.meshCollider.boundingSphere.Value.radius, transform * Matrix.CreateTranslation(collider.meshCollider.boundingBox.Value.center + collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color, inFront);
				}
			}
		}

		static bool IsDeformBone(Node node)
		{
			bool ik = node.name.IndexOf("ik", StringComparison.OrdinalIgnoreCase) >= 0;
			bool poleTarget = node.name.IndexOf("pole_target", StringComparison.OrdinalIgnoreCase) >= 0 || node.name.IndexOf("poletarget", StringComparison.OrdinalIgnoreCase) >= 0;

			return !ik && !poleTarget;
		}

		static void DrawDebugSkeletonNode(Node node, Dictionary<string, SceneFormat.ColliderData> boneColliders, Matrix nodeTransform, uint color, bool[] mask)
		{
			bool isLeafNode = node.children == null;
			if (isLeafNode)
			{
				//Vector3 endPoint = nodeTransform * (Vector3.Up * 0.1f);
				//DrawDebugLine(nodeTransform.translation, endPoint, color);
			}
			else
			{
				for (int i = 0; i < node.children.Length; i++)
				{
					if (IsDeformBone(node.children[i]))
					{
						Matrix childTransform = nodeTransform * node.children[i].transform;
						DrawDebugLine(nodeTransform.translation, childTransform.translation, color);
						DrawDebugSkeletonNode(node.children[i], boneColliders, childTransform, color, mask);
					}
				}
			}

			if (boneColliders != null)
			{
				if (boneColliders.ContainsKey(node.name))
				{
					if (mask == null || mask[node.id])
						DrawDebugCollider(boneColliders[node.name], nodeTransform, color);
				}
			}
		}

		public static void DrawDebugSkeleton(Skeleton skeleton, Dictionary<string, SceneFormat.ColliderData> boneColliders, Matrix transform, uint color, bool[] mask = null)
		{
			DrawDebugSkeletonNode(skeleton.rootNode, boneColliders, transform * skeleton.rootNode.transform, color, mask);
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
			Renderer3D_DrawPointLight(position, color, 0, 0, null, 0, 0, 0);
		}

		public static void DrawPointLight(PointLight light, Matrix transform)
		{
			if (light.shadowMap != null)
			{
				Span<ushort> shadowMap = stackalloc ushort[6];
				shadowMap[0] = light.shadowMap.renderTargets[0].handle;
				shadowMap[1] = light.shadowMap.renderTargets[1].handle;
				shadowMap[2] = light.shadowMap.renderTargets[2].handle;
				shadowMap[3] = light.shadowMap.renderTargets[3].handle;
				shadowMap[4] = light.shadowMap.renderTargets[4].handle;
				shadowMap[5] = light.shadowMap.renderTargets[5].handle;
				fixed (ushort* shadowMapPtr = shadowMap)
					Renderer3D_DrawPointLight(transform * light.offset, light.color, 1, light.shadowMap.cubemap.handle, shadowMapPtr, PointShadowMap.resolution, light.shadowMap.nearPlane, (byte)(light.shadowMap.needsUpdate ? 1 : 0));
				light.shadowMap.needsUpdate = light.dynamicShadowMap;
			}
			else
			{
				Renderer3D_DrawPointLight(transform * light.offset, light.color, 0, 0, null, 0, 0, 0);
			}
		}

		public static void DrawDirectionalLight(DirectionalLight light)
		{
			Renderer3D_DrawDirectionalLight(light.direction, light.color, (byte)(light.shadowMap.needsUpdate ? 1 : 0), light.shadowMap.resolution, light.shadowMap.renderTargets[0].handle, light.shadowMap.renderTargets[1].handle, light.shadowMap.renderTargets[2].handle);
		}

		public static void DrawWater(Vector3 position, float size)
		{
			// not supported atm, dont use
			Debug.Assert(false);
		}

		public static void DrawWater(Vector3 position, Model model)
		{
			// TODO implement
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

		public static void SetCamera(Vector3 position, Quaternion rotation, float fov, float aspect, float near, float far)
		{
			cameraPosition = position;
			cameraRotation = rotation;

			Matrix projection = Matrix.CreatePerspective(MathHelper.ToRadians(fov), aspect, near, far);
			pv = projection * Matrix.CreateTransform(position, rotation).inverted;
			Renderer3D_SetCamera(position, rotation, projection, fov, aspect, near, far);
		}

		public static void SetCameraOrtho(Vector3 position, Quaternion rotation, float width, float height, float near, float far)
		{
			cameraPosition = position;
			cameraRotation = rotation;

			Matrix projection = Matrix.CreateOrthographic(width, height, near, far);
			pv = projection * Matrix.CreateTransform(position, rotation).inverted;
			Renderer3D_SetCamera(position, rotation, projection, 90, width / height, near, far);
		}

		public static ushort End()
		{
			meshRenderCounter = 0;
			meshCulledCounter = 0;

			ushort frame = Renderer3D_End();

			GUI.Draw();

			return frame;
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
		extern static void Renderer3D_SetSettings(RendererSettings settings);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_SetCamera(Vector3 position, Quaternion rotation, Matrix proj, float fov, float aspect, float near, float far);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern unsafe static void Renderer3D_DrawMesh(MeshData* mesh, Matrix transform, IntPtr material, IntPtr animation, byte isOccluder);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern unsafe static void Renderer3D_DrawScene(SceneData* scene, Matrix transform, IntPtr animation, byte isOccluder);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawCustomGeometry(int numVertexBuffers, ushort* vertexBuffers, byte dynamicVertexBuffers, ushort indexBuffer, PrimitiveType primitiveType, BlendState blendState, Matrix transform, IntPtr material);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawCloth(IntPtr cloth, IntPtr material, Vector3 position, Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawPointLight(Vector3 position, Vector3 color, byte hasShadowMap, ushort shadowMap, ushort* shadowMapRTs, int shadowMapRes, float shadowMapNear, byte shadowMapNeedsUpdate);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawDirectionalLight(Vector3 direction, Vector3 color, byte shadowsNeedUpdate, int shadowMapRes, ushort cascade0, ushort cascade1, ushort cascade2);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static unsafe void Renderer3D_DrawParticleSystem(ParticleSystemData* particleSystem);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawSky(ushort sky, float intensity, Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawEnvironmentMap(ushort environmentMap, float intensity);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawEnvironmentMapMask(Vector3 position, Vector3 size, float falloff);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawReflectionProbe(Vector3 position, Vector3 size);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_DrawDebugLine(Vector3 position0, Vector3 position1, uint color);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static void Renderer3D_Begin();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static ushort Renderer3D_End();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		extern static int Renderer3D_DrawDebugStats(int x, int y, byte color);
	}
}
