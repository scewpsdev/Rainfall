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
		internal static extern IntPtr Animation_CreateAnimationState(IntPtr model);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Animation_UpdateAnimationState(IntPtr state, IntPtr model, Matrix[] nodeAnimationTransforms, int numNodes);
	}
}
