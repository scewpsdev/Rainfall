#include "ImGuiLayer.h"

#include "Application.h"
#include "Input.h"

#include "bgfx/common/imgui/vs_ocornut_imgui.bin.h"
#include "bgfx/common/imgui/fs_ocornut_imgui.bin.h"
#include "bgfx/common/imgui/vs_imgui_image.bin.h"
#include "bgfx/common/imgui/fs_imgui_image.bin.h"

#include "bgfx/common/imgui/roboto_regular.ttf.h"
#include "bgfx/common/imgui/robotomono_regular.ttf.h"
#include "bgfx/common/imgui/icons_kenney.ttf.h"
#include "bgfx/common/imgui/icons_font_awesome.ttf.h"

#include "bgfx/common/iconfontheaders/icons_font_awesome.h"
#include "bgfx/common/iconfontheaders/icons_kenney.h"

//#include "bgfx/common/bgfx_utils.h"

#include "imgui/imgui.h"
#include "imgui/imgui_internal.h"
#include "imguizmo/ImGuizmo.h"

#include <bgfx/embedded_shader.h>
#include <bx/math.h>

#include <GLFW/glfw3.h>


#define STB_RECT_PACK_IMPLEMENTATION
//#include <stb/stb_rect_pack.h>
#define STB_TRUETYPE_IMPLEMENTATION
//#include <stb/stb_truetype.h>


#define FONT_SIZE 18.0f

#define IMGUI_FLAGS_NONE        UINT8_C(0x00)
#define IMGUI_FLAGS_ALPHA_BLEND UINT8_C(0x01)


static const bgfx::EmbeddedShader s_embeddedShaders[] =
{
	BGFX_EMBEDDED_SHADER(vs_ocornut_imgui),
	BGFX_EMBEDDED_SHADER(fs_ocornut_imgui),
	BGFX_EMBEDDED_SHADER(vs_imgui_image),
	BGFX_EMBEDDED_SHADER(fs_imgui_image),

	BGFX_EMBEDDED_SHADER_END()
};

struct FontRangeMerge
{
	const void* data;
	size_t      size;
	ImWchar     ranges[3];
};

static FontRangeMerge s_fontRangeMerge[] =
{
	{ s_iconsKenneyTtf,      sizeof(s_iconsKenneyTtf),      { ICON_MIN_KI, ICON_MAX_KI, 0 } },
	{ s_iconsFontAwesomeTtf, sizeof(s_iconsFontAwesomeTtf), { ICON_MIN_FA, ICON_MAX_FA, 0 } },
};

static ImGuiContext* m_imgui;
static bgfx::VertexLayout  m_layout;
static bgfx::ProgramHandle m_program;
static bgfx::ProgramHandle m_imageProgram;
static bgfx::TextureHandle m_texture;
static bgfx::UniformHandle s_tex;
static bgfx::UniformHandle u_imageLodEnabled;
static ImFont* m_font;
static int64_t m_last;
static int32_t m_lastScroll;
static bgfx::ViewId m_viewId;


static void setupStyle(bool _dark)
{
	// Doug Binks' darl color scheme
	// https://gist.github.com/dougbinks/8089b4bbaccaaf6fa204236978d165a9
	ImGuiStyle& style = ImGui::GetStyle();
	if (_dark)
	{
		ImGui::StyleColorsDark(&style);
	}
	else
	{
		ImGui::StyleColorsLight(&style);
	}

	style.FrameRounding = 4.0f;
	style.WindowBorderSize = 0.0f;
}

static void* ImGuiAlloc(size_t sz, void* user_data)
{
	return BX_ALLOC(Application_GetAllocator(), sz);
}

static void ImGuiFree(void* ptr, void* user_data)
{
	return BX_FREE(Application_GetAllocator(), ptr);
}

void ImGuiLayerInit()
{
	m_viewId = 255;
	m_lastScroll = 0;
	m_last = Application_GetTimestamp();

	m_imgui = ImGui::CreateContext();
	ImGui::SetAllocatorFunctions(ImGuiAlloc, ImGuiFree);

	ImGuiIO& io = ImGui::GetIO();

	io.DisplaySize = ImVec2(1280.0f, 720.0f);
	io.DeltaTime = 1.0f / 60.0f;
	io.IniFilename = NULL;

	setupStyle(true);

	io.KeyMap[ImGuiKey_Tab] = (int)KeyCode::Tab;
	io.KeyMap[ImGuiKey_LeftArrow] = (int)KeyCode::Left;
	io.KeyMap[ImGuiKey_RightArrow] = (int)KeyCode::Right;
	io.KeyMap[ImGuiKey_UpArrow] = (int)KeyCode::Up;
	io.KeyMap[ImGuiKey_DownArrow] = (int)KeyCode::Down;
	io.KeyMap[ImGuiKey_PageUp] = (int)KeyCode::PageUp;
	io.KeyMap[ImGuiKey_PageDown] = (int)KeyCode::PageDown;
	io.KeyMap[ImGuiKey_Home] = (int)KeyCode::Home;
	io.KeyMap[ImGuiKey_End] = (int)KeyCode::End;
	io.KeyMap[ImGuiKey_Insert] = (int)KeyCode::Insert;
	io.KeyMap[ImGuiKey_Delete] = (int)KeyCode::Delete;
	io.KeyMap[ImGuiKey_Backspace] = (int)KeyCode::Backspace;
	io.KeyMap[ImGuiKey_Space] = (int)KeyCode::Space;
	io.KeyMap[ImGuiKey_Enter] = (int)KeyCode::Return;
	io.KeyMap[ImGuiKey_Escape] = (int)KeyCode::Esc;
	io.KeyMap[ImGuiKey_LeftCtrl] = (int)KeyCode::LeftCtrl;
	io.KeyMap[ImGuiKey_LeftShift] = (int)KeyCode::LeftShift;
	io.KeyMap[ImGuiKey_LeftAlt] = (int)KeyCode::LeftAlt;
	io.KeyMap[ImGuiKey_LeftSuper] = (int)KeyCode::LeftMeta;
	io.KeyMap[ImGuiKey_RightCtrl] = (int)KeyCode::RightCtrl;
	io.KeyMap[ImGuiKey_RightShift] = (int)KeyCode::RightShift;
	io.KeyMap[ImGuiKey_RightAlt] = (int)KeyCode::RightAlt;
	io.KeyMap[ImGuiKey_RightSuper] = (int)KeyCode::RightMeta;
	io.KeyMap[ImGuiKey_Menu] = (int)KeyCode::Menu;
	io.KeyMap[ImGuiKey_0] = (int)KeyCode::Key0;
	io.KeyMap[ImGuiKey_1] = (int)KeyCode::Key1;
	io.KeyMap[ImGuiKey_2] = (int)KeyCode::Key2;
	io.KeyMap[ImGuiKey_3] = (int)KeyCode::Key3;
	io.KeyMap[ImGuiKey_4] = (int)KeyCode::Key4;
	io.KeyMap[ImGuiKey_5] = (int)KeyCode::Key5;
	io.KeyMap[ImGuiKey_6] = (int)KeyCode::Key6;
	io.KeyMap[ImGuiKey_7] = (int)KeyCode::Key7;
	io.KeyMap[ImGuiKey_8] = (int)KeyCode::Key8;
	io.KeyMap[ImGuiKey_9] = (int)KeyCode::Key9;
	io.KeyMap[ImGuiKey_A] = (int)KeyCode::KeyA;
	io.KeyMap[ImGuiKey_B] = (int)KeyCode::KeyB;
	io.KeyMap[ImGuiKey_C] = (int)KeyCode::KeyC;
	io.KeyMap[ImGuiKey_D] = (int)KeyCode::KeyD;
	io.KeyMap[ImGuiKey_E] = (int)KeyCode::KeyE;
	io.KeyMap[ImGuiKey_F] = (int)KeyCode::KeyF;
	io.KeyMap[ImGuiKey_G] = (int)KeyCode::KeyG;
	io.KeyMap[ImGuiKey_H] = (int)KeyCode::KeyH;
	io.KeyMap[ImGuiKey_I] = (int)KeyCode::KeyI;
	io.KeyMap[ImGuiKey_J] = (int)KeyCode::KeyJ;
	io.KeyMap[ImGuiKey_K] = (int)KeyCode::KeyK;
	io.KeyMap[ImGuiKey_L] = (int)KeyCode::KeyL;
	io.KeyMap[ImGuiKey_M] = (int)KeyCode::KeyM;
	io.KeyMap[ImGuiKey_N] = (int)KeyCode::KeyN;
	io.KeyMap[ImGuiKey_O] = (int)KeyCode::KeyO;
	io.KeyMap[ImGuiKey_P] = (int)KeyCode::KeyP;
	io.KeyMap[ImGuiKey_Q] = (int)KeyCode::KeyQ;
	io.KeyMap[ImGuiKey_R] = (int)KeyCode::KeyR;
	io.KeyMap[ImGuiKey_S] = (int)KeyCode::KeyS;
	io.KeyMap[ImGuiKey_T] = (int)KeyCode::KeyT;
	io.KeyMap[ImGuiKey_U] = (int)KeyCode::KeyU;
	io.KeyMap[ImGuiKey_V] = (int)KeyCode::KeyV;
	io.KeyMap[ImGuiKey_W] = (int)KeyCode::KeyW;
	io.KeyMap[ImGuiKey_X] = (int)KeyCode::KeyX;
	io.KeyMap[ImGuiKey_Y] = (int)KeyCode::KeyY;
	io.KeyMap[ImGuiKey_Z] = (int)KeyCode::KeyZ;
	io.KeyMap[ImGuiKey_F1] = (int)KeyCode::F1;
	io.KeyMap[ImGuiKey_F2] = (int)KeyCode::F2;
	io.KeyMap[ImGuiKey_F3] = (int)KeyCode::F3;
	io.KeyMap[ImGuiKey_F4] = (int)KeyCode::F4;
	io.KeyMap[ImGuiKey_F5] = (int)KeyCode::F5;
	io.KeyMap[ImGuiKey_F6] = (int)KeyCode::F6;
	io.KeyMap[ImGuiKey_F7] = (int)KeyCode::F7;
	io.KeyMap[ImGuiKey_F8] = (int)KeyCode::F8;
	io.KeyMap[ImGuiKey_F9] = (int)KeyCode::F9;
	io.KeyMap[ImGuiKey_F10] = (int)KeyCode::F10;
	io.KeyMap[ImGuiKey_F11] = (int)KeyCode::F11;
	io.KeyMap[ImGuiKey_F12] = (int)KeyCode::F12;
	io.KeyMap[ImGuiKey_Apostrophe] = (int)KeyCode::Apostrophe;        // '
	io.KeyMap[ImGuiKey_Comma] = (int)KeyCode::Comma;             // ,
	io.KeyMap[ImGuiKey_Minus] = (int)KeyCode::Minus;             // -
	io.KeyMap[ImGuiKey_Period] = (int)KeyCode::Period;            // .
	io.KeyMap[ImGuiKey_Slash] = (int)KeyCode::Slash;             // /
	io.KeyMap[ImGuiKey_Semicolon] = (int)KeyCode::Semicolon;         // ;
	io.KeyMap[ImGuiKey_Equal] = (int)KeyCode::Equal;             // =
	io.KeyMap[ImGuiKey_LeftBracket] = (int)KeyCode::LeftBracket;       // [
	io.KeyMap[ImGuiKey_Backslash] = (int)KeyCode::Backslash;         // \ (this text inhibit multiline comment caused by backslash)
	io.KeyMap[ImGuiKey_RightBracket] = (int)KeyCode::RightBracket;      // ]
	io.KeyMap[ImGuiKey_GraveAccent] = (int)KeyCode::GraveAccent;       // `
	io.KeyMap[ImGuiKey_CapsLock] = (int)KeyCode::CapsLock;
	io.KeyMap[ImGuiKey_ScrollLock] = (int)KeyCode::ScrollLock;
	io.KeyMap[ImGuiKey_NumLock] = (int)KeyCode::NumLock;
	io.KeyMap[ImGuiKey_PrintScreen] = (int)KeyCode::Print;
	io.KeyMap[ImGuiKey_Pause] = (int)KeyCode::Pause;
	io.KeyMap[ImGuiKey_Keypad0] = (int)KeyCode::NumPad0;
	io.KeyMap[ImGuiKey_Keypad1] = (int)KeyCode::NumPad1;
	io.KeyMap[ImGuiKey_Keypad2] = (int)KeyCode::NumPad2;
	io.KeyMap[ImGuiKey_Keypad3] = (int)KeyCode::NumPad3;
	io.KeyMap[ImGuiKey_Keypad4] = (int)KeyCode::NumPad4;
	io.KeyMap[ImGuiKey_Keypad5] = (int)KeyCode::NumPad5;
	io.KeyMap[ImGuiKey_Keypad6] = (int)KeyCode::NumPad6;
	io.KeyMap[ImGuiKey_Keypad7] = (int)KeyCode::NumPad7;
	io.KeyMap[ImGuiKey_Keypad8] = (int)KeyCode::NumPad8;
	io.KeyMap[ImGuiKey_Keypad9] = (int)KeyCode::NumPad9;
	io.KeyMap[ImGuiKey_KeypadDecimal] = (int)KeyCode::NumPadDecimal;
	io.KeyMap[ImGuiKey_KeypadDivide] = (int)KeyCode::NumPadDivide;
	io.KeyMap[ImGuiKey_KeypadMultiply] = (int)KeyCode::NumPadMultiply;
	io.KeyMap[ImGuiKey_KeypadSubtract] = (int)KeyCode::NumPadSubtract;
	io.KeyMap[ImGuiKey_KeypadAdd] = (int)KeyCode::NumPadAdd;
	io.KeyMap[ImGuiKey_KeypadEnter] = (int)KeyCode::NumPadEnter;
	io.KeyMap[ImGuiKey_KeypadEqual] = (int)KeyCode::NumPadEqual;

	io.ConfigFlags |= 0
		| ImGuiConfigFlags_NavEnableGamepad
		| ImGuiConfigFlags_NavEnableKeyboard
		;

	io.NavInputs[ImGuiNavInput_Activate] = (int)KeyCode::GamepadA;
	io.NavInputs[ImGuiNavInput_Cancel] = (int)KeyCode::GamepadB;
	//		io.NavInputs[ImGuiNavInput_Input]       = (int)entry::Key::;
	//		io.NavInputs[ImGuiNavInput_Menu]        = (int)entry::Key::;
	io.NavInputs[ImGuiNavInput_DpadLeft] = (int)KeyCode::GamepadLeft;
	io.NavInputs[ImGuiNavInput_DpadRight] = (int)KeyCode::GamepadRight;
	io.NavInputs[ImGuiNavInput_DpadUp] = (int)KeyCode::GamepadUp;
	io.NavInputs[ImGuiNavInput_DpadDown] = (int)KeyCode::GamepadDown;
	io.NavInputs[ImGuiNavInput_FocusNext];
	//		io.NavInputs[ImGuiNavInput_LStickLeft]  = (int)entry::Key::;
	//		io.NavInputs[ImGuiNavInput_LStickRight] = (int)entry::Key::;
	//		io.NavInputs[ImGuiNavInput_LStickUp]    = (int)entry::Key::;
	//		io.NavInputs[ImGuiNavInput_LStickDown]  = (int)entry::Key::;
	//		io.NavInputs[ImGuiNavInput_FocusPrev]   = (int)entry::Key::;
	//		io.NavInputs[ImGuiNavInput_FocusNext]   = (int)entry::Key::;
	//		io.NavInputs[ImGuiNavInput_TweakSlow]   = (int)entry::Key::;
	//		io.NavInputs[ImGuiNavInput_TweakFast]   = (int)entry::Key::;

	m_imgui->ConfigNavWindowingKeyNext = ImGuiMod_Ctrl | ImGuiMod_Alt | ImGuiKey_Tab;
	m_imgui->ConfigNavWindowingKeyPrev = ImGuiMod_Ctrl | ImGuiMod_Shift | ImGuiMod_Alt | ImGuiKey_Tab;

	bgfx::RendererType::Enum type = bgfx::getRendererType();
	m_program = bgfx::createProgram(
		bgfx::createEmbeddedShader(s_embeddedShaders, type, "vs_ocornut_imgui")
		, bgfx::createEmbeddedShader(s_embeddedShaders, type, "fs_ocornut_imgui")
		, true
	);

	u_imageLodEnabled = bgfx::createUniform("u_imageLodEnabled", bgfx::UniformType::Vec4);
	m_imageProgram = bgfx::createProgram(
		bgfx::createEmbeddedShader(s_embeddedShaders, type, "vs_imgui_image")
		, bgfx::createEmbeddedShader(s_embeddedShaders, type, "fs_imgui_image")
		, true
	);

	m_layout
		.begin()
		.add(bgfx::Attrib::Position, 2, bgfx::AttribType::Float)
		.add(bgfx::Attrib::TexCoord0, 2, bgfx::AttribType::Float)
		.add(bgfx::Attrib::Color0, 4, bgfx::AttribType::Uint8, true)
		.end();

	s_tex = bgfx::createUniform("s_tex", bgfx::UniformType::Sampler);

	uint8_t* data;
	int32_t width;
	int32_t height;
	{
		ImFontConfig config;
		config.FontDataOwnedByAtlas = false;
		config.MergeMode = false;
		//			config.MergeGlyphCenterV = true;

		const ImWchar* ranges = io.Fonts->GetGlyphRangesCyrillic();
		m_font = io.Fonts->AddFontFromMemoryTTF((void*)s_robotoRegularTtf, sizeof(s_robotoRegularTtf), FONT_SIZE, &config, ranges);
		//m_font[ImGui::Font::Regular] = io.Fonts->AddFontFromMemoryTTF((void*)s_robotoRegularTtf, sizeof(s_robotoRegularTtf), FONT_SIZE, &config, ranges);
		//m_font[ImGui::Font::Mono] = io.Fonts->AddFontFromMemoryTTF((void*)s_robotoMonoRegularTtf, sizeof(s_robotoMonoRegularTtf), FONT_SIZE - 3.0f, &config, ranges);

		config.MergeMode = true;
		config.DstFont = m_font; // [ImGui::Font::Regular] ;

		for (uint32_t ii = 0; ii < BX_COUNTOF(s_fontRangeMerge); ++ii)
		{
			const FontRangeMerge& frm = s_fontRangeMerge[ii];

			io.Fonts->AddFontFromMemoryTTF((void*)frm.data
				, (int)frm.size
				, FONT_SIZE - 3.0f
				, &config
				, frm.ranges
			);
		}
	}

	io.Fonts->GetTexDataAsRGBA32(&data, &width, &height);

	m_texture = bgfx::createTexture2D(
		(uint16_t)width
		, (uint16_t)height
		, false
		, 1
		, bgfx::TextureFormat::BGRA8
		, 0
		, bgfx::copy(data, width * height * 4)
	);

	//ImGui::InitDockContext();
}

void ImGuiLayerDestroy()
{
	//ImGui::ShutdownDockContext();
	ImGui::DestroyContext(m_imgui);

	bgfx::destroy(s_tex);
	bgfx::destroy(m_texture);

	bgfx::destroy(u_imageLodEnabled);
	bgfx::destroy(m_imageProgram);
	bgfx::destroy(m_program);
}

void ImGuiLayerBeginFrame()
{
	ImGuiIO& io = ImGui::GetIO();

	int64_t now = Application_GetTimestamp();
	int64_t frameTime = now - m_last;
	m_last = now;
	io.DeltaTime = frameTime / 1e9f;

	ImGui::NewFrame();
	ImGuizmo::BeginFrame();
}

static bool checkAvailTransientBuffers(uint32_t _numVertices, const bgfx::VertexLayout& _layout, uint32_t _numIndices)
{
	return _numVertices == bgfx::getAvailTransientVertexBuffer(_numVertices, _layout)
		&& (0 == _numIndices || _numIndices == bgfx::getAvailTransientIndexBuffer(_numIndices))
		;
}

static void ImGuiLayerDraw(ImDrawData* _drawData)
{
	// Avoid rendering when minimized, scale coordinates for retina displays (screen coordinates != framebuffer coordinates)
	int fb_width = (int)(_drawData->DisplaySize.x * _drawData->FramebufferScale.x);
	int fb_height = (int)(_drawData->DisplaySize.y * _drawData->FramebufferScale.y);
	if (fb_width <= 0 || fb_height <= 0)
		return;

	bgfx::setViewName(m_viewId, "ImGui");
	bgfx::setViewMode(m_viewId, bgfx::ViewMode::Sequential);

	const bgfx::Caps* caps = bgfx::getCaps();
	{
		float ortho[16];
		float x = _drawData->DisplayPos.x;
		float y = _drawData->DisplayPos.y;
		float width = _drawData->DisplaySize.x;
		float height = _drawData->DisplaySize.y;

		bx::mtxOrtho(ortho, x, x + width, y + height, y, 0.0f, 1000.0f, 0.0f, caps->homogeneousDepth);
		bgfx::setViewTransform(m_viewId, NULL, ortho);
		bgfx::setViewRect(m_viewId, 0, 0, uint16_t(width), uint16_t(height));
	}

	const ImVec2 clipPos = _drawData->DisplayPos;       // (0,0) unless using multi-viewports
	const ImVec2 clipScale = _drawData->FramebufferScale; // (1,1) unless using retina display which are often (2,2)

	// Render command lists
	for (int32_t ii = 0, num = _drawData->CmdListsCount; ii < num; ++ii)
	{
		bgfx::TransientVertexBuffer tvb;
		bgfx::TransientIndexBuffer tib;

		const ImDrawList* drawList = _drawData->CmdLists[ii];
		uint32_t numVertices = (uint32_t)drawList->VtxBuffer.size();
		uint32_t numIndices = (uint32_t)drawList->IdxBuffer.size();

		if (!checkAvailTransientBuffers(numVertices, m_layout, numIndices))
		{
			// not enough space in transient buffer just quit drawing the rest...
			break;
		}

		bgfx::allocTransientVertexBuffer(&tvb, numVertices, m_layout);
		bgfx::allocTransientIndexBuffer(&tib, numIndices, sizeof(ImDrawIdx) == 4);

		ImDrawVert* verts = (ImDrawVert*)tvb.data;
		bx::memCopy(verts, drawList->VtxBuffer.begin(), numVertices * sizeof(ImDrawVert));

		ImDrawIdx* indices = (ImDrawIdx*)tib.data;
		bx::memCopy(indices, drawList->IdxBuffer.begin(), numIndices * sizeof(ImDrawIdx));

		bgfx::Encoder* encoder = bgfx::begin();

		uint32_t offset = 0;
		for (const ImDrawCmd* cmd = drawList->CmdBuffer.begin(), *cmdEnd = drawList->CmdBuffer.end(); cmd != cmdEnd; ++cmd)
		{
			if (cmd->UserCallback)
			{
				cmd->UserCallback(drawList, cmd);
			}
			else if (0 != cmd->ElemCount)
			{
				uint64_t state = 0
					| BGFX_STATE_WRITE_RGB
					| BGFX_STATE_WRITE_A
					| BGFX_STATE_MSAA
					;

				bgfx::TextureHandle th = m_texture;
				bgfx::ProgramHandle program = m_program;

				if (NULL != cmd->TextureId)
				{
					union { ImTextureID ptr; struct { bgfx::TextureHandle handle; uint8_t flags; uint8_t mip; } s; } texture = { cmd->TextureId };
					state |= 0 != (IMGUI_FLAGS_ALPHA_BLEND & texture.s.flags)
						? BGFX_STATE_BLEND_FUNC(BGFX_STATE_BLEND_SRC_ALPHA, BGFX_STATE_BLEND_INV_SRC_ALPHA)
						: BGFX_STATE_NONE
						;
					th = texture.s.handle;
					if (0 != texture.s.mip)
					{
						const float lodEnabled[4] = { float(texture.s.mip), 1.0f, 0.0f, 0.0f };
						bgfx::setUniform(u_imageLodEnabled, lodEnabled);
						program = m_imageProgram;
					}
				}
				else
				{
					state |= BGFX_STATE_BLEND_FUNC(BGFX_STATE_BLEND_SRC_ALPHA, BGFX_STATE_BLEND_INV_SRC_ALPHA);
				}

				// Project scissor/clipping rectangles into framebuffer space
				ImVec4 clipRect;
				clipRect.x = (cmd->ClipRect.x - clipPos.x) * clipScale.x;
				clipRect.y = (cmd->ClipRect.y - clipPos.y) * clipScale.y;
				clipRect.z = (cmd->ClipRect.z - clipPos.x) * clipScale.x;
				clipRect.w = (cmd->ClipRect.w - clipPos.y) * clipScale.y;

				if (clipRect.x < fb_width
					&& clipRect.y < fb_height
					&& clipRect.z >= 0.0f
					&& clipRect.w >= 0.0f)
				{
					const uint16_t xx = uint16_t(bx::max(clipRect.x, 0.0f));
					const uint16_t yy = uint16_t(bx::max(clipRect.y, 0.0f));
					encoder->setScissor(xx, yy
						, uint16_t(bx::min(clipRect.z, 65535.0f) - xx)
						, uint16_t(bx::min(clipRect.w, 65535.0f) - yy)
					);

					encoder->setState(state);
					encoder->setTexture(0, s_tex, th);
					encoder->setVertexBuffer(0, &tvb, 0, numVertices);
					encoder->setIndexBuffer(&tib, offset, cmd->ElemCount);
					encoder->submit(m_viewId, program);
				}
			}

			offset += cmd->ElemCount;
		}

		bgfx::end(encoder);
	}
}

void ImGuiLayerEndFrame()
{
	ImGui::Render();
	ImGuiLayerDraw(ImGui::GetDrawData());
}

RFAPI int ImGui_TranslateKey(KeyCode key)
{
	static int keys[(int)KeyCode::Count] = {
		ImGuiKey_None,
		ImGuiKey_Escape,
		ImGuiKey_Enter,
		ImGuiKey_Tab,
		ImGuiKey_Space,
		ImGuiKey_Backspace,
		ImGuiKey_UpArrow,
		ImGuiKey_DownArrow,
		ImGuiKey_LeftArrow,
		ImGuiKey_RightArrow,
		ImGuiKey_Insert,
		ImGuiKey_Delete,
		ImGuiKey_Home,
		ImGuiKey_End,
		ImGuiKey_PageUp,
		ImGuiKey_PageDown,
		ImGuiKey_PrintScreen,
		ImGuiKey_Pause,
		ImGuiKey_KeypadAdd,
		ImGuiKey_Minus,
		ImGuiKey_Equal,
		ImGuiKey_CapsLock,
		ImGuiKey_ScrollLock,
		ImGuiKey_NumLock,
		ImGuiKey_KeypadMultiply,
		ImGuiKey_LeftBracket,
		ImGuiKey_RightBracket,
		ImGuiKey_Semicolon,
		ImGuiKey_None, // Quotation marks
		ImGuiKey_Comma,
		ImGuiKey_Period,
		ImGuiKey_Slash,
		ImGuiKey_Backslash,
		ImGuiKey_None, // Tilde
		ImGuiKey_Apostrophe,
		ImGuiKey_GraveAccent,
		ImGuiKey_F1,
		ImGuiKey_F2,
		ImGuiKey_F3,
		ImGuiKey_F4,
		ImGuiKey_F5,
		ImGuiKey_F6,
		ImGuiKey_F7,
		ImGuiKey_F8,
		ImGuiKey_F9,
		ImGuiKey_F10,
		ImGuiKey_F11,
		ImGuiKey_F12,
		ImGuiKey_0,
		ImGuiKey_1,
		ImGuiKey_2,
		ImGuiKey_3,
		ImGuiKey_4,
		ImGuiKey_5,
		ImGuiKey_6,
		ImGuiKey_7,
		ImGuiKey_8,
		ImGuiKey_9,
		ImGuiKey_A,
		ImGuiKey_B,
		ImGuiKey_C,
		ImGuiKey_D,
		ImGuiKey_E,
		ImGuiKey_F,
		ImGuiKey_G,
		ImGuiKey_H,
		ImGuiKey_I,
		ImGuiKey_J,
		ImGuiKey_K,
		ImGuiKey_L,
		ImGuiKey_M,
		ImGuiKey_N,
		ImGuiKey_O,
		ImGuiKey_P,
		ImGuiKey_Q,
		ImGuiKey_R,
		ImGuiKey_S,
		ImGuiKey_T,
		ImGuiKey_U,
		ImGuiKey_V,
		ImGuiKey_W,
		ImGuiKey_X,
		ImGuiKey_Y,
		ImGuiKey_Z,

		ImGuiKey_None, // World1
		ImGuiKey_None, // World2

		ImGuiKey_Keypad0,
		ImGuiKey_Keypad1,
		ImGuiKey_Keypad2,
		ImGuiKey_Keypad3,
		ImGuiKey_Keypad4,
		ImGuiKey_Keypad5,
		ImGuiKey_Keypad6,
		ImGuiKey_Keypad7,
		ImGuiKey_Keypad8,
		ImGuiKey_Keypad9,
		ImGuiKey_KeypadDecimal,
		ImGuiKey_KeypadDivide,
		ImGuiKey_KeypadMultiply,
		ImGuiKey_KeypadSubtract,
		ImGuiKey_KeypadAdd,
		ImGuiKey_KeypadEnter,
		ImGuiKey_KeypadEqual,
		ImGuiKey_LeftShift,
		ImGuiKey_LeftCtrl,
		ImGuiKey_LeftAlt,
		ImGuiKey_LeftSuper,
		ImGuiKey_RightShift,
		ImGuiKey_RightCtrl,
		ImGuiKey_RightAlt,
		ImGuiKey_RightSuper,
		ImGuiKey_Menu,
	};

	return keys[(int)key];
}

RFAPI int ImGui_TranslateMouseButton(MouseButton button)
{
	static int buttons[(int)MouseButton::Count] = {
		0,
		ImGuiMouseButton_Left,
		ImGuiMouseButton_Right,
		ImGuiMouseButton_Middle,
		3,
		4
	};

	return buttons[(int)button];
}

bool ImGuiLayerProcessEvent(const Event* ev)
{
	ImGuiIO& io = ImGui::GetIO();

	bool mouseLocked = Application_IsMouseLocked();
	bool imguiHovered = ImGui::IsAnyItemActive() || ImGui::IsAnyItemHovered() || ImGui::IsWindowHovered(ImGuiHoveredFlags_AnyWindow | ImGuiHoveredFlags_AllowWhenBlockedByPopup);

	switch (ev->type)
	{
	case EventType::Size:
	{
		SizeEvent* sizeEvent = (SizeEvent*)ev;

		io.DisplaySize = ImVec2((float)sizeEvent->width, (float)sizeEvent->height);
		io.DisplayFramebufferScale = ImVec2(1.0f, 1.0f);

		return false;
	}
	break;

	case EventType::Mouse:
	{
		if (mouseLocked)
			return false;

		MouseEvent* mouseEvent = (MouseEvent*)ev;
		if (mouseEvent->move)
		{
			io.MousePos = ImVec2((float)mouseEvent->x, (float)mouseEvent->y);
			io.MouseWheel = (float)(mouseEvent->z - m_lastScroll);
			m_lastScroll = mouseEvent->z;

			if (imguiHovered)
				return true;
			return false;
		}
		else
		{
			io.MouseDown[ImGui_TranslateMouseButton(mouseEvent->button)] = mouseEvent->down;

			if (mouseEvent->down && imguiHovered)
				return true;
			return false;
		}
	}
	break;

	case EventType::Key:
	{
		if (mouseLocked)
			return false;

		KeyEvent* keyEvent = (KeyEvent*)ev;

		if (imguiHovered || !keyEvent->down)
		{
			io.KeysDown[(int)keyEvent->key] = keyEvent->down;

			io.KeyCtrl = io.KeysDown[(int)KeyCode::LeftCtrl] || io.KeysDown[(int)KeyCode::RightCtrl];
			io.KeyShift = io.KeysDown[(int)KeyCode::LeftShift] || io.KeysDown[(int)KeyCode::RightShift];
			io.KeyAlt = io.KeysDown[(int)KeyCode::LeftAlt] || io.KeysDown[(int)KeyCode::RightAlt];
			io.KeySuper = io.KeysDown[(int)KeyCode::LeftMeta] || io.KeysDown[(int)KeyCode::RightMeta];

			if (keyEvent->down)
				return true;
		}

		return false;
	}
	break;

	case EventType::Char:
	{
		if (mouseLocked)
			return false;

		CharEvent* charEvent = (CharEvent*)ev;

		if (charEvent->value > 0 && charEvent->value < 0x10000)
			io.AddInputCharacter(charEvent->value);

		return true;
	}
	break;

	default:
		return false;
	}
}

RFAPI float ImGui_GetMouseScroll()
{
	return ImGui::GetIO().MouseWheel;
}

/*
RFAPI bool ImGui_Tab(const char* label, ImVec2 size_arg, ImGuiButtonFlags flags)
{
	using namespace ImGui;

	ImGuiWindow* window = GetCurrentWindow();
	if (window->SkipItems)
		return false;

	ImGuiContext& g = *GImGui;
	const ImGuiStyle& style = g.Style;
	const ImGuiID id = window->GetID(label);
	const ImVec2 label_size = CalcTextSize(label, NULL, true);

	ImVec2 pos = window->DC.CursorPos;
	if ((flags & ImGuiButtonFlags_AlignTextBaseLine) && style.FramePadding.y < window->DC.CurrLineTextBaseOffset) // Try to vertically align buttons that are smaller/have no padding so that text baseline matches (bit hacky, since it shouldn't be a flag)
		pos.y += window->DC.CurrLineTextBaseOffset - style.FramePadding.y;
	ImVec2 size = CalcItemSize(size_arg, label_size.x + style.FramePadding.x * 2.0f, label_size.y + style.FramePadding.y * 2.0f);

	const ImRect bb(pos, pos + size);
	ItemSize(size, style.FramePadding.y);
	if (!ItemAdd(bb, id))
		return false;

	bool hovered, held;
	bool pressed = ButtonBehavior(bb, id, &hovered, &held, flags);

	// Render
	const ImU32 col = GetColorU32((held && hovered) ? ImGuiCol_ButtonActive : hovered ? ImGuiCol_ButtonHovered : ImGuiCol_Button);
	RenderNavHighlight(bb, id);
	RenderFrame(bb.Min, bb.Max, col, true, style.FrameRounding);

	window->DrawList->AddRectFilled(bb.Min, bb.Max, col, style.FrameRounding);
	const float border_size = g.Style.FrameBorderSize;
	if (true && border_size > 0.0f)
	{
		window->DrawList->AddRect(bb.Min + ImVec2(1, 1), bb.Max + ImVec2(1, 1), GetColorU32(ImGuiCol_BorderShadow), style.FrameRounding, 0, border_size);
		window->DrawList->AddRect(bb.Min, bb.Max, GetColorU32(ImGuiCol_Border), style.FrameRounding, 0, border_size);
	}

	if (g.LogEnabled)
		LogSetNextTextDecoration("[", "]");
	RenderTextClipped(bb.Min + style.FramePadding, bb.Max - style.FramePadding, label, NULL, &label_size, style.ButtonTextAlign, &bb);

	// Automatically close popups
	//if (pressed && !(flags & ImGuiButtonFlags_DontClosePopups) && (window->Flags & ImGuiWindowFlags_Popup))
	//    CloseCurrentPopup();

	IMGUI_TEST_ENGINE_ITEM_INFO(id, label, g.LastItemData.StatusFlags);
	return pressed;
}
*/
