using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Rainfall.Native;

namespace Rainfall
{
	public enum KeyCode
	{
		None = 0,
		Esc,
		Return,
		Tab,
		Space,
		Backspace,
		Up,
		Down,
		Left,
		Right,
		Insert,
		Delete,
		Home,
		End,
		PageUp,
		PageDown,
		Print,
		Pause,
		Plus,
		Minus,
		Equal,
		CapsLock,
		ScrollLock,
		NumLock,
		Asterisk,
		LeftBracket,
		RightBracket,
		Semicolon,
		Quote,
		Comma,
		Period,
		Slash,
		Backslash,
		Tilde,
		Apostrophe,
		GraveAccent,
		F1,
		F2,
		F3,
		F4,
		F5,
		F6,
		F7,
		F8,
		F9,
		F10,
		F11,
		F12,
		Key0,
		Key1,
		Key2,
		Key3,
		Key4,
		Key5,
		Key6,
		Key7,
		Key8,
		Key9,
		A,
		B,
		C,
		D,
		E,
		F,
		G,
		H,
		I,
		J,
		K,
		L,
		M,
		N,
		O,
		P,
		Q,
		R,
		S,
		T,
		U,
		V,
		W,
		X,
		Y,
		Z,

		World1,
		World2,

		NumPad0,
		NumPad1,
		NumPad2,
		NumPad3,
		NumPad4,
		NumPad5,
		NumPad6,
		NumPad7,
		NumPad8,
		NumPad9,
		NumPadDecimal,
		NumPadDivide,
		NumPadMultiply,
		NumPadSubtract,
		NumPadAdd,
		NumPadEnter,
		NumPadEqual,
		Shift,
		Ctrl,
		Alt,
		Meta,
		RightShift,
		RightCtrl,
		RightAlt,
		RightMeta,
		Menu,

		Count
	}

	public enum MouseButton
	{
		None,

		Left,
		Right,
		Middle,
		Button4,
		Button5,

		Count
	}

	public enum KeyModifier : byte
	{
		None = 0,
		LeftAlt = 1 << 0,
		RightAlt = 1 << 1,
		LeftCtrl = 1 << 2,
		RightCtrl = 1 << 3,
		LeftShift = 1 << 4,
		RightShift = 1 << 5,
		LeftMeta = 1 << 6,
		RightMeta = 1 << 7,
	}

	public enum CursorMode : int
	{
		Normal,
		Hidden,
		Disabled,
	}

	public enum GamepadButton
	{
		None = -1,

		A = 0,
		B = 1,
		X = 2,
		Y = 3,
		BumperL = 4,
		BumperR = 5,
		Back = 6,
		Start = 7,
		Guide = 8,
		ThumbL = 9,
		ThumbR = 10,
		DPadUp = 11,
		DPadRight = 12,
		DPadDown = 13,
		DPadLeft = 14,

		Count
	}

	public enum GamepadAxis
	{
		LeftX,
		LeftY,
		LeftZ,
		RightX,
		RightY,
		RightZ,

		Count
	}

	public enum SuspendState
	{
		WillSuspend,
		DidSuspend,
		WillResume,
		DidResume,

		Count
	}

	struct KeyState
	{
		private uint _keys0;
		private uint _keys1;
		private uint _keys2;
		private uint _keys3;
		private uint _keys4;
		private uint _keys5;
		private uint _keys6;
		private uint _keys7;

		private byte _modifiers;

		public bool CapsLock => (_modifiers & 1) > 0;
		public bool NumLock => (_modifiers & 2) > 0;

		public bool this[KeyCode key]
		{
			get
			{
				return InternalGetKey(key);
			}
		}

		private bool InternalGetKey(KeyCode key)
		{
			uint num = (uint)(1 << (int)(key & (KeyCode)31));
			return ((uint)(((int)key >> 5) switch
			{
				0 => (int)_keys0,
				1 => (int)_keys1,
				2 => (int)_keys2,
				3 => (int)_keys3,
				4 => (int)_keys4,
				5 => (int)_keys5,
				6 => (int)_keys6,
				7 => (int)_keys7,
				_ => 0,
			}) & num) != 0;
		}

		internal void InternalSetKey(KeyCode key)
		{
			uint num = (uint)(1 << (int)(key & (KeyCode)31));
			switch ((int)key >> 5)
			{
				case 0:
					_keys0 |= num;
					break;
				case 1:
					_keys1 |= num;
					break;
				case 2:
					_keys2 |= num;
					break;
				case 3:
					_keys3 |= num;
					break;
				case 4:
					_keys4 |= num;
					break;
				case 5:
					_keys5 |= num;
					break;
				case 6:
					_keys6 |= num;
					break;
				case 7:
					_keys7 |= num;
					break;
			}
		}

		internal void InternalClearKey(KeyCode key)
		{
			uint num = (uint)(1 << (int)(key & (KeyCode)31));
			switch ((int)key >> 5)
			{
				case 0:
					_keys0 &= ~num;
					break;
				case 1:
					_keys1 &= ~num;
					break;
				case 2:
					_keys2 &= ~num;
					break;
				case 3:
					_keys3 &= ~num;
					break;
				case 4:
					_keys4 &= ~num;
					break;
				case 5:
					_keys5 &= ~num;
					break;
				case 6:
					_keys6 &= ~num;
					break;
				case 7:
					_keys7 &= ~num;
					break;
			}
		}

		internal void InternalClearAllKeys()
		{
			_keys0 = 0u;
			_keys1 = 0u;
			_keys2 = 0u;
			_keys3 = 0u;
			_keys4 = 0u;
			_keys5 = 0u;
			_keys6 = 0u;
			_keys7 = 0u;
		}

		internal KeyState(List<KeyCode> keys, bool capsLock = false, bool numLock = false)
		{
			this = default(KeyState);
			_keys0 = 0u;
			_keys1 = 0u;
			_keys2 = 0u;
			_keys3 = 0u;
			_keys4 = 0u;
			_keys5 = 0u;
			_keys6 = 0u;
			_keys7 = 0u;
			_modifiers = (byte)(0u | (capsLock ? 1u : 0u) | (numLock ? 2u : 0u));
			if (keys == null)
			{
				return;
			}

			foreach (KeyCode key in keys)
			{
				InternalSetKey(key);
			}
		}

		public KeyState(KeyCode[] keys, bool capsLock = false, bool numLock = false)
		{
			this = default(KeyState);
			_keys0 = 0u;
			_keys1 = 0u;
			_keys2 = 0u;
			_keys3 = 0u;
			_keys4 = 0u;
			_keys5 = 0u;
			_keys6 = 0u;
			_keys7 = 0u;
			_modifiers = (byte)(0u | (capsLock ? 1u : 0u) | (numLock ? 2u : 0u));
			if (keys != null)
			{
				foreach (KeyCode key in keys)
				{
					InternalSetKey(key);
				}
			}
		}

		public KeyState(params KeyCode[] keys)
		{
			this = default(KeyState);
			_keys0 = 0u;
			_keys1 = 0u;
			_keys2 = 0u;
			_keys3 = 0u;
			_keys4 = 0u;
			_keys5 = 0u;
			_keys6 = 0u;
			_keys7 = 0u;
			_modifiers = 0;
			if (keys != null)
			{
				foreach (KeyCode key in keys)
				{
					InternalSetKey(key);
				}
			}
		}

		public bool IsKeyDown(KeyCode key)
		{
			return InternalGetKey(key);
		}

		public int GetPressedKeyCount()
		{
			return (int)(CountBits(_keys0) + CountBits(_keys1) + CountBits(_keys2) + CountBits(_keys3) + CountBits(_keys4) + CountBits(_keys5) + CountBits(_keys6) + CountBits(_keys7));
		}

		private static uint CountBits(uint v)
		{
			v -= (v >> 1) & 0x55555555;
			v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
			return ((v + (v >> 4)) & 0xF0F0F0F) * 16843009 >> 24;
		}
	}

	struct MouseState
	{
		public int x;
		public int y;
		public int z;

		private int _scrollWheelValue;
		private int _horizontalScrollWheelValue;

		private byte _buttons;

		internal bool InternalGetButton(MouseButton button)
		{
			return (_buttons & (1 << (int)button)) > 0;
		}

		internal void InternalSetButton(MouseButton button)
		{
			_buttons |= (byte)(1 << (int)button);
		}

		internal void InternalClearButton(MouseButton button)
		{
			_buttons = (byte)(_buttons & (0xFFFFFFFF - (1 << (int)button)));
		}

		public bool IsButtonDown(MouseButton button)
		{
			return InternalGetButton(button);
		}

		/*
		public bool LeftButton
		{
			get
			{
				return (_buttons & 1) > 0;
			}
			internal set
			{
				if (value)
				{
					_buttons |= 1;
				}
				else
				{
					_buttons = (byte)(_buttons & 0xFFFFFFFEu);
				}
			}
		}

		public bool MiddleButton
		{
			get
			{
				return (_buttons & 4) > 0;
			}
			internal set
			{
				if (value)
				{
					_buttons |= 4;
				}
				else
				{
					_buttons = (byte)(_buttons & 0xFFFFFFFBu);
				}
			}
		}

		public bool RightButton
		{
			get
			{
				return (_buttons & 2) > 0;
			}
			internal set
			{
				if (value)
				{
					_buttons |= 2;
				}
				else
				{
					_buttons = (byte)(_buttons & 0xFFFFFFFDu);
				}
			}
		}
		*/

		public int ScrollWheelValue
		{
			get
			{
				return _scrollWheelValue;
			}
			internal set
			{
				_scrollWheelValue = value;
			}
		}

		public int HorizontalScrollWheelValue
		{
			get
			{
				return _horizontalScrollWheelValue;
			}
			internal set
			{
				_horizontalScrollWheelValue = value;
			}
		}

		/*
		public bool XButton1
		{
			get
			{
				return (_buttons & 8) > 0;
			}
			internal set
			{
				if (value)
				{
					_buttons |= 8;
				}
				else
				{
					_buttons = (byte)(_buttons & 0xFFFFFFF7u);
				}
			}
		}

		public bool XButton2
		{
			get
			{
				return (_buttons & 0x10) > 0;
			}
			internal set
			{
				if (value)
				{
					_buttons |= 16;
				}
				else
				{
					_buttons = (byte)(_buttons & 0xFFFFFFEFu);
				}
			}
		}
		*/
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct GamepadState
	{
		public fixed byte buttons[15];
		public fixed float axes[6];


		internal bool IsButtonDown(GamepadButton button)
		{
			return buttons[(int)button] != 0;
		}

		internal void InternalSetButton(GamepadButton button)
		{
			buttons[(int)button] = 1;
		}

		internal void InternalClearButton(GamepadButton button)
		{
			buttons[(int)button] = 0;
		}
	}

	public static class Input
	{
		static KeyState keysCurrent, keysLast;
		static MouseState mouseCurrent, mouseLast;
		static GamepadState gamepadCurrent, gamepadLast;

		static CursorMode _cursorMode = CursorMode.Normal;

		static bool mouseJustLocked = false;


		internal static void OnKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
		{
			if (down)
				keysCurrent.InternalSetKey(key);
			else
				keysCurrent.InternalClearKey(key);
		}

		internal static void OnMouseButtonEvent(MouseButton button, bool down)
		{
			if (down)
				mouseCurrent.InternalSetButton(button);
			else
				mouseCurrent.InternalClearButton(button);
		}

		internal static void OnMouseMoveEvent(int x, int y, int z)
		{
			mouseCurrent.x = x;
			mouseCurrent.y = y;
			mouseCurrent.z = z;

			if (mouseJustLocked)
			{
				mouseLast.x = x;
				mouseLast.y = y;
				mouseLast.z = z;
				mouseJustLocked = false;
			}
		}

		internal static void Update()
		{
			keysLast = keysCurrent;
			mouseLast = mouseCurrent;

			gamepadLast = gamepadCurrent;
			Application.Application_GetGamepadState(0, out gamepadCurrent);
		}

		public static bool IsKeyDown(KeyCode key)
		{
			return keysCurrent.IsKeyDown(key);
		}

		public static bool IsKeyPressed(KeyCode key)
		{
			return keysCurrent.IsKeyDown(key) && !keysLast.IsKeyDown(key);
		}

		public static bool IsKeyReleased(KeyCode key)
		{
			return !keysCurrent.IsKeyDown(key) && keysLast.IsKeyDown(key);
		}

		public static bool IsMouseButtonDown(MouseButton button)
		{
			return mouseCurrent.IsButtonDown(button);
		}

		public static bool IsMouseButtonPressed(MouseButton button, bool consume = false)
		{
			bool result = mouseCurrent.IsButtonDown(button) && !mouseLast.IsButtonDown(button);
			if (result && consume)
				ConsumeMouseButtonEvent(button);
			return result;
		}

		public static bool IsMouseButtonReleased(MouseButton button)
		{
			return !mouseCurrent.IsButtonDown(button) && mouseLast.IsButtonDown(button);
		}

		public static bool IsGamepadButtonDown(GamepadButton button)
		{
			return gamepadCurrent.IsButtonDown(button);
		}

		public static bool IsGamepadButtonPressed(GamepadButton button)
		{
			return gamepadCurrent.IsButtonDown(button) && !gamepadLast.IsButtonDown(button);
		}

		public static bool IsGamepadButtonReleased(GamepadButton button)
		{
			return !gamepadCurrent.IsButtonDown(button) && gamepadLast.IsButtonDown(button);
		}

		public static Vector2 GamepadAxis
		{
			get
			{
				unsafe
				{
					return new Vector2(gamepadCurrent.axes[0], gamepadCurrent.axes[1]);
				}
			}
		}

		public static Vector2 GamepadAxisRight
		{
			get
			{
				unsafe
				{
					return new Vector2(gamepadCurrent.axes[2], gamepadCurrent.axes[3]);
				}
			}
		}

		public static float GamepadTriggerLeft
		{
			get
			{
				unsafe
				{
					return gamepadCurrent.axes[4];
				}
			}
		}

		public static float GamepadTriggerRight
		{
			get
			{
				unsafe
				{
					return gamepadCurrent.axes[5];
				}
			}
		}

		public static void ConsumeKeyEvent(KeyCode key)
		{
			if (IsKeyPressed(key))
				keysLast.InternalSetKey(key);
			else if (IsKeyReleased(key))
				keysLast.InternalClearKey(key);
		}

		public static void ConsumeMouseButtonEvent(MouseButton button)
		{
			if (IsMouseButtonPressed(button))
				mouseLast.InternalSetButton(button);
			else if (IsMouseButtonReleased(button))
				mouseLast.InternalClearButton(button);
		}

		public static void ConsumeGamepadButtonEvent(GamepadButton button)
		{
			if (IsGamepadButtonPressed(button))
				gamepadLast.InternalSetButton(button);
			else if (IsGamepadButtonReleased(button))
				gamepadLast.InternalClearButton(button);
		}

		public static void ConsumeScrollEvent()
		{
			mouseLast.z = mouseCurrent.z;
		}

		public static CursorMode cursorMode
		{
			get { return _cursorMode; }
			set
			{
				Application.Application_SetCursorMode(value);
				_cursorMode = value;
				if (value == CursorMode.Disabled)
					mouseJustLocked = true;
			}
		}

		public static Vector2i cursorPosition
		{
			get { return new Vector2i(mouseCurrent.x, mouseCurrent.y); }
			set
			{
				Native.Application.Application_SetMousePosition(value.x, value.y);
			}
		}

		public static Vector2i cursorMove
		{
			get { return new Vector2i(mouseCurrent.x - mouseLast.x, mouseCurrent.y - mouseLast.y); }
		}

		public static int scrollPosition
		{
			get { return mouseCurrent.z; }
		}

		public static int scrollMove
		{
			get { return mouseCurrent.z - mouseLast.z; }
		}

		public static bool cursorHasMoved
		{
			get => mouseCurrent.x != mouseLast.x || mouseCurrent.y != mouseLast.y;
		}
	}
}
