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
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct BoundingSphere
	{
		public float xcenter, ycenter, zcenter;
		public float radius;

		public Vector3 center { get => new Vector3(xcenter, ycenter, zcenter); }
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MeshData
	{
		PositionNormalTangent* positionsNormalsTangents;
		Vector2* texcoords;
		uint* vertexColors;
		IntPtr boneWeights;

		public int vertexCount { get; internal set; }

		int* indexData;
		public int indexCount { get; internal set; }

		internal int materialID;
		internal int skeletonID;

		public BoundingBox boundingBox;
		public BoundingSphere boundingSphere;


		NodeData* node;

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
			return indexData[index];
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
	internal unsafe struct NodeData
	{
		internal int id;
		internal fixed byte name[32];
		internal Matrix transform;

		internal int numChildren;
		internal int* children;

		internal int numMeshes;
		internal int* meshes;


		internal NodeData* parent;
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
		internal NodeData* nodes;
		public IntPtr lights;
	}

	public class Model
	{
		internal IntPtr handle;

		public readonly Skeleton skeleton;

		public bool isStatic = true;


		internal Model(IntPtr handle)
		{
			this.handle = handle;
			skeleton = new Skeleton(handle);
		}

		public Model(int numVertices, Span<PositionNormalTangent> vertices, Span<Vector2> uvs, int numIndices, Span<int> indices, MaterialData material)
		{
			unsafe
			{
				fixed (PositionNormalTangent* verticesPtr = vertices)
				fixed (Vector2* uvsPtr = uvs)
				fixed (int* indicesPtr = indices)
				{
					handle = Model_Create(numVertices, verticesPtr, uvsPtr, numIndices, indicesPtr, &material);
				}
				skeleton = null;
			}
		}

		public unsafe SceneData* sceneDataHandle
		{
			get => Native.Resource.Resource_ModelGetSceneData(handle);
		}

		public void configureLODs(float maxDistance)
		{
			Model_ConfigureLODs(handle, maxDistance);
		}

		public float maxDistance
		{
			get => Model_GetMaxDistance(handle);
		}

		public MeshData? getMeshData(int index)
		{
			unsafe
			{
				SceneData* scene = (SceneData*)sceneDataHandle;
				if (index < scene->numMeshes)
					return scene->meshes[index];
				return null;
			}
		}

		public unsafe MaterialData? getMaterialData(int meshIndex)
		{
			SceneData* scene = (SceneData*)sceneDataHandle;
			if (meshIndex < scene->numMeshes)
				return scene->materials[scene->meshes[meshIndex].materialID];
			return null;
		}

		public AnimationData? getAnimationData(string name)
		{
			unsafe
			{
				SceneData* scene = (SceneData*)sceneDataHandle;
				for (int i = 0; i < scene->numAnimations; i++)
				{
					if (StringUtils.CompareStrings(name, scene->animations[i].name))
						return scene->animations[i];
				}
				return null;
			}
		}

		public int getMeshIndex(string name)
		{
			unsafe
			{
				SceneData* scene = (SceneData*)sceneDataHandle;
				for (int i = 0; i < scene->numMeshes; i++)
				{
					int nodeID = scene->meshes[i].nodeID;
					NodeData* node = &scene->nodes[nodeID];
					if (StringUtils.CompareStrings(name, node->name))
						return i;
				}
				return -1;
			}
		}

		public int meshCount
		{
			get
			{
				unsafe
				{
					SceneData* scene = (SceneData*)sceneDataHandle;
					return scene->numMeshes;
				}
			}
		}

		public BoundingBox? boundingBox
		{
			get
			{
				unsafe
				{
					SceneData* scene = (SceneData*)sceneDataHandle;
					if (scene->numMeshes > 0)
					{
						return scene->meshes[0].boundingBox;
					}
					return null;
				}
			}
		}

		public BoundingSphere? boundingSphere
		{
			get
			{
				unsafe
				{
					SceneData* scene = (SceneData*)sceneDataHandle;
					if (scene->numMeshes > 0)
					{
						return scene->meshes[0].boundingSphere;
					}
					return null;
				}
			}
		}

		public override bool Equals(object obj)
		{
			unsafe
			{
				if (obj is Model)
				{
					Model model = obj as Model;
					return model.sceneDataHandle == sceneDataHandle;
				}
				return false;
			}
		}

		public override int GetHashCode()
		{
			unsafe
			{
				return ((IntPtr)sceneDataHandle).GetHashCode();
			}
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe IntPtr Model_Create(int numVertices, PositionNormalTangent* vertices, Vector2* uvs, int numIndices, int* indices, MaterialData* material);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void Model_ConfigureLODs(IntPtr model, float maxDistance);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern float Model_GetMaxDistance(IntPtr model);
	}
}