#include "Event.h"

#include "Application.h"


EventQueue::EventQueue()
	: m_queue(Application_GetAllocator())
{
}

EventQueue::~EventQueue()
{
	for (const Event* ev = poll(); NULL != ev; ev = poll())
	{
		release(ev);
	}
}

void EventQueue::postAxisEvent(GamepadHandle gamepad, GamepadAxis axis, int32_t value)
{
	AxisEvent* ev = BX_NEW(Application_GetAllocator(), AxisEvent)();
	ev->gamepad = gamepad;
	ev->axis = axis;
	ev->value = value;
	m_queue.push(ev);
}

void EventQueue::postCharEvent(uint8_t _len, uint32_t _value)
{
	CharEvent* ev = BX_NEW(Application_GetAllocator(), CharEvent)();
	ev->length = _len;
	ev->value = _value;
	m_queue.push(ev);
}

void EventQueue::postExitEvent()
{
	Event* ev = BX_NEW(Application_GetAllocator(), Event)(EventType::Exit);
	m_queue.push(ev);
}

void EventQueue::postGamepadEvent(GamepadHandle _gamepad, bool _connected)
{
	GamepadEvent* ev = BX_NEW(Application_GetAllocator(), GamepadEvent)();
	ev->gamepad = _gamepad;
	ev->connected = _connected;
	m_queue.push(ev);
}

void EventQueue::postKeyEvent(KeyCode _key, uint8_t _modifiers, bool _down)
{
	KeyEvent* ev = BX_NEW(Application_GetAllocator(), KeyEvent)();
	ev->key = _key;
	ev->modifiers = _modifiers;
	ev->down = _down;
	m_queue.push(ev);
}

void EventQueue::postMouseEvent(int32_t _mx, int32_t _my, int32_t _mz)
{
	MouseEvent* ev = BX_NEW(Application_GetAllocator(), MouseEvent)();
	ev->x = _mx;
	ev->y = _my;
	ev->z = _mz;
	ev->button = MouseButton::None;
	ev->down = false;
	ev->move = true;
	m_queue.push(ev);
}

void EventQueue::postMouseEvent(int32_t _mx, int32_t _my, int32_t _mz, MouseButton _button, bool _down)
{
	MouseEvent* ev = BX_NEW(Application_GetAllocator(), MouseEvent)();
	ev->x = _mx;
	ev->y = _my;
	ev->z = _mz;
	ev->button = _button;
	ev->down = _down;
	ev->move = false;
	m_queue.push(ev);
}

void EventQueue::postSizeEvent(uint32_t _width, uint32_t _height)
{
	SizeEvent* ev = BX_NEW(Application_GetAllocator(), SizeEvent)();
	ev->width = _width;
	ev->height = _height;
	m_queue.push(ev);
}

void EventQueue::postWindowEvent(void* _nwh)
{
	WindowEvent* ev = BX_NEW(Application_GetAllocator(), WindowEvent)();
	ev->window = _nwh;
	m_queue.push(ev);
}

void EventQueue::postSuspendEvent(SuspendState _state)
{
	SuspendEvent* ev = BX_NEW(Application_GetAllocator(), SuspendEvent)();
	ev->state = _state;
	m_queue.push(ev);
}

void EventQueue::postDropFileEvent(const bx::FilePath& _filePath)
{
	DropFileEvent* ev = BX_NEW(Application_GetAllocator(), DropFileEvent)();
	ev->filepath = _filePath;
	m_queue.push(ev);
}

const Event* EventQueue::poll()
{
	return m_queue.pop();
}

void EventQueue::release(const Event* _event) const
{
	BX_DELETE(Application_GetAllocator(), const_cast<Event*>(_event));
}
