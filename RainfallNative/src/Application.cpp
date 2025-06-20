#include "Application.h"

#include "Event.h"
#include "Input.h"
#include "Console.h"
#include "Hash.h"
#include "Platform.h"
#include "Resource.h"

#include "audio/Audio.h"

#include "physics/Physics.h"

#include "utils/ImGuiLayer.h"

#include <mutex>
#include <unordered_map>
#include <map>
#include <algorithm>
#include <iterator>

#include <bx/bx.h>
#include <bx/thread.h>
#include <bx/allocator.h>
#include <bx/file.h>

#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>

#if GLFW_VERSION_MINOR < 2
#	error "GLFW 3.2 or later is required"
#endif // GLFW_VERSION_MINOR < 2

#if BX_PLATFORM_LINUX || BX_PLATFORM_BSD
#	if ENTRY_CONFIG_USE_WAYLAND
#		include <wayland-egl.h>
#		define GLFW_EXPOSE_NATIVE_WAYLAND
#	else
#		define GLFW_EXPOSE_NATIVE_X11
#		define GLFW_EXPOSE_NATIVE_GLX
#	endif
#elif BX_PLATFORM_OSX
#	define GLFW_EXPOSE_NATIVE_COCOA
#	define GLFW_EXPOSE_NATIVE_NSGL
#elif BX_PLATFORM_WINDOWS
#	define GLFW_EXPOSE_NATIVE_WIN32
#	define GLFW_EXPOSE_NATIVE_WGL
#endif //
#include <GLFW/glfw3native.h>

#include <bgfx/platform.h>


struct LaunchParams
{
	int width;
	int height;
	const char* title;
	bool maximized;
	bool fullscreen;

	int fpsCap;
	int vsync;
};

struct ApplicationCallbacks
{
	void (*init)();
	void (*destroy)();
	void (*update)();
	void (*fixedUpdate)(float delta);
	void (*draw)();

	void (*onInternalErrorEvent)(const char* msg);
	void (*onAxisEvent)(GamepadAxis axis, int value, int gamepad);
	void (*onGamepadEvent)(int gamepad, int event);
	void (*onCharEvent)(uint8_t length, uint32_t value);
	void (*onKeyEvent)(KeyCode key, KeyModifier modifier, bool down);
	void (*onMouseButtonEvent)(MouseButton button, bool down);
	void (*onMouseMoveEvent)(int x, int y, int z);
	void (*onViewportSizeEvent)(int width, int height);
	void (*onDropFileEvent)(const char* filepath);
	bool (*onExitEvent)(bool windowExit);
};

enum class MessageType
{
	WindowCreate,
	WindowDestroy,
	WindowShow,
	WindowSetTitle,
	WindowSetPos,
	WindowSetSize,
	WindowToggleFrame,
	WindowSetMaximized,
	WindowToggleFullscreen,
	WindowSetVSync,
	WindowSetFpsCap,
	MouseSetPos,
	MouseLock,
};

struct Message
{
	MessageType type;
	int x, y;
	int width, height;
	uint32_t flags;
	int value;
	char title[32];

	Message(MessageType type)
		: type(type), x(0), y(0), width(0), height(0), value(false)
	{
	}
};


GLFWwindow* window = nullptr;
int windowX, windowY;
int windowWidth, windowHeight;
bool isFullscreen = false;

double scrollPos = 0.0;

bx::SpScUnboundedQueueT<Message> messageQueue(Application_GetAllocator());

bx::Thread gameThread;
static bool keepRunning;

uint32_t width, height;
uint32_t reset;
uint32_t debug;
uint32_t frameIdx;
bool captureFrame = false;

int fpsCap;
int vsync;
CursorMode cursorMode = CursorMode::Normal;

int64_t currentFrame = 0;
float delta = 0;
int fps = 0;
float ms = 0.0f;
bool isPaused = false;

float timeAccumulator = 0.0f;
int frameCounter = 0;
float msCounter = 0.0f;
int64_t lastFrame;
int64_t lastSecond;

EventQueue eventQueue;

static KeyCode keyTranslation[GLFW_KEY_LAST + 1];


struct MemoryAllocator : bx::DefaultAllocator
{
#if TRACK_ALLOCATIONS
public:
	std::unordered_map<void*, int64_t> allocations;
	int64_t numBytes = 0;

	std::mutex mutex;


	inline int getNumBytes() { return (int)numBytes; }
	inline int getNumAllocations() { return (int)allocations.size(); }
#else
	inline int getNumBytes() { return 0; }
	inline int getNumAllocations() { return 0; }
#endif


	virtual void* realloc(
		void* ptr
		, size_t size
		, size_t align
		, const char* file
		, uint32_t line
	) override
	{
		void* result = bx::DefaultAllocator::realloc(ptr, size, align, file, line);
		if (!ptr) // malloc
			; // memset(result, 0, size);
		else if (size == 0)
			; // free
		else
		{
			// realloc
			int a = 5;
		}

#if TRACK_ALLOCATIONS
		if (size == 0)
		{
			if (ptr)
			{
				mutex.lock();
				auto it = allocations.find(ptr);
				if (it != allocations.end())
				{
					numBytes -= allocations.at(ptr);
					allocations.erase(ptr);
				}
				else
				{
					Console_Error("Freeing unrecognized pointer %x, f: %f, %d, %c", ptr, *(float*)ptr, *(int*)ptr, *(char*)ptr);
				}
				mutex.unlock();
			}
		}
		else if (!ptr)
		{
			mutex.lock();
			allocations.emplace(result, size);
			numBytes += size;
			mutex.unlock();
		}
		else
		{
			mutex.lock();
			int64_t& s = allocations.at(ptr);
			numBytes += size - s;
			s = size;
			mutex.unlock();
		}
#endif

		return result;
	}
};

bx::AllocatorI* Application_GetAllocator()
{
	BX_PRAGMA_DIAGNOSTIC_PUSH();
	BX_PRAGMA_DIAGNOSTIC_IGNORED_MSVC(4459); // warning C4459: declaration of 's_allocator' hides global declaration
	BX_PRAGMA_DIAGNOSTIC_IGNORED_CLANG_GCC("-Wshadow");
	static MemoryAllocator s_allocator;
	return &s_allocator;
	BX_PRAGMA_DIAGNOSTIC_POP();
}

RFAPI void Application_GetTopAllocators(int num, char* files, int64_t* sizes)
{
#if TRACK_ALLOCATION_FILES
	MemoryAllocator* allocator = (MemoryAllocator*)Application_GetAllocator();
	auto it = allocator->fileMapSorted.end();
	for (int i = 0; i < num; i++)
	{
		it--;
		int64_t size = it->first;
		const char* file = it->second;
		strcpy(&files[i * 128], file);
		sizes[i] = size;
	}
#endif
}


static void InitKeyTranslation()
{
	keyTranslation[GLFW_KEY_SPACE] = KeyCode::Space;
	keyTranslation[GLFW_KEY_APOSTROPHE] = KeyCode::Apostrophe;
	keyTranslation[GLFW_KEY_COMMA] = KeyCode::Comma;
	keyTranslation[GLFW_KEY_MINUS] = KeyCode::Minus;
	keyTranslation[GLFW_KEY_PERIOD] = KeyCode::Period;
	keyTranslation[GLFW_KEY_SLASH] = KeyCode::Slash;
	keyTranslation[GLFW_KEY_0] = KeyCode::Key0;
	keyTranslation[GLFW_KEY_1] = KeyCode::Key1;
	keyTranslation[GLFW_KEY_2] = KeyCode::Key2;
	keyTranslation[GLFW_KEY_3] = KeyCode::Key3;
	keyTranslation[GLFW_KEY_4] = KeyCode::Key4;
	keyTranslation[GLFW_KEY_5] = KeyCode::Key5;
	keyTranslation[GLFW_KEY_6] = KeyCode::Key6;
	keyTranslation[GLFW_KEY_7] = KeyCode::Key7;
	keyTranslation[GLFW_KEY_8] = KeyCode::Key8;
	keyTranslation[GLFW_KEY_9] = KeyCode::Key9;
	keyTranslation[GLFW_KEY_SEMICOLON] = KeyCode::Semicolon;
	keyTranslation[GLFW_KEY_EQUAL] = KeyCode::Equal;
	keyTranslation[GLFW_KEY_A] = KeyCode::A;
	keyTranslation[GLFW_KEY_B] = KeyCode::B;
	keyTranslation[GLFW_KEY_C] = KeyCode::C;
	keyTranslation[GLFW_KEY_D] = KeyCode::D;
	keyTranslation[GLFW_KEY_E] = KeyCode::E;
	keyTranslation[GLFW_KEY_F] = KeyCode::F;
	keyTranslation[GLFW_KEY_G] = KeyCode::G;
	keyTranslation[GLFW_KEY_H] = KeyCode::H;
	keyTranslation[GLFW_KEY_I] = KeyCode::I;
	keyTranslation[GLFW_KEY_J] = KeyCode::J;
	keyTranslation[GLFW_KEY_K] = KeyCode::K;
	keyTranslation[GLFW_KEY_L] = KeyCode::L;
	keyTranslation[GLFW_KEY_M] = KeyCode::M;
	keyTranslation[GLFW_KEY_N] = KeyCode::N;
	keyTranslation[GLFW_KEY_O] = KeyCode::O;
	keyTranslation[GLFW_KEY_P] = KeyCode::P;
	keyTranslation[GLFW_KEY_Q] = KeyCode::Q;
	keyTranslation[GLFW_KEY_R] = KeyCode::R;
	keyTranslation[GLFW_KEY_S] = KeyCode::S;
	keyTranslation[GLFW_KEY_T] = KeyCode::T;
	keyTranslation[GLFW_KEY_U] = KeyCode::U;
	keyTranslation[GLFW_KEY_V] = KeyCode::V;
	keyTranslation[GLFW_KEY_W] = KeyCode::W;
	keyTranslation[GLFW_KEY_X] = KeyCode::X;
	keyTranslation[GLFW_KEY_Y] = KeyCode::Y;
	keyTranslation[GLFW_KEY_Z] = KeyCode::Z;
	keyTranslation[GLFW_KEY_LEFT_BRACKET] = KeyCode::LeftBracket;
	keyTranslation[GLFW_KEY_BACKSLASH] = KeyCode::Backslash;
	keyTranslation[GLFW_KEY_RIGHT_BRACKET] = KeyCode::RightBracket;
	keyTranslation[GLFW_KEY_GRAVE_ACCENT] = KeyCode::GraveAccent;
	keyTranslation[GLFW_KEY_WORLD_1] = KeyCode::World1;
	keyTranslation[GLFW_KEY_WORLD_2] = KeyCode::World2;

	keyTranslation[GLFW_KEY_ESCAPE] = KeyCode::Esc;
	keyTranslation[GLFW_KEY_ENTER] = KeyCode::Return;
	keyTranslation[GLFW_KEY_TAB] = KeyCode::Tab;
	keyTranslation[GLFW_KEY_BACKSPACE] = KeyCode::Backspace;
	keyTranslation[GLFW_KEY_INSERT] = KeyCode::Insert;
	keyTranslation[GLFW_KEY_DELETE] = KeyCode::Delete;
	keyTranslation[GLFW_KEY_RIGHT] = KeyCode::Right;
	keyTranslation[GLFW_KEY_LEFT] = KeyCode::Left;
	keyTranslation[GLFW_KEY_DOWN] = KeyCode::Down;
	keyTranslation[GLFW_KEY_UP] = KeyCode::Up;
	keyTranslation[GLFW_KEY_PAGE_UP] = KeyCode::PageUp;
	keyTranslation[GLFW_KEY_PAGE_DOWN] = KeyCode::PageDown;
	keyTranslation[GLFW_KEY_HOME] = KeyCode::Home;
	keyTranslation[GLFW_KEY_END] = KeyCode::End;
	keyTranslation[GLFW_KEY_CAPS_LOCK] = KeyCode::CapsLock;
	keyTranslation[GLFW_KEY_SCROLL_LOCK] = KeyCode::ScrollLock;
	keyTranslation[GLFW_KEY_NUM_LOCK] = KeyCode::NumLock;
	keyTranslation[GLFW_KEY_PRINT_SCREEN] = KeyCode::Print;
	keyTranslation[GLFW_KEY_PAUSE] = KeyCode::Pause;
	keyTranslation[GLFW_KEY_F1] = KeyCode::F1;
	keyTranslation[GLFW_KEY_F2] = KeyCode::F2;
	keyTranslation[GLFW_KEY_F3] = KeyCode::F3;
	keyTranslation[GLFW_KEY_F4] = KeyCode::F4;
	keyTranslation[GLFW_KEY_F5] = KeyCode::F5;
	keyTranslation[GLFW_KEY_F6] = KeyCode::F6;
	keyTranslation[GLFW_KEY_F7] = KeyCode::F7;
	keyTranslation[GLFW_KEY_F8] = KeyCode::F8;
	keyTranslation[GLFW_KEY_F9] = KeyCode::F9;
	keyTranslation[GLFW_KEY_F10] = KeyCode::F10;
	keyTranslation[GLFW_KEY_F11] = KeyCode::F11;
	keyTranslation[GLFW_KEY_F12] = KeyCode::F12;
	keyTranslation[GLFW_KEY_KP_0] = KeyCode::NumPad0;
	keyTranslation[GLFW_KEY_KP_1] = KeyCode::NumPad1;
	keyTranslation[GLFW_KEY_KP_2] = KeyCode::NumPad2;
	keyTranslation[GLFW_KEY_KP_3] = KeyCode::NumPad3;
	keyTranslation[GLFW_KEY_KP_4] = KeyCode::NumPad4;
	keyTranslation[GLFW_KEY_KP_5] = KeyCode::NumPad5;
	keyTranslation[GLFW_KEY_KP_6] = KeyCode::NumPad6;
	keyTranslation[GLFW_KEY_KP_7] = KeyCode::NumPad7;
	keyTranslation[GLFW_KEY_KP_8] = KeyCode::NumPad8;
	keyTranslation[GLFW_KEY_KP_9] = KeyCode::NumPad9;
	keyTranslation[GLFW_KEY_KP_DECIMAL] = KeyCode::NumPadDecimal;
	keyTranslation[GLFW_KEY_KP_DIVIDE] = KeyCode::NumPadDivide;
	keyTranslation[GLFW_KEY_KP_MULTIPLY] = KeyCode::NumPadMultiply;
	keyTranslation[GLFW_KEY_KP_SUBTRACT] = KeyCode::NumPadSubtract;
	keyTranslation[GLFW_KEY_KP_ADD] = KeyCode::NumPadAdd;
	keyTranslation[GLFW_KEY_KP_ENTER] = KeyCode::NumPadEnter;
	keyTranslation[GLFW_KEY_KP_EQUAL] = KeyCode::NumPadEqual;
	keyTranslation[GLFW_KEY_LEFT_SHIFT] = KeyCode::Shift;
	keyTranslation[GLFW_KEY_LEFT_CONTROL] = KeyCode::Ctrl;
	keyTranslation[GLFW_KEY_LEFT_ALT] = KeyCode::Alt;
	keyTranslation[GLFW_KEY_LEFT_SUPER] = KeyCode::Meta;
	keyTranslation[GLFW_KEY_RIGHT_SHIFT] = KeyCode::RightShift;
	keyTranslation[GLFW_KEY_RIGHT_CONTROL] = KeyCode::RightCtrl;
	keyTranslation[GLFW_KEY_RIGHT_ALT] = KeyCode::RightAlt;
	keyTranslation[GLFW_KEY_RIGHT_SUPER] = KeyCode::RightMeta;
	keyTranslation[GLFW_KEY_MENU] = KeyCode::Menu;
}

static void ErrorCallback(int error, const char* description)
{
	Console_Error("GLFW error %d: %s", error, description);
}

static KeyModifier TranslateKeyModifiers(int mods)
{
	KeyModifier modifiers = KeyModifier::None;
	if (mods & GLFW_MOD_ALT)
		modifiers = modifiers | KeyModifier::LeftAlt;
	if (mods & GLFW_MOD_CONTROL)
		modifiers = modifiers | KeyModifier::LeftCtrl;
	if (mods & GLFW_MOD_SUPER)
		modifiers = modifiers | KeyModifier::LeftMeta;
	if (mods & GLFW_MOD_SHIFT)
		modifiers = modifiers | KeyModifier::LeftShift;
	return modifiers;
}

static MouseButton TranslateMouseButton(int button)
{
	if (button == GLFW_MOUSE_BUTTON_LEFT)
		return MouseButton::Left;
	if (button == GLFW_MOUSE_BUTTON_RIGHT)
		return MouseButton::Right;
	if (button == GLFW_MOUSE_BUTTON_MIDDLE)
		return MouseButton::Middle;
	if (button == GLFW_MOUSE_BUTTON_4)
		return MouseButton::Button4;
	if (button == GLFW_MOUSE_BUTTON_5)
		return MouseButton::Button5;
	return MouseButton::None;
}

static void KeyCallback(GLFWwindow* window, int key, int scancode, int action, int mods)
{
	if (key != GLFW_KEY_UNKNOWN)
	{
		eventQueue.postKeyEvent(keyTranslation[key], (uint8_t)TranslateKeyModifiers(mods), action != GLFW_RELEASE);
	}
}

// Based on cutef8 by Jeff Bezanson (Public Domain)
static uint8_t encodeUTF8(uint8_t _chars[4], uint32_t _scancode)
{
	uint8_t length = 0;

	if (_scancode < 0x80)
	{
		_chars[length++] = (char)_scancode;
	}
	else if (_scancode < 0x800)
	{
		_chars[length++] = (_scancode >> 6) | 0xc0;
		_chars[length++] = (_scancode & 0x3f) | 0x80;
	}
	else if (_scancode < 0x10000)
	{
		_chars[length++] = (_scancode >> 12) | 0xe0;
		_chars[length++] = ((_scancode >> 6) & 0x3f) | 0x80;
		_chars[length++] = (_scancode & 0x3f) | 0x80;
	}
	else if (_scancode < 0x110000)
	{
		_chars[length++] = (_scancode >> 18) | 0xf0;
		_chars[length++] = ((_scancode >> 12) & 0x3f) | 0x80;
		_chars[length++] = ((_scancode >> 6) & 0x3f) | 0x80;
		_chars[length++] = (_scancode & 0x3f) | 0x80;
	}

	return length;
}

static void CharCallback(GLFWwindow* window, uint32_t scancode)
{
	uint8_t value[4];
	uint8_t length = encodeUTF8(value, scancode);
	if (length)
		eventQueue.postCharEvent(length, scancode);
}

static void ScrollCallback(GLFWwindow* window, double dx, double dy)
{
	double mx, my;
	glfwGetCursorPos(window, &mx, &my);
	scrollPos += dy;
	eventQueue.postMouseEvent((int)mx, (int)my, (int)scrollPos);
}

static void CursorPosCallback(GLFWwindow* window, double mx, double my)
{
	eventQueue.postMouseEvent((int)mx, (int)my, (int)scrollPos);
}

static void MouseButtonCallback(GLFWwindow* window, int button, int action, int mods)
{
	double mx, my;
	glfwGetCursorPos(window, &mx, &my);
	eventQueue.postMouseEvent((int)mx, (int)my, (int)scrollPos, TranslateMouseButton(button), action != GLFW_RELEASE);
}

static void JoystickCallback(int jid, int event)
{
	eventQueue.postGamepadEvent(jid, event);
}

static void WindowSizeCallback(GLFWwindow* window, int width, int height)
{
	if (width == 0 || height == 0)
		return;
	eventQueue.postSizeEvent(width, height);
}

static void DropFileCallback(GLFWwindow* window, int count, const char** files)
{
	for (int i = 0; i < count; i++)
	{
		eventQueue.postDropFileEvent(files[i]);
	}
}

static void* GetNativeWindowHandle(GLFWwindow* _window)
{
#	if BX_PLATFORM_LINUX || BX_PLATFORM_BSD
# 		if ENTRY_CONFIG_USE_WAYLAND
	wl_egl_window* win_impl = (wl_egl_window*)glfwGetWindowUserPointer(_window);
	if (!win_impl)
	{
		int width, height;
		glfwGetWindowSize(_window, &width, &height);
		struct wl_surface* surface = (struct wl_surface*)glfwGetWaylandWindow(_window);
		if (!surface)
			return nullptr;
		win_impl = wl_egl_window_create(surface, width, height);
		glfwSetWindowUserPointer(_window, (void*)(uintptr_t)win_impl);
	}
	return (void*)(uintptr_t)win_impl;
#		else
	return (void*)(uintptr_t)glfwGetX11Window(_window);
#		endif
#	elif BX_PLATFORM_OSX
	return glfwGetCocoaWindow(_window);
#	elif BX_PLATFORM_WINDOWS
	return glfwGetWin32Window(_window);
#	endif // BX_PLATFORM_
}

static void SetBGFXWindow(GLFWwindow* window, bgfx::PlatformData& pd)
{
#	if BX_PLATFORM_LINUX || BX_PLATFORM_BSD
# 		if ENTRY_CONFIG_USE_WAYLAND
	pd.ndt = glfwGetWaylandDisplay();
#		else
	pd.ndt = glfwGetX11Display();
#endif
#	elif BX_PLATFORM_OSX
	pd.ndt = NULL;
#	elif BX_PLATFORM_WINDOWS
	pd.ndt = NULL;
#	endif // BX_PLATFORM_WINDOWS
	pd.nwh = GetNativeWindowHandle(window);
	pd.context = NULL;
	pd.backBuffer = NULL;
	pd.backBufferDS = NULL;
}

static void DestroyWindow(GLFWwindow* _window)
{
	if (!_window)
		return;
#	if BX_PLATFORM_LINUX || BX_PLATFORM_BSD
#		if ENTRY_CONFIG_USE_WAYLAND
	wl_egl_window* win_impl = (wl_egl_window*)glfwGetWindowUserPointer(_window);
	if (win_impl)
	{
		glfwSetWindowUserPointer(_window, nullptr);
		wl_egl_window_destroy(win_impl);
	}
#		endif
#	endif
	glfwDestroyWindow(_window);
}

RFAPI void Application_Terminate()
{
	eventQueue.postExitEvent();
}

static bool ProcessEvents(const ApplicationCallbacks& callbacks)
{
	uint32_t nextReset = BGFX_RESET_NONE |
		(vsync ? BGFX_RESET_VSYNC : 0)
		;

	bool wasResized = false;

	while (const Event* ev = eventQueue.poll())
	{
		if (ImGuiLayerProcessEvent(ev)) // if the event was already handled by ImGui, skip it
			continue;

		switch (ev->type)
		{
		case EventType::Axis:
		{
			AxisEvent* axisEvent = (AxisEvent*)ev;
			callbacks.onAxisEvent(axisEvent->axis, axisEvent->value, axisEvent->gamepad);
			break;
		}
		case EventType::Char:
		{
			CharEvent* charEvent = (CharEvent*)ev;
			callbacks.onCharEvent(charEvent->length, charEvent->value);
			break;
		}
		case EventType::Gamepad:
		{
			GamepadEvent* gamepadEvent = (GamepadEvent*)ev;
			callbacks.onGamepadEvent(gamepadEvent->gamepad, gamepadEvent->event);
			break;
		}
		case EventType::Key:
		{
			KeyEvent* keyEvent = (KeyEvent*)ev;
			callbacks.onKeyEvent(keyEvent->key, (KeyModifier)keyEvent->modifiers, keyEvent->down);
			break;
		}
		case EventType::Mouse:
		{
			MouseEvent* mouseEvent = (MouseEvent*)ev;
			if (mouseEvent->move)
				callbacks.onMouseMoveEvent(mouseEvent->x, mouseEvent->y, mouseEvent->z);
			else
				callbacks.onMouseButtonEvent(mouseEvent->button, mouseEvent->down);
			break;
		}
		case EventType::Size:
		{
			SizeEvent* sizeEvent = (SizeEvent*)ev;
			width = sizeEvent->width;
			height = sizeEvent->height;
			wasResized = true;
			break;
		}

		case EventType::Window:
			break;

		case EventType::Suspend:
			break;

		case EventType::DropFile:
		{
			DropFileEvent* dropFileEvent = (DropFileEvent*)ev;
			callbacks.onDropFileEvent(dropFileEvent->filepath.getCPtr());
			break;
		}

		case EventType::Exit:
			keepRunning = false;
			return false;

		default:
			break;
		}

		BX_FREE(Application_GetAllocator(), (void*)ev);
	}

	if (nextReset != reset || wasResized)
	{
		reset = nextReset;
		bgfx::reset(width, height, reset);
	}
	if (wasResized)
		callbacks.onViewportSizeEvent(width, height);

	return true;
}

template<typename T>
static inline T min(const T& a, const T& b)
{
	return a < b ? a : b;
}

template<typename T>
static inline T max(const T& a, const T& b)
{
	return a > b ? a : b;
}

template<typename T>
static inline T clamp(const T& a, const T& v0, const T& v1)
{
	return min(max(a, v0), v1);
}

static bool Loop(const ApplicationCallbacks& callbacks)
{
	int64_t beforeFrame = Application_GetTimestamp();
	if (!isPaused)
		currentFrame += beforeFrame - lastFrame;

	int64_t maxDelta = 1000000000 / 10;
	delta = max(min(beforeFrame - lastFrame, maxDelta), (int64_t)0) / 1e9f;
	if (fpsCap != 0)
		delta = 1.0f / fpsCap;

	bool exit = !ProcessEvents(callbacks);

	if (beforeFrame - lastSecond > 1000000000)
	{
		lastSecond = beforeFrame;

		ms = msCounter / frameCounter;
		msCounter = 0.0f;

		fps = frameCounter;
		frameCounter = 0;
	}

	callbacks.update();

	bgfx::setViewRect(0, 0, 0, bgfx::BackbufferRatio::Equal);
	bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH);
	bgfx::touch(0);

	bgfx::dbgTextClear();

	ImGuiLayerBeginFrame();

	callbacks.draw();

	ImGuiLayerEndFrame();

	frameIdx = bgfx::frame(captureFrame);
	captureFrame = false;

	int64_t afterFrame = Application_GetTimestamp();

	if (fpsCap)
	{

		int64_t frameLength = afterFrame - beforeFrame;
		int64_t remaining = 1000000000 / fpsCap - frameLength;
		if (remaining > 0)
		{
			Application_SleepFor((int)(remaining / 1000000));
		}
	}

	msCounter += (afterFrame - beforeFrame) / 1e6f;
	frameCounter++;

	lastFrame = beforeFrame;

	return !exit;
}

static const char* rendererTypeNames[] = {
		"Noop",         //!< No rendering.
		"Agc",          //!< AGC
		"Direct3D9",    //!< Direct3D 9.0
		"Direct3D11",   //!< Direct3D 11.0
		"Direct3D12",   //!< Direct3D 12.0
		"Gnm",          //!< GNM
		"Metal",        //!< Metal
		"Nvn",          //!< NVN
		"OpenGLES",     //!< OpenGL ES 2.0+
		"OpenGL",       //!< OpenGL 2.1+
		"Vulkan",       //!< Vulkan
		"WebGPU",       //!< WebGPU
};

struct BGFXCallbacks : bgfx::CallbackI
{
	~BGFXCallbacks() override
	{
	}

	/// This callback is called on unrecoverable errors.
	/// It's not safe to continue (Exluding _code `Fatal::DebugCheck`),
	/// inform the user and terminate the application.
	///
	/// @param[in] _filePath File path where fatal message was generated.
	/// @param[in] _line Line where fatal message was generated.
	/// @param[in] _code Fatal error code.
	/// @param[in] _str More information about error.
	///
	/// @remarks
	///   Not thread safe and it can be called from any thread.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.fatal`.
	///
	void fatal(
		const char* _filePath
		, uint16_t _line
		, bgfx::Fatal::Enum _code
		, const char* _str
	) override
	{
		Console_Error("BGFX Fatal Error code %d %s:%d: %s", _code, _filePath, _line, _str);
	}

	/// Print debug message.
	///
	/// @param[in] _filePath File path where debug message was generated.
	/// @param[in] _line Line where debug message was generated.
	/// @param[in] _format `printf` style format.
	/// @param[in] _argList Variable arguments list initialized with
	///   `va_start`.
	///
	/// @remarks
	///   Not thread safe and it can be called from any thread.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.trace_vargs`.
	///
	void traceVargs(
		const char* _filePath
		, uint16_t _line
		, const char* _format
		, va_list _argList
	) override
	{
		Console_LogV(_format, _argList);
	}

	/// Profiler region begin.
	///
	/// @param[in] _name Region name, contains dynamic string.
	/// @param[in] _abgr Color of profiler region.
	/// @param[in] _filePath File path where `profilerBegin` was called.
	/// @param[in] _line Line where `profilerBegin` was called.
	///
	/// @remarks
	///   Not thread safe and it can be called from any thread.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.profiler_begin`.
	///
	void profilerBegin(
		const char* _name
		, uint32_t _abgr
		, const char* _filePath
		, uint16_t _line
	) override
	{

	}

	/// Profiler region begin with string literal name.
	///
	/// @param[in] _name Region name, contains string literal.
	/// @param[in] _abgr Color of profiler region.
	/// @param[in] _filePath File path where `profilerBeginLiteral` was called.
	/// @param[in] _line Line where `profilerBeginLiteral` was called.
	///
	/// @remarks
	///   Not thread safe and it can be called from any thread.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.profiler_begin_literal`.
	///
	void profilerBeginLiteral(
		const char* _name
		, uint32_t _abgr
		, const char* _filePath
		, uint16_t _line
	) override
	{

	}

	/// Profiler region end.
	///
	/// @remarks
	///   Not thread safe and it can be called from any thread.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.profiler_end`.
	///
	void profilerEnd() override
	{

	}

	/// Returns the size of a cached item. Returns 0 if no cached item was
	/// found.
	///
	/// @param[in] _id Cache id.
	/// @returns Number of bytes to read.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.cache_read_size`.
	///
	uint32_t cacheReadSize(uint64_t _id) override
	{
		return 0;
	}

	/// Read cached item.
	///
	/// @param[in] _id Cache id.
	/// @param[in] _data Buffer where to read data.
	/// @param[in] _size Size of data to read.
	///
	/// @returns True if data is read.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.cache_read`.
	///
	bool cacheRead(uint64_t _id, void* _data, uint32_t _size) override
	{
		return false;
	}

	/// Write cached item.
	///
	/// @param[in] _id Cache id.
	/// @param[in] _data Data to write.
	/// @param[in] _size Size of data to write.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.cache_write`.
	///
	void cacheWrite(uint64_t _id, const void* _data, uint32_t _size) override
	{

	}

	/// Screenshot captured. Screenshot format is always 4-byte BGRA.
	///
	/// @param[in] _filePath File path.
	/// @param[in] _width Image width.
	/// @param[in] _height Image height.
	/// @param[in] _pitch Number of bytes to skip between the start of
	///   each horizontal line of the image.
	/// @param[in] _data Image data.
	/// @param[in] _size Image size.
	/// @param[in] _yflip If true, image origin is bottom left.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.screen_shot`.
	///
	void screenShot(
		const char* _filePath
		, uint32_t _width
		, uint32_t _height
		, uint32_t _pitch
		, const void* _data
		, uint32_t _size
		, bool _yflip
	) override
	{

	}

	/// Called when a video capture begins.
	///
	/// @param[in] _width Image width.
	/// @param[in] _height Image height.
	/// @param[in] _pitch Number of bytes to skip between the start of
	///   each horizontal line of the image.
	/// @param[in] _format Texture format. See: `TextureFormat::Enum`.
	/// @param[in] _yflip If true, image origin is bottom left.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.capture_begin`.
	///
	void captureBegin(
		uint32_t _width
		, uint32_t _height
		, uint32_t _pitch
		, bgfx::TextureFormat::Enum _format
		, bool _yflip
	) override
	{

	}

	/// Called when a video capture ends.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.capture_end`.
	///
	void captureEnd() override
	{

	}

	/// Captured frame.
	///
	/// @param[in] _data Image data.
	/// @param[in] _size Image size.
	///
	/// @attention C99 equivalent is `bgfx_callback_vtbl.capture_frame`.
	///
	void captureFrame(const void* _data, uint32_t _size) override
	{

	}
};

static int RunApp(const LaunchParams& params, const ApplicationCallbacks& callbacks)
{
	bgfx::Init init;
	init.type = bgfx::RendererType::Direct3D11;
	init.vendorId = BGFX_PCI_ID_NONE;
	init.resolution.width = width;
	init.resolution.height = height;
	init.resolution.reset = reset;
	init.callback = new BGFXCallbacks();
	init.debug = false;
	init.profile = false;
	SetBGFXWindow(window, init.platformData);

	if (!bgfx::init(init))
	{
		Console_Error("Failed to initialize BGFX");
		return 1;
	}

	bgfx::RendererType::Enum rendererTypes[8];
	int numRendererTypes = bgfx::getSupportedRenderers(8, rendererTypes);
	printf("BGFX %d, renderer: %s\n", BGFX_API_VERSION, rendererTypeNames[init.type]);

#ifdef _DEBUG
	debug = BGFX_DEBUG_TEXT | BGFX_DEBUG_PROFILER;
#else
	debug = BGFX_DEBUG_TEXT;
#endif
	bgfx::setDebug(debug);

	reset = BGFX_RESET_NONE;

	fpsCap = params.fpsCap;
	vsync = params.vsync;

	bgfx::setViewClear(0, BGFX_CLEAR_COLOR, 0xff);
	bgfx::touch(0);
	bgfx::frame();

	Console_SetErrorCallback(callbacks.onInternalErrorEvent);

#ifndef RAINFALL_EXCLUDE_PHYSICS
	fixedUpdateCallback = callbacks.fixedUpdate;
#endif

	ImGuiLayerInit();

	ResourceInit();

	callbacks.init();

	messageQueue.push(BX_NEW(Application_GetAllocator(), Message)(MessageType::WindowShow));
	//eventQueue.postSizeEvent(width, height);

	lastFrame = Application_GetTimestamp();
	lastSecond = Application_GetTimestamp();

	while (Loop(callbacks))
	{
		//
	}

	callbacks.destroy();

	ResourceTerminate();

	ImGuiLayerDestroy();

	bgfx::shutdown();

	return 0;
}

struct GameThreadData
{
	const LaunchParams& params;
	const ApplicationCallbacks& callbacks;
	//MesaApp* app;
};

static int GameThreadEntry(bx::Thread* thread, void* userData)
{
	GameThreadData* gameThreadData = (GameThreadData*)userData;
	int result = EXIT_SUCCESS;

	while (keepRunning)
	{
		result = RunApp(gameThreadData->params, gameThreadData->callbacks);
	}

	return result;
}

RFAPI int Application_Run(LaunchParams params, ApplicationCallbacks callbacks)
{
	Platform_Init();

	glfwSetErrorCallback(ErrorCallback);

	if (!glfwInit())
	{
		Console_Error("Failed to initialize GLFW");
		return EXIT_FAILURE;
	}
	printf("GLFW %s\n", glfwGetVersionString());

	glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
	glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);
	glfwWindowHint(GLFW_MAXIMIZED, params.maximized ? GLFW_TRUE : GLFW_FALSE);

	if (params.fullscreen)
	{
		GLFWmonitor* monitor = glfwGetPrimaryMonitor();
		const GLFWvidmode* videoMode = glfwGetVideoMode(monitor);
		window = glfwCreateWindow(videoMode->width, videoMode->height, params.title, monitor, nullptr);
		width = videoMode->width;
		height = videoMode->height;
		windowX = 100;
		windowY = 100;
		windowWidth = params.width;
		windowHeight = params.height;
		isFullscreen = true;
	}
	else
	{
		window = glfwCreateWindow(params.width, params.height, params.title, nullptr, nullptr);
		glfwGetWindowPos(window, &windowX, &windowY);
		glfwGetWindowSize(window, &windowWidth, &windowHeight);
		width = windowWidth;
		height = windowHeight;
		isFullscreen = false;
	}

	if (!window)
	{
		Console_Error("Failed to create window");
		glfwTerminate();
		return EXIT_FAILURE;
	}

#if BX_PLATFORM_WINDOWS
	HICON icon = ExtractIcon(GetModuleHandle(NULL), L"PixelEngine.exe", 0);
	SendMessage(glfwGetWin32Window(window), WM_SETICON, ICON_SMALL, (LPARAM)icon);
	SendMessage(glfwGetWin32Window(window), WM_SETICON, ICON_BIG, (LPARAM)icon);
#endif

	glfwSetKeyCallback(window, KeyCallback);
	glfwSetCharCallback(window, CharCallback);
	glfwSetScrollCallback(window, ScrollCallback);
	glfwSetCursorPosCallback(window, CursorPosCallback);
	glfwSetMouseButtonCallback(window, MouseButtonCallback);
	glfwSetJoystickCallback(JoystickCallback);
	glfwSetWindowSizeCallback(window, WindowSizeCallback);
	glfwSetDropCallback(window, DropFileCallback);

	InitKeyTranslation();

	eventQueue.postSizeEvent(width, height);

	// TODO init gamepads

	keepRunning = true;

	GameThreadData gameThreadData = { params, callbacks };
	gameThread.init(GameThreadEntry, &gameThreadData);

	double xpos, ypos;
	glfwGetCursorPos(window, &xpos, &ypos);
	CursorPosCallback(window, xpos, ypos);

	bool running = true;
	while (running)
	{
		glfwWaitEventsTimeout(1 / 140.0f);

		bool windowExit = glfwWindowShouldClose(window);
		if (!keepRunning || windowExit)
		{
			if (callbacks.onExitEvent(windowExit))
				running = false;
			else
				glfwSetWindowShouldClose(window, GLFW_FALSE);
		}

		// TODO update gamepads

		while (Message* msg = messageQueue.pop())
		{
			switch (msg->type)
			{
			case MessageType::WindowCreate:
			{
				// TODO
			}
			break;

			case MessageType::WindowDestroy:
			{
				// TODO
			}
			break;

			case MessageType::WindowShow:
			{
				glfwShowWindow(window);
			}
			break;

			case MessageType::WindowSetTitle:
			{
				glfwSetWindowTitle(window, msg->title);
			}
			break;

			case MessageType::WindowSetPos:
			{
				glfwSetWindowPos(window, msg->x, msg->y);
			}
			break;

			case MessageType::WindowSetSize:
			{
				glfwSetWindowSize(window, msg->width, msg->height);
			}
			break;

			case MessageType::WindowToggleFrame:
			{
				// Wait for glfwSetWindowDecorated to exist
			}
			break;

			case MessageType::WindowSetMaximized:
			{
				glfwMaximizeWindow(window);
			}
			break;

			case MessageType::WindowToggleFullscreen:
			{
				if (glfwGetWindowMonitor(window))
				{
					glfwSetWindowMonitor(window, nullptr, windowX, windowY, windowWidth, windowHeight, 0);
					isFullscreen = false;
				}
				else
				{
					GLFWmonitor* monitor = glfwGetPrimaryMonitor();
					if (monitor)
					{
						glfwGetWindowPos(window, &windowX, &windowY);
						glfwGetWindowSize(window, &windowWidth, &windowHeight);

						const GLFWvidmode* mode = glfwGetVideoMode(monitor);
						glfwSetWindowMonitor(window, monitor, 0, 0, mode->width, mode->height, mode->refreshRate);

						isFullscreen = true;

						glfwFocusWindow(window);
					}
				}
			}
			break;

			case MessageType::WindowSetVSync:
			{
				vsync = msg->value;
				// glfwSwapInterval(msg->value); // Only works with current OpenGL context
			}
			break;

			case MessageType::WindowSetFpsCap:
			{
				fpsCap = msg->value;
			}
			break;

			case MessageType::MouseSetPos:
			{
				glfwSetCursorPos(window, (double)msg->x, (double)msg->y);
			}
			break;

			case MessageType::MouseLock:
			{
				glfwSetInputMode(window, GLFW_CURSOR, msg->value == (int)CursorMode::Normal ? GLFW_CURSOR_NORMAL : msg->value == (int)CursorMode::Hidden ? GLFW_CURSOR_HIDDEN : GLFW_CURSOR_DISABLED);
				cursorMode = (CursorMode)msg->value;
			}
			break;
			}

			BX_FREE(Application_GetAllocator(), msg);
		}
	}

	glfwHideWindow(window);

	eventQueue.postExitEvent();
	gameThread.shutdown();

	DestroyWindow(window);
	glfwTerminate();

	Platform_Terminate();

	return gameThread.getExitCode();
}

RFAPI void Application_SetTimerPaused(bool paused)
{
	isPaused = paused;
}

RFAPI int64_t Application_GetCurrentTime()
{
	return currentFrame;
}

RFAPI float Application_GetFrameTime()
{
	return delta;
}

RFAPI int Application_GetFPS()
{
	return fps;
}

RFAPI float Application_GetMS()
{
	return ms;
}

RFAPI int64_t Application_GetMemoryUsage()
{
	MemoryAllocator* allocator = (MemoryAllocator*)Application_GetAllocator();
	return allocator ? allocator->getNumBytes() : 0;
}

RFAPI int Application_GetNumAllocations()
{
	MemoryAllocator* allocator = (MemoryAllocator*)Application_GetAllocator();
	return allocator ? allocator->getNumAllocations() : 0;
}

RFAPI void Application_SetDebugTextEnabled(bool enabled)
{
	if (enabled)
		debug |= BGFX_DEBUG_TEXT;
	else
	{
		debug |= BGFX_DEBUG_TEXT;
		debug ^= BGFX_DEBUG_TEXT;
	}
	bgfx::setDebug(debug);
}

RFAPI bool Application_IsDebugTextEnabled()
{
	return debug & BGFX_DEBUG_TEXT;
}

RFAPI void Application_SetDebugStatsEnabled(bool enabled)
{
	if (enabled)
		debug |= BGFX_DEBUG_STATS;
	else
	{
		debug |= BGFX_DEBUG_STATS;
		debug ^= BGFX_DEBUG_STATS;
	}
	bgfx::setDebug(debug);
}

RFAPI bool Application_IsDebugStatsEnabled()
{
	return debug & BGFX_DEBUG_STATS;
}

RFAPI void Application_SetDebugWireframeEnabled(bool enabled)
{
	if (enabled)
		debug |= BGFX_DEBUG_WIREFRAME;
	else
	{
		debug |= BGFX_DEBUG_WIREFRAME;
		debug ^= BGFX_DEBUG_WIREFRAME;
	}
	bgfx::setDebug(debug);
}

RFAPI bool Application_IsDebugWireframeEnabled()
{
	return debug & BGFX_DEBUG_WIREFRAME;
}

RFAPI bool Application_GetGamepadState(int gamepad, GLFWgamepadstate* state)
{
	return glfwGetGamepadState(gamepad, state);
}

RFAPI void Application_SetCursorMode(CursorMode mode)
{
	Message* msg = BX_NEW(Application_GetAllocator(), Message)(MessageType::MouseLock);
	msg->value = (int)mode;
	messageQueue.push(msg);
}

RFAPI void Application_SetMousePosition(int x, int y)
{
	Message* msg = BX_NEW(Application_GetAllocator(), Message)(MessageType::MouseSetPos);
	msg->x = x;
	msg->y = y;
	messageQueue.push(msg);
}

RFAPI void Application_SetWindowVisible(bool visible)
{
	if (visible)
		glfwShowWindow(window);
	else
		glfwHideWindow(window);
}

RFAPI void Application_SetWindowPosition(int x, int y)
{
	Message* msg = BX_NEW(Application_GetAllocator(), Message)(MessageType::WindowSetPos);
	msg->x = x;
	msg->y = y;
	messageQueue.push(msg);
}

RFAPI void Application_GetWindowPosition(int* outX, int* outY)
{
	glfwGetWindowPos(window, outX, outY);
}

RFAPI void Application_SetWindowSize(int width, int height)
{
	Message* msg = BX_NEW(Application_GetAllocator(), Message)(MessageType::WindowSetSize);
	msg->width = width;
	msg->height = height;
	messageQueue.push(msg);
}

RFAPI void Application_SetWindowTitle(const char* title)
{
	Message* msg = BX_NEW(Application_GetAllocator(), Message)(MessageType::WindowSetTitle);
	strcpy(msg->title, title);
	messageQueue.push(msg);
}

RFAPI void Application_SetWindowMaximized(bool maximized)
{
	Message* msg = BX_NEW(Application_GetAllocator(), Message)(MessageType::WindowSetMaximized);
	msg->value = maximized;
	messageQueue.push(msg);
}

RFAPI void Application_ToggleFullscreen()
{
	Message* msg = BX_NEW(Application_GetAllocator(), Message)(MessageType::WindowToggleFullscreen);
	messageQueue.push(msg);
}

RFAPI void Application_SetVSync(int vsync)
{
	Message* msg = BX_NEW(Application_GetAllocator(), Message)(MessageType::WindowSetVSync);
	msg->value = vsync;
	messageQueue.push(msg);
}

RFAPI void Application_SetFpsCap(int fpsCap)
{
	Message* msg = BX_NEW(Application_GetAllocator(), Message)(MessageType::WindowSetFpsCap);
	msg->value = fpsCap;
	messageQueue.push(msg);
}

RFAPI CursorMode Application_GetCursorMode()
{
	return cursorMode;
}

RFAPI void Application_GetMonitorSize(int* outWidth, int* outHeight)
{
	if (GLFWmonitor* monitor = glfwGetPrimaryMonitor())
	{
		const GLFWvidmode* videoMode = glfwGetVideoMode(monitor);
		*outWidth = videoMode->width;
		*outHeight = videoMode->height;
	}
}

RFAPI void Application_CaptureFrame()
{
	captureFrame = true;
}
