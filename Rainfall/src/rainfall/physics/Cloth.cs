using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Rainfall
{
	public class Cloth
	{
		[StructLayout(LayoutKind.Sequential)]
		struct ClothData
		{
			IntPtr cloth;
			IntPtr fabric;
			IntPtr mesh;

			internal Vector3 position;
			internal Quaternion rotation;
			UInt16 animatedVertexBuffer;
		}


		internal IntPtr handle;


		public unsafe Cloth(Model model, float[] invMasses, Vector3 position, Quaternion rotation)
		{
			fixed (float* invMassesPtr = invMasses)
				handle = Physics_CreateCloth(model.getMeshData(0), invMassesPtr, position, rotation);
		}

		public void destroy()
		{
			Physics_DestroyCloth(handle);
		}

		public void setTransform(Vector3 position, Quaternion rotation, bool teleport = false)
		{
			if (teleport)
				Physics_ClothSetTransform(handle, position, rotation);
			else
				Physics_ClothMoveTo(handle, position, rotation);
		}

		public unsafe Vector3 position
		{
			get => ((ClothData*)handle)->position;
		}

		public unsafe Quaternion rotation
		{
			get => ((ClothData*)handle)->rotation;
		}

		public unsafe void setSpheres(Span<Vector4> spheres, int first, int last)
		{
			fixed (Vector4* spheresPtr = spheres)
				Physics_ClothSetSpheres(handle, spheresPtr, spheres.Length, first, last);
		}

		public int numSpheres
		{
			get => Physics_ClothGetNumSpheres(handle);
		}

		public unsafe void setCapsules(Span<Vector2i> capsules, int first, int last)
		{
			fixed (Vector2i* capsulesPtr = capsules)
				Physics_ClothSetCapsules(handle, capsulesPtr, capsules.Length, first, last);
		}

		public int numCapsules
		{
			get => Physics_ClothGetNumCapsules(handle);
		}

		public static void SetWind(Vector3 wind)
		{
			Physics_ClothsSetWind(wind);
		}


		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void Physics_ClothsSetWind(Vector3 wind);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe IntPtr Physics_CreateCloth(MeshData* mesh, float* invMasses, Vector3 position, Quaternion rotation);

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
