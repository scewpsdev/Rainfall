using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	// call it when you want a gizmo
	// Needs view and projection matrices. 
	// matrix parameter is the source matrix (where will be gizmo be drawn) and might be transformed by the function. Return deltaMatrix is optional
	// translation is applied in world space
	public enum GuizmoManipulateOperation : uint
	{
		TRANSLATE_X = (1u << 0),
		TRANSLATE_Y = (1u << 1),
		TRANSLATE_Z = (1u << 2),
		ROTATE_X = (1u << 3),
		ROTATE_Y = (1u << 4),
		ROTATE_Z = (1u << 5),
		ROTATE_SCREEN = (1u << 6),
		SCALE_X = (1u << 7),
		SCALE_Y = (1u << 8),
		SCALE_Z = (1u << 9),
		BOUNDS = (1u << 10),
		TRANSLATE = TRANSLATE_X | TRANSLATE_Y | TRANSLATE_Z,
		ROTATE = ROTATE_X | ROTATE_Y | ROTATE_Z | ROTATE_SCREEN,
		SCALE = SCALE_X | SCALE_Y | SCALE_Z
	}

	public enum GuizmoManipulateMode : uint
	{
		LOCAL,
		WORLD
	}

	public static class ImGuizmo
	{
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetDrawlist(void* drawlist = null);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ImGuizmoSetRect")]
		public static extern unsafe void SetRect(float x, float y, float width, float height);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetOrthographic([MarshalAs(UnmanagedType.I1)] bool isOrthographic);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void DrawCubes(ref float view, ref float projection, ref float matrices, int matrixCount);
		public static void DrawCubes(Matrix view, Matrix projection, Matrix transform)
		{
			DrawCubes(ref view.m00, ref projection.m00, ref transform.m00, 1);
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void DrawGrid(ref float view, ref float projection, ref float matrix, float gridSize);
		public static void DrawGrid(Matrix view, Matrix projection, Matrix transform, float gridSize)
		{
			DrawGrid(ref view.m00, ref projection.m00, ref transform.m00, gridSize);
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern unsafe bool Manipulate(float* view, float* projection, GuizmoManipulateOperation operation, GuizmoManipulateMode mode, float* matrix, float* deltaMatrix = null, float* snap = null, float* localBounds = null, float* boundsSnap = null);
		public static unsafe bool Manipulate(Matrix view, Matrix projection, GuizmoManipulateOperation operation, GuizmoManipulateMode mode, ref Matrix matrix, float* deltaMatrix = null, Vector3? snap = null)
		{
			fixed (float* matrixPtr = &matrix.m00)
			{
				if (snap.HasValue)
				{
					Vector3 snapValue = snap.Value;
					return Manipulate(&view.m00, &projection.m00, operation, mode, matrixPtr, null, &snapValue.x);
				}
				else
				{
					return Manipulate(&view.m00, &projection.m00, operation, mode, matrixPtr, null, null);
				}
			}
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void ViewManipulate(float* view, float length, Vector2 position, Vector2 size, uint backgroundColor);
		public static unsafe void ViewManipulate(ref Matrix view, float length, Vector2 position, Vector2 size, uint backgroundColor)
		{
			fixed (float* viewPtr = &view.m00)
				ViewManipulate(viewPtr, length, position, size, backgroundColor);
		}
	}
}
