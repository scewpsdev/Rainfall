using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;
using System.Runtime.CompilerServices;
using Rainfall.Native;

namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct PositionNormalTangent
	{
		public Vector3 position;
		public Vector3 normal;
		public Vector3 tangent;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct BoundingBox
	{
		public float x0, y0, z0;
		public float x1, y1, z1;

		public BoundingBox(float x0, float y0, float z0, float x1, float y1, float z1)
		{
			this.x0 = x0;
			this.y0 = y0;
			this.z0 = z0;
			this.x1 = x1;
			this.y1 = y1;
			this.z1 = z1;
		}

		public Vector3 size { get => new Vector3(x1 - x0, y1 - y0, z1 - z0); }
		public Vector3 center { get => new Vector3(x0 + x1, y0 + y1, z0 + z1) * 0.5f; }
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct BoundingSphere
	{
		public float xcenter, ycenter, zcenter;
		public float radius;

		public Vector3 center
		{
			get => new Vector3(xcenter, ycenter, zcenter);
			set { xcenter = value.x; ycenter = value.y; zcenter = value.z; }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MeshData
	{
		PositionNormalTangent* positionsNormalsTangents;
		Vector2* texcoords;
		uint* vertexColors;
		IntPtr boneWeights;

		public void* vertices { get => positionsNormalsTangents; }
		public int vertexCount { get; internal set; }

		public int* indices { get; internal set; }
		public int indexCount { get; internal set; }

		internal int materialID;
		internal int skeletonID;

		public BoundingBox boundingBox;
		public BoundingSphere boundingSphere;


		public NodeData* node;

		UInt16 vertexNormalTangentBuffer;
		UInt16 texcoordBuffer;
		UInt16 vertexColorBuffer;
		UInt16 boneWeightBuffer;

		UInt16 indexBuffer;


		public PositionNormalTangent getVertex(int index)
		{
			return positionsNormalsTangents[index];
		}

		public Vector2 getUV(int index)
		{
			return texcoords[index];
		}

		public int getIndex(int index)
		{
			return indices[index];
		}

		public ushort vertexBufferID
		{
			get { return vertexNormalTangentBuffer; }
		}

		public ushort indexBufferID
		{
			get { return indexBuffer; }
		}

		public bool hasSkeleton
		{
			get { return skeletonID != -1; }
		}

		public int nodeID
		{
			get { return node != null ? node->id : -1; }
		}
	}

	public unsafe struct MaterialData
	{
		public UInt32 color;
		public float metallicFactor;
		public float roughnessFactor;
		public Vector3 emissiveColor;
		public float emissiveStrength;

		public IntPtr diffuse;
		public IntPtr normal;
		public IntPtr roughness;
		public IntPtr metallic;
		public IntPtr emissive;
	};

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct SkeletonData
	{
		internal int boneCount;
		internal IntPtr bones;

		internal Matrix inverseBindPose;
	};

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct NodeData
	{
		public int id { get; internal set; }
		internal fixed byte name[32];
		public Matrix transform { get; internal set; }

		internal int numChildren;
		internal int* children;

		internal int numMeshes;
		internal int* meshes;


		public NodeData* parent { get; internal set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct AnimationData
	{
		internal fixed byte name[32];

		public float duration;

		internal int numPositions;
		internal int numRotations;
		internal int numScales;
		internal int numChannels;

		IntPtr positionKeyframes;
		IntPtr rotationKeyframes;
		IntPtr scaleKeyframes;
		IntPtr channels;
	}

	public enum LightType
	{
		Undefined,
		Directional,
		Point,
		Spot,
		Ambient,
		Area,
	};

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct LightData
	{
		public fixed byte name[32];

		public LightType type;
		public float x, y, z;
		public float xdir, ydir, zdir;
		public Vector3 color;

		public Vector3 position { get => new Vector3(x, y, z); }
		public Vector3 direction { get => new Vector3(xdir, ydir, zdir); }
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SceneData
	{
		public int numMeshes;
		public int numMaterials;
		public int numSkeletons;
		public int numAnimations;
		public int numNodes;
		public int numLights;

		public MeshData* meshes;
		public MaterialData* materials;
		internal SkeletonData* skeletons;
		public AnimationData* animations;
		public NodeData* nodes;
		public LightData* lights;
	}

	public class Model
	{
		public unsafe SceneData* scene { get; private set; }
		bool ownsScene = false;
		public float maxDistance = float.MaxValue;

		public readonly Skeleton skeleton;

		public bool isStatic = true;


		unsafe internal Model(SceneData* scene)
		{
			this.scene = scene;
			skeleton = new Skeleton(scene);
		}

		public unsafe Model(int numVertices, Span<PositionNormalTangent> vertices, Span<Vector2> uvs, int numIndices, Span<int> indices, MaterialData material)
		{
			fixed (PositionNormalTangent* verticesPtr = vertices)
			fixed (Vector2* uvsPtr = uvs)
			fixed (int* indicesPtr = indices)
			{
				scene = Model_Create(numVertices, verticesPtr, uvsPtr, numIndices, indicesPtr, &material);
			}
			skeleton = null;
			ownsScene = true;
		}

		public unsafe void destroy()
		{
			if (ownsScene)
				Model_Destroy(scene);
			scene = null;
		}

		public unsafe void drawMesh(GraphicsDevice graphics, int meshID, Shader shader, Matrix transform)
		{
			Model_DrawMesh(graphics.currentPass, scene, meshID, shader.handle, ref transform);
		}

		public unsafe void drawMeshAnimated(GraphicsDevice graphics, int meshID, Shader shader, Animator animator, Matrix transform)
		{
			Model_DrawMeshAnimated(graphics.currentPass, scene, meshID, shader.handle, animator.handle, ref transform);
		}

		public unsafe void draw(GraphicsDevice graphics, Shader shader, Shader animatedShader, Animator animator, Matrix transform)
		{
			Model_Draw(graphics.currentPass, scene, shader.handle, animatedShader != null ? animatedShader.handle : IntPtr.Zero, animator != null ? animator.handle : IntPtr.Zero, ref transform);
		}

		public unsafe MeshData? getMeshData(int index)
		{
			if (index < scene->numMeshes)
				return scene->meshes[index];
			return null;
		}

		public unsafe MaterialData? getMaterialData(int meshIndex)
		{
			if (meshIndex < scene->numMeshes)
				return scene->materials[scene->meshes[meshIndex].materialID];
			return null;
		}

		public unsafe AnimationData? getAnimationData(string name)
		{
			for (int i = 0; i < scene->numAnimations; i++)
			{
				if (StringUtils.CompareStrings(name, scene->animations[i].name))
					return scene->animations[i];
			}
			return null;
		}

		public unsafe int getMeshIndex(string name)
		{
			for (int i = 0; i < scene->numMeshes; i++)
			{
				int nodeID = scene->meshes[i].nodeID;
				NodeData* node = &scene->nodes[nodeID];
				if (StringUtils.CompareStrings(name, node->name))
					return i;
			}
			return -1;
		}

		public unsafe byte* getMeshName(int index)
		{
			return scene->meshes[index].node->name;
		}

		public unsafe int meshCount
		{
			get => scene->numMeshes;
		}

		public unsafe BoundingBox? boundingBox
		{
			get => scene->numMeshes > 0 ? scene->meshes[0].boundingBox : null;
		}

		public unsafe BoundingSphere? boundingSphere
		{
			get => scene->numMeshes > 0 ? scene->meshes[0].boundingSphere : null;
		}

		public unsafe int lightCount
		{
			get => scene->numLights;
		}

		public unsafe LightData getLight(int idx)
		{
			return scene->lights[idx];
		}

		public unsafe bool isAnimated
		{
			get => scene->numAnimations > 0;
		}

		public override bool Equals(object obj)
		{
			unsafe
			{
				if (obj is Model)
				{
					Model model = obj as Model;
					return model.scene == scene;
				}
				return false;
			}
		}

		public override unsafe int GetHashCode()
		{
			return ((IntPtr)scene).GetHashCode();
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe SceneData* Model_Create(int numVertices, PositionNormalTangent* vertices, Vector2* uvs, int numIndices, int* indices, MaterialData* material);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void Model_Destroy(SceneData* scene);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void Model_DrawMesh(int pass, SceneData* scene, int meshID, IntPtr shader, ref Matrix transform);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void Model_DrawMeshAnimated(int pass, SceneData* scene, int meshID, IntPtr shader, IntPtr animationState, ref Matrix transform);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void Model_Draw(int pass, SceneData* scene, IntPtr shader, IntPtr animatedShader, IntPtr animationState, ref Matrix transform);
	}
}