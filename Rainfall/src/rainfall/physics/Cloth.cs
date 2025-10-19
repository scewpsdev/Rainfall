using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ClothParams
	{
		public float stiffness = 1.0f;
		public float inertia = 1.0f;
		public Vector3 gravity = new Vector3(0, -9.81f, 0);

		public ClothParams(int _)
		{
		}
	}

	public unsafe class Cloth
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct ClothData
		{
			IntPtr cloth;
			IntPtr fabric;
			internal MeshData* mesh;

			internal Vector3 position;
			internal Quaternion rotation;
			internal UInt16 animatedVertexBuffer;
		}


		internal ClothData* handle;


		public unsafe Cloth(MeshData* mesh, Animator animator, float[] invMasses, ClothParams clothParams, Vector3 position, Quaternion rotation)
		{
			fixed (float* invMassesPtr = invMasses)
				handle = (ClothData*)Physics_CreateCloth(mesh, Matrix.Identity, animator != null ? animator.handle : IntPtr.Zero, invMassesPtr, clothParams, position, rotation);
		}

		public unsafe Cloth(Model model, int meshIdx, Animator animator, Vector3 position, Quaternion rotation, ClothParams clothParams)
		{
			MeshData* mesh = model.getMeshData(meshIdx);
			Node meshNode = model.skeleton.getNode(mesh->nodeID);
			float[] invMasses = createDefaultInvMasses(mesh);
			fixed (float* invMassesPtr = invMasses)
				handle = (ClothData*)Physics_CreateCloth(mesh, meshNode.transform, animator != null ? animator.handle : IntPtr.Zero, invMassesPtr, clothParams, position, rotation);
		}

		static unsafe float[] createDefaultInvMasses(MeshData* mesh)
		{
			float[] invMasses = new float[mesh->vertexCount];
			for (int i = 0; i < mesh->vertexCount; i++)
			{
				invMasses[i] = 1;
				if (mesh->vertexColors != null)
				{
					uint color = mesh->getVertexColor(i);
					uint r = (color & 0x00FF0000) >> 16;
					uint b = (color & 0x000000FF) >> 0;
					if (r == 0xFF && b == 0xFF)
					{
						uint g = (color & 0x0000FF00) >> 8;
						invMasses[i] = g == 255 ? 1 : 0; // g / 255.0f;
					}
				}
			}
			return invMasses;
		}

		public void destroy()
		{
			Physics_DestroyCloth((IntPtr)handle);
		}

		public void setTransform(Vector3 position, Quaternion rotation, bool teleport = false)
		{
			if (teleport)
				Physics_ClothSetTransform((IntPtr)handle, position, rotation);
			else
				Physics_ClothMoveTo((IntPtr)handle, position, rotation);
		}

		public unsafe Vector3 position
		{
			get => handle->position;
		}

		public unsafe Quaternion rotation
		{
			get => handle->rotation;
		}

		public unsafe void setSpheres(Span<Vector4> spheres, int first, int last)
		{
			fixed (Vector4* spheresPtr = spheres)
				Physics_ClothSetSpheres((IntPtr)handle, spheresPtr, spheres.Length, first, last);
		}

		public int numSpheres
		{
			get => Physics_ClothGetNumSpheres((IntPtr)handle);
		}

		public unsafe void setCapsules(Span<Vector2i> capsules, int first, int last)
		{
			fixed (Vector2i* capsulesPtr = capsules)
				Physics_ClothSetCapsules((IntPtr)handle, capsulesPtr, capsules.Length, first, last);
		}

		public int numCapsules
		{
			get => Physics_ClothGetNumCapsules((IntPtr)handle);
		}

		public static void SetWind(Vector3 wind)
		{
			Physics_ClothsSetWind(wind);
		}


		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void Physics_ClothsSetWind(Vector3 wind);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe IntPtr Physics_CreateCloth(MeshData* mesh, Matrix meshLocalTransform, IntPtr animator, float* invMasses, ClothParams clothParams, Vector3 position, Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void Physics_DestroyCloth(IntPtr cloth);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void Physics_ClothSetTransform(IntPtr cloth, Vector3 position, Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void Physics_ClothMoveTo(IntPtr cloth, Vector3 position, Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void Physics_ClothSetSpheres(IntPtr cloth, Vector4* spheres, int numSpheres, int first, int last);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern int Physics_ClothGetNumSpheres(IntPtr cloth);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void Physics_ClothSetCapsules(IntPtr cloth, Vector2i* capsules, int numCapsules, int first, int last);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern int Physics_ClothGetNumCapsules(IntPtr cloth);
	}
}
