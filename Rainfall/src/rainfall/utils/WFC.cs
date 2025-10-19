using Rainfall.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


[StructLayout(LayoutKind.Sequential)]
public struct WFCOptions
{
	byte _periodicInput;  // True if the input is toric.
	byte _periodicOutput; // True if the output is toric.
	public int outHeight;  // The height of the output in pixels.
	public int outWidth;   // The width of the output in pixels.
	public int symmetry; // The number of symmetries (the order is defined in wfc).
	byte _ground;       // True if the ground needs to be set (see init_ground).
	public int patternSize; // The width and height in pixel of the patterns.

	public bool periodicInput
	{
		get => _periodicInput != 0;
		set { _periodicInput = (byte)(value ? 1 : 0); }
	}

	public bool periodicOutput
	{
		get => _periodicOutput != 0;
		set { _periodicOutput = (byte)(value ? 1 : 0); }
	}

	public bool ground
	{
		get => _ground != 0;
		set { _ground = (byte)(value ? 1 : 0); }
	}
}

public static class WFC
{
	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	internal static extern unsafe byte WFC_Run(WFCOptions options, uint seed, int* input, int inputWidth, int inputHeight, int* output);

	public static unsafe bool Run(WFCOptions options, uint seed, int* input, int width, int height, int* output)
	{
		return WFC_Run(options, seed, input, width, height, output) != 0;
	}

	public static bool Run(WFCOptions options, uint seed, Span<int> input, int width, int height, Span<int> output)
	{
		unsafe
		{
			fixed (int* inputPtr = input, outputPtr = output)
			{
				return WFC_Run(options, seed, inputPtr, width, height, outputPtr) != 0;
			}
		}
	}

	public static bool Run(WFCOptions options, uint seed, Span<byte> input, int width, int height, Span<uint> output)
	{
		unsafe
		{
			fixed (byte* inputPtr = input)
			{
				fixed (uint* outputPtr = output)
				{
					return WFC_Run(options, seed, (int*)inputPtr, width, height, (int*)outputPtr) != 0;
				}
			}
		}
	}
}
