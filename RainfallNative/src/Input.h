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
	KeyA,
	KeyB,
	KeyC,
	KeyD,
	KeyE,
	KeyF,
	KeyG,
	KeyH,
	KeyI,
	KeyJ,
	KeyK,
	KeyL,
	KeyM,
	KeyN,
	KeyO,
	KeyP,
	KeyQ,
	KeyR,
	KeyS,
	KeyT,
	KeyU,
	KeyV,
	KeyW,
	KeyX,
	KeyY,
	KeyZ,

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
	LeftShift,
	LeftCtrl,
	LeftAlt,
	LeftMeta,
	RightShift,
	RightCtrl,
	RightAlt,
	RightMeta,
	Menu,

	GamepadA,
	GamepadB,
	GamepadX,
	GamepadY,
	GamepadThumbL,
	GamepadThumbR,
	GamepadShoulderL,
	GamepadShoulderR,
	GamepadUp,
	GamepadDown,
	GamepadLeft,
	GamepadRight,
	GamepadBack,
	GamepadStart,
	GamepadGuide,

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

struct GamepadHandle { uint16_t idx; };
inline bool isValid(GamepadHandle handle) { return handle.idx != UINT16_MAX; }


inline KeyModifier operator|(KeyModifier m1, KeyModifier m2) { return (KeyModifier)((uint8_t)m1 | (uint8_t)m2); }
