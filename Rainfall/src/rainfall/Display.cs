using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public static class Display
	{
		public static int width { get; private set; }
		public static int height { get; private set; }
		static bool visible;
		static bool maximized;
		static bool _fullscreen;
		static int _vsync;
		static int _fpsCap;


		internal static void Init(LaunchParams launchParams)
		{
			width = launchParams.width;
			height = launchParams.height;
			visible = true;
			maximized = launchParams.maximized;
			_fullscreen = launchParams.fullscreen;
			_vsync = launchParams.vsync;
			_fpsCap = launchParams.fpsCap;
		}

		internal static void OnViewportSizeEvent(int newWidth, int newHeight)
		{
			width = newWidth;
			height = newHeight;
		}

		public static bool windowVisible
		{
			get { return visible; }
			set
			{
				if (value != visible)
				{
					Native.Application.Application_SetWindowVisible(value);
					visible = value;
				}
			}
		}

		public static Vector2i windowPosition
		{
			get
			{
				Native.Application.Application_GetWindowPosition(out int width, out int height);
				return new Vector2i(width, height);
			}
			set
			{
				Native.Application.Application_SetWindowPosition(value.x, value.y);
			}
		}

		public static string windowTitle
		{
			set
			{
				Native.Application.Application_SetWindowTitle(value);
			}
		}

		public static Vector2i viewportSize
		{
			get { return new Vector2i(width, height); }
			set
			{
				Native.Application.Application_SetWindowSize(value.x, value.y);
			}
		}

		public static float aspectRatio
		{
			get { return (float)width / height; }
		}

		public static Vector2i monitorSize
		{
			get
			{
				Native.Application.Application_GetMonitorSize(out int width, out int height);
				return new Vector2i(width, height);
			}
		}

		public static bool windowMaximized
		{
			get { return maximized; }
			set
			{
				Native.Application.Application_SetWindowMaximized(value);
				maximized = value;
			}
		}

		public static bool fullscreen
		{
			get { return _fullscreen; }
			set
			{
				if (_fullscreen != value)
				{
					Native.Application.Application_ToggleFullscreen();
					_fullscreen = value;
				}
			}
		}

		public static int vsync
		{
			get { return _vsync; }
			set
			{
				Native.Application.Application_SetVSync(value);
				_vsync = value;
			}
		}

		public static int fpsCap
		{
			get { return _fpsCap; }
			set
			{
				Native.Application.Application_SetFpsCap(value);
				_fpsCap = value;
			}
		}

		public static void ToggleFullscreen()
		{
			Native.Application.Application_ToggleFullscreen();
			_fullscreen = !_fullscreen;
		}
	}
}
