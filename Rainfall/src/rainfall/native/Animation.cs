using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall.Native
{
	internal static class Animation
	{
		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe void Animation_AnimateNode(int nodeID, AnimationData* animation, float timer, byte looping, ref Matrix outTransform);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Animation_CreateAnimationState(SceneData* scene);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Animation_DestroyAnimationState(IntPtr state);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe void Animation_UpdateAnimationState(IntPtr state, SceneData* scene, Matrix[] nodeAnimationTransforms, int numNodes);
	}
}
