using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct LaunchParams
	{
		public int width = 1280;
		public int height = 720;
		[MarshalAs(UnmanagedType.LPStr)] public string title = "";
		private byte _maximized = 0;
		private byte _fullscreen = 0;

		public int fpsCap = 0;
		public int vsync = 0;


		public LaunchParams(string[] args)
		{
			//argv = args;
			//argc = args.Length;
		}

		public bool maximized
		{
			get { return _maximized != 0; }
			set { _maximized = (byte)(value ? 1 : 0); }
		}

		public bool fullscreen
		{
			get { return _fullscreen != 0; }
			set { _fullscreen = (byte)(value ? 1 : 0); }
		}
	}

	public abstract class Game
	{
		public static Game instance { get; private set; } = null;

		static void Game_Init()
		{
			Display.Init(instance.launchParams);
			instance.init();
		}

		static void Game_Destroy()
		{
			instance.destroy();
		}

		static void Game_Update()
		{
			instance.update();
		}

		static void Game_Draw()
		{
			instance.draw();

			Input.Update();
		}

		static void Game_OnInternalErrorEvent(string msg)
		{
			System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
			Console.Error.WriteLine("Internal error occured: " + msg);
			Console.Error.WriteLine(stackTrace.ToString());
		}

		static void Game_OnAxisEvent(GamepadAxis axis, int value, ushort gamepadHandle)
		{
			instance.onAxisEvent(axis, value, gamepadHandle);
		}

		static void Game_OnGamepadEvent(ushort gamepadHandle, bool connected)
		{
			instance.onGamepadEvent(gamepadHandle, connected);
		}

		static void Game_OnCharEvent(byte length, uint value)
		{
			instance.onCharEvent(length, value);
		}

		static void Game_OnKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
		{
			Input.OnKeyEvent(key, modifiers, down);
			instance.onKeyEvent(key, modifiers, down);
		}

		static void Game_OnMouseButtonEvent(MouseButton button, bool down)
		{
			Input.OnMouseButtonEvent(button, down);
			instance.onMouseButtonEvent(button, down);
		}

		static void Game_OnMouseMoveEvent(int x, int y, int z)
		{
			Input.OnMouseMoveEvent(x, y, z);
			instance.onMouseMoveEvent(x, y, z);
		}

		static void Game_OnViewportSizeEvent(int width, int height)
		{
			Display.OnViewportSizeEvent(width, height);
			instance.onViewportSizeEvent(width, height);
		}

		static void Game_OnDropFileEvent([MarshalAs(UnmanagedType.LPStr)] string filepath)
		{
			instance.onDropFileEvent(filepath);
		}

		static byte Game_OnExitEvent(byte windowExit)
		{
			return (byte)(instance.onExitEvent(windowExit != 0) ? 1 : 0);
		}


		LaunchParams launchParams;

		public GraphicsDevice graphics { get; private set; }

		public Game()
		{
			Debug.Assert(instance == null);
			instance = this;

			graphics = new GraphicsDevice();
		}

		public void run(LaunchParams launchParams)
		{
			this.launchParams = launchParams;

			Native.ApplicationCallbacks callbacks = new Native.ApplicationCallbacks();
			callbacks.init = Game_Init;
			callbacks.destroy = Game_Destroy;
			callbacks.update = Game_Update;
			callbacks.draw = Game_Draw;
			callbacks.onInternalError = Game_OnInternalErrorEvent;
			callbacks.onAxisEvent = Game_OnAxisEvent;
			callbacks.onGamepadEvent = Game_OnGamepadEvent;
			callbacks.onCharEvent = Game_OnCharEvent;
			callbacks.onKeyEvent = Game_OnKeyEvent;
			callbacks.onMouseButtonEvent = Game_OnMouseButtonEvent;
			callbacks.onMouseMoveEvent = Game_OnMouseMoveEvent;
			callbacks.onViewportSizeEvent = Game_OnViewportSizeEvent;
			callbacks.onDropFileEvent = Game_OnDropFileEvent;
			callbacks.onExitEvent = Game_OnExitEvent;

			Native.Application.Application_Run(launchParams, callbacks);
		}

		public void terminate()
		{
			Native.Application.Application_Terminate();
		}

		public abstract void init();
		public abstract void destroy();
		public abstract void update();
		public abstract void draw();

		protected virtual void onAxisEvent(GamepadAxis axis, int value, ushort gamepadHandle)
		{
		}

		protected virtual void onGamepadEvent(ushort gamepadHandle, bool connected)
		{
		}

		protected virtual void onCharEvent(byte length, uint value)
		{
		}

		protected virtual void onKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
		{
		}

		protected virtual void onMouseButtonEvent(MouseButton button, bool down)
		{
		}

		protected virtual void onMouseMoveEvent(int x, int y, int z)
		{
		}

		protected virtual void onViewportSizeEvent(int width, int height)
		{
		}

		protected virtual void onDropFileEvent([MarshalAs(UnmanagedType.LPStr)] string filepath)
		{
		}

		protected virtual bool onExitEvent(bool windowExit)
		{
			return true;
		}
	}
}
