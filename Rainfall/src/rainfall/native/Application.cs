﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Rainfall;

namespace Rainfall.Native
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void GameInit_t();
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void GameDestroy_t();
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void GameUpdate_t();
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void GameFixedUpdate_t(float delta);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void GameDraw_t();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void OnInternalErrorEvent_t([MarshalAs(UnmanagedType.LPStr)] string msg);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void OnAxisEvent_t(GamepadAxis axis, int value, ushort gamepadHandle);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void OnGamepadEvent_t(int gamepad, int ev);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void OnCharEvent_t(byte length, uint value);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void OnKeyEvent_t(KeyCode key, KeyModifier modifiers, bool down);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void OnMouseButtonEvent_t(MouseButton button, bool down);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void OnMouseMoveEvent_t(int x, int y, int z);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void OnViewportSizeEvent_t(int width, int height);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void OnDropFileEvent_t([MarshalAs(UnmanagedType.LPStr)] string filepath);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate byte OnExitEvent_t(byte windowExit);


	internal struct ApplicationCallbacks
	{
		internal GameInit_t init;
		internal GameDestroy_t destroy;
		internal GameUpdate_t update;
		internal GameFixedUpdate_t fixedUpdate;
		internal GameDraw_t draw;

		internal OnInternalErrorEvent_t onInternalError;
		internal OnAxisEvent_t onAxisEvent;
		internal OnGamepadEvent_t onGamepadEvent;
		internal OnCharEvent_t onCharEvent;
		internal OnKeyEvent_t onKeyEvent;
		internal OnMouseButtonEvent_t onMouseButtonEvent;
		internal OnMouseMoveEvent_t onMouseMoveEvent;
		internal OnViewportSizeEvent_t onViewportSizeEvent;
		internal OnDropFileEvent_t onDropFileEvent;
		internal OnExitEvent_t onExitEvent;
	}

	internal static class Application
	{
		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe void Application_GetTopAllocators(int num, byte* files, long* sizes);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_Run(LaunchParams launchParams, ApplicationCallbacks callbacks);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetTimerPaused(byte paused);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern long Application_GetCurrentTime();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern float Application_GetFrameTime();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int Application_GetFPS();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern float Application_GetMS();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern long Application_GetMemoryUsage();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int Application_GetNumAllocations();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern long Application_GetTimestamp();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern long Application_Terminate();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetDebugTextEnabled(byte enabled);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Application_IsDebugTextEnabled();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetDebugStatsEnabled(byte enabled);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Application_IsDebugStatsEnabled();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetDebugWireframeEnabled(byte enabled);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Application_IsDebugWireframeEnabled();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern byte Application_GetGamepadState(int gamepad, out GamepadState state);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetCursorMode(CursorMode mode);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetMousePosition(int x, int y);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetWindowVisible(bool visible);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetWindowPosition(int x, int y);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_GetWindowPosition(out int width, out int height);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetWindowSize(int width, int height);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetWindowTitle([MarshalAs(UnmanagedType.LPStr)] string title);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetWindowMaximized(bool maximized);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_ToggleFullscreen();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetVSync(int vsync);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_SetFpsCap(int fpsCap);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern CursorMode Application_GetCursorMode();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int Application_GetMonitorSize(out int width, out int height);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Application_CaptureFrame();
	}
}
