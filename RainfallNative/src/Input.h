#pragma once

#include <stdint.h>


enum class KeyCode : int
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
};

enum class MouseButton : int
{
	None,

	Left,
	Right,
	Middle,
	Button4,
	Button5,

	Count
};

enum class KeyModifier : uint8_t
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
};

enum class CursorMode : int
{
	Normal,
	Hidden,
	Disabled,
};

enum class GamepadButton : int
{
	A,
	B,
	X,
	Y,
	ThumbL,
	ThumbR,
	ShoulderL,
	ShoulderR,
	Up,
	Down,
	Left,
	Right,
	Back,
	Start,
	Guide,

	Count
};

enum class GamepadAxis : int
{
	LeftX,
	LeftY,
	LeftZ,
	RightX,
	RightY,
	RightZ,

	Count
};

enum class SuspendState : int
{
	WillSuspend,
	DidSuspend,
	WillResume,
	DidResume,

	Count
};


inline KeyModifier operator|(KeyModifier m1, KeyModifier m2) { return (KeyModifier)((uint8_t)m1 | (uint8_t)m2); }
