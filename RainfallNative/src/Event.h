#pragma once

#include "Input.h"

#include <bx/allocator.h>
#include <bx/filepath.h>
#include <bx/spscqueue.h>


enum class EventType : int
{
	Axis,
	Char,
	Exit,
	Gamepad,
	Key,
	Mouse,
	Size,
	Window,
	Suspend,
	DropFile,
};

struct Event
{
	EventType type;


	Event(EventType type)
		: type(type)
	{
	}
};

struct AxisEvent : public Event
{
	GamepadAxis axis;
	int32_t value;
	int gamepad;


	AxisEvent()
		: Event(EventType::Axis)
	{
	}
};

struct CharEvent : public Event
{
	uint8_t length;
	uint32_t value;


	CharEvent()
		: Event(EventType::Char)
	{
	}
};

struct GamepadEvent : public Event
{
	int gamepad;
	int event;


	GamepadEvent()
		: Event(EventType::Gamepad)
	{
	}
};

struct KeyEvent : public Event
{
	KeyCode key;
	uint8_t modifiers;
	bool down;


	KeyEvent()
		: Event(EventType::Key)
	{
	}
};

struct MouseEvent : public Event
{
	int32_t x;
	int32_t y;
	int32_t z;
	MouseButton button;
	bool down;
	bool move;


	MouseEvent()
		: Event(EventType::Mouse)
	{
	}
};

struct SizeEvent : public Event
{
	uint32_t width;
	uint32_t height;


	SizeEvent()
		: Event(EventType::Size)
	{
	}
};

struct WindowEvent : public Event
{
	void* window;


	WindowEvent()
		: Event(EventType::Window)
	{
	}
};

struct SuspendEvent : public Event
{
	SuspendState state;


	SuspendEvent()
		: Event(EventType::Suspend)
	{
	}
};

struct DropFileEvent : public Event
{
	bx::FilePath filepath;


	DropFileEvent()
		: Event(EventType::DropFile)
	{
	}
};

struct EventQueue
{
	bx::SpScUnboundedQueueT<Event> m_queue;


	EventQueue();
	~EventQueue();

	void postAxisEvent(int gamepad, GamepadAxis axis, int32_t value);
	void postCharEvent(uint8_t _len, uint32_t _value);
	void postExitEvent();
	void postGamepadEvent(int _gamepad, int event);
	void postKeyEvent(KeyCode _key, uint8_t _modifiers, bool _down);
	void postMouseEvent(int32_t _mx, int32_t _my, int32_t _mz);
	void postMouseEvent(int32_t _mx, int32_t _my, int32_t _mz, MouseButton _button, bool _down);
	void postSizeEvent(uint32_t _width, uint32_t _height);
	void postWindowEvent(void* _nwh = NULL);
	void postSuspendEvent(SuspendState _state);
	void postDropFileEvent(const bx::FilePath& _filePath);

	const Event* poll();
	void release(const Event* _event) const;
};
