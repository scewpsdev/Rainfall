using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public enum ImGuiWindowFlags : int
	{
		None = 0,
		NoTitleBar = 1 << 0,   // Disable title-bar
		NoResize = 1 << 1,   // Disable user resizing with the lower-right grip
		NoMove = 1 << 2,   // Disable user moving the window
		NoScrollbar = 1 << 3,   // Disable scrollbars (window can still scroll with mouse or programmatically)
		NoScrollWithMouse = 1 << 4,   // Disable user vertically scrolling with mouse wheel. On child window, mouse wheel will be forwarded to the parent unless NoScrollbar is also set.
		NoCollapse = 1 << 5,   // Disable user collapsing window by double-clicking on it. Also referred to as Window Menu Button (e.g. within a docking node).
		AlwaysAutoResize = 1 << 6,   // Resize every window to its content every frame
		NoBackground = 1 << 7,   // Disable drawing background color (WindowBg, etc.) and outside border. Similar as using SetNextWindowBgAlpha(0.0f).
		NoSavedSettings = 1 << 8,   // Never load/save settings in .ini file
		NoMouseInputs = 1 << 9,   // Disable catching mouse, hovering test with pass through.
		MenuBar = 1 << 10,  // Has a menu-bar
		HorizontalScrollbar = 1 << 11,  // Allow horizontal scrollbar to appear (off by default). You may use SetNextWindowContentSize(ImVec2(width,0.0f)); prior to calling Begin() to specify width. Read code in imgui_demo in the "Horizontal Scrolling" section.
		NoFocusOnAppearing = 1 << 12,  // Disable taking focus when transitioning from hidden to visible state
		NoBringToFrontOnFocus = 1 << 13,  // Disable bringing window to front when taking focus (e.g. clicking on it or programmatically giving it focus)
		AlwaysVerticalScrollbar = 1 << 14,  // Always show vertical scrollbar (even if ContentSize.y < Size.y)
		AlwaysHorizontalScrollbar = 1 << 15,  // Always show horizontal scrollbar (even if ContentSize.x < Size.x)
		AlwaysUseWindowPadding = 1 << 16,  // Ensure child windows without border uses style.WindowPadding (ignored by default for non-bordered child windows, because more convenient)
		NoNavInputs = 1 << 18,  // No gamepad/keyboard navigation within the window
		NoNavFocus = 1 << 19,  // No focusing toward this window with gamepad/keyboard navigation (e.g. skipped by CTRL+TAB)
		UnsavedDocument = 1 << 20,  // Display a dot next to the title. When used in a tab/docking context, tab is selected when clicking the X + closure is not assumed (will wait for user to stop submitting the tab). Otherwise closure is assumed when pressing the X, so if you keep submitting the tab may reappear at end of tab bar.
		NoDocking = 1 << 21,  // Disable docking of this window

		NoNav = NoNavInputs | NoNavFocus,
		NoDecoration = NoTitleBar | NoResize | NoScrollbar | NoCollapse,
		NoInputs = NoMouseInputs | NoNavInputs | NoNavFocus,

		NavFlattened = 1 << 23,  // [BETA] On child window: allow gamepad/keyboard navigation to cross over parent border to this child or between sibling child windows.
		ChildWindow = 1 << 24,  // Don't use! For internal use by BeginChild()
		Tooltip = 1 << 25,  // Don't use! For internal use by BeginTooltip()
		Popup = 1 << 26,  // Don't use! For internal use by BeginPopup()
		Modal = 1 << 27,  // Don't use! For internal use by BeginPopupModal()
		ChildMenu = 1 << 28,  // Don't use! For internal use by BeginMenu()
		DockNodeHost = 1 << 29   // Don't use! For internal use by Begin()/NewFrame()

		// [Obsolete]
		//ImGuiWindowFlags_ResizeFromAnySide    = 1 << 17,  // [Obsolete] --> Set io.ConfigWindowsResizeFromEdges=true and make sure mouse cursors are supported by backend (io.BackendFlags & ImGuiBackendFlags_HasMouseCursors)
	}

	public enum ImGuiInputTextFlags : int
	{
		ImGuiInputTextFlags_None = 0,
		ImGuiInputTextFlags_CharsDecimal = 1 << 0,   // Allow 0123456789.+-*/
		ImGuiInputTextFlags_CharsHexadecimal = 1 << 1,   // Allow 0123456789ABCDEFabcdef
		ImGuiInputTextFlags_CharsUppercase = 1 << 2,   // Turn a..z into A..Z
		ImGuiInputTextFlags_CharsNoBlank = 1 << 3,   // Filter out spaces, tabs
		ImGuiInputTextFlags_AutoSelectAll = 1 << 4,   // Select entire text when first taking mouse focus
		ImGuiInputTextFlags_EnterReturnsTrue = 1 << 5,   // Return 'true' when Enter is pressed (as opposed to every time the value was modified). Consider looking at the IsItemDeactivatedAfterEdit() function.
		ImGuiInputTextFlags_CallbackCompletion = 1 << 6,   // Callback on pressing TAB (for completion handling)
		ImGuiInputTextFlags_CallbackHistory = 1 << 7,   // Callback on pressing Up/Down arrows (for history handling)
		ImGuiInputTextFlags_CallbackAlways = 1 << 8,   // Callback on each iteration. User code may query cursor position, modify text buffer.
		ImGuiInputTextFlags_CallbackCharFilter = 1 << 9,   // Callback on character inputs to replace or discard them. Modify 'EventChar' to replace or discard, or return 1 in callback to discard.
		ImGuiInputTextFlags_AllowTabInput = 1 << 10,  // Pressing TAB input a '\t' character into the text field
		ImGuiInputTextFlags_CtrlEnterForNewLine = 1 << 11,  // In multi-line mode, unfocus with Enter, add new line with Ctrl+Enter (default is opposite: unfocus with Ctrl+Enter, add line with Enter).
		ImGuiInputTextFlags_NoHorizontalScroll = 1 << 12,  // Disable following the cursor horizontally
		ImGuiInputTextFlags_AlwaysOverwrite = 1 << 13,  // Overwrite mode
		ImGuiInputTextFlags_ReadOnly = 1 << 14,  // Read-only mode
		ImGuiInputTextFlags_Password = 1 << 15,  // Password mode, display all characters as '*'
		ImGuiInputTextFlags_NoUndoRedo = 1 << 16,  // Disable undo/redo. Note that input text owns the text data while active, if you want to provide your own undo/redo stack you need e.g. to call ClearActiveID().
		ImGuiInputTextFlags_CharsScientific = 1 << 17,  // Allow 0123456789.+-*/eE (Scientific notation input)
		ImGuiInputTextFlags_CallbackResize = 1 << 18,  // Callback on buffer capacity changes request (beyond 'buf_size' parameter value), allowing the string to grow. Notify when the string wants to be resized (for string types which hold a cache of their Size). You will be provided a new BufSize in the callback and NEED to honor it. (see misc/cpp/imgui_stdlib.h for an example of using this)
		ImGuiInputTextFlags_CallbackEdit = 1 << 19   // Callback on any edit (note that InputText() already returns true on edit, the callback is useful mainly to manipulate the underlying buffer while focus is active)
	}

	public enum ImGuiTreeNodeFlags : int
	{
		None = 0,
		Selected = 1 << 0,   // Draw as selected
		Framed = 1 << 1,   // Draw frame with background (e.g. for CollapsingHeader)
		AllowItemOverlap = 1 << 2,   // Hit testing to allow subsequent widgets to overlap this one
		NoTreePushOnOpen = 1 << 3,   // Don't do a TreePush() when open (e.g. for CollapsingHeader) = no extra indent nor pushing on ID stack
		NoAutoOpenOnLog = 1 << 4,   // Don't automatically and temporarily open node when Logging is active (by default logging will automatically open tree nodes)
		DefaultOpen = 1 << 5,   // Default node to be open
		OpenOnDoubleClick = 1 << 6,   // Need double-click to open node
		OpenOnArrow = 1 << 7,   // Only open when clicking on the arrow part. If ImGuiTreeNodeFlags_OpenOnDoubleClick is also set, single-click arrow or double-click all box to open.
		Leaf = 1 << 8,   // No collapsing, no arrow (use as a convenience for leaf nodes).
		Bullet = 1 << 9,   // Display a bullet instead of arrow
		FramePadding = 1 << 10,  // Use FramePadding (even for an unframed text node) to vertically align text baseline to regular widget height. Equivalent to calling AlignTextToFramePadding().
		SpanAvailWidth = 1 << 11,  // Extend hit box to the right-most edge, even if not framed. This is not the default in order to allow adding other items on the same line. In the future we may refactor the hit system to be front-to-back, allowing natural overlaps and then this can become the default.
		SpanFullWidth = 1 << 12,  // Extend hit box to the left-most and right-most edges (bypass the indented area).
		NavLeftJumpsBackHere = 1 << 13,  // (WIP) Nav: left direction may move to this TreeNode() from any of its child (items submitted between TreeNode and TreePop)
										 //ImGuiTreeNodeFlags_NoScrollOnOpen     = 1 << 14,  // FIXME: TODO: Disable automatic scroll on TreePop() if node got just open and contents is not visible
		CollapsingHeader = Framed | NoTreePushOnOpen | NoAutoOpenOnLog
	}

	public enum ImGuiComboFlags : int
	{
		ImGuiComboFlags_None = 0,
		ImGuiComboFlags_PopupAlignLeft = 1 << 0,   // Align the popup toward the left by default
		ImGuiComboFlags_HeightSmall = 1 << 1,   // Max ~4 items visible. Tip: If you want your combo popup to be a specific size you can use SetNextWindowSizeConstraints() prior to calling BeginCombo()
		ImGuiComboFlags_HeightRegular = 1 << 2,   // Max ~8 items visible (default)
		ImGuiComboFlags_HeightLarge = 1 << 3,   // Max ~20 items visible
		ImGuiComboFlags_HeightLargest = 1 << 4,   // As many fitting items as possible
		ImGuiComboFlags_NoArrowButton = 1 << 5,   // Display on the preview box without the square arrow button
		ImGuiComboFlags_NoPreview = 1 << 6,   // Display only a square arrow button
		ImGuiComboFlags_HeightMask_ = ImGuiComboFlags_HeightSmall | ImGuiComboFlags_HeightRegular | ImGuiComboFlags_HeightLarge | ImGuiComboFlags_HeightLargest
	}

	public enum ImGuiHoveredFlags : int
	{
		None = 0,        // Return true if directly over the item/window, not obstructed by another window, not obstructed by an active popup or modal blocking inputs under them.
		ChildWindows = 1 << 0,   // IsWindowHovered() only: Return true if any children of the window is hovered
		RootWindow = 1 << 1,   // IsWindowHovered() only: Test from root window (top most parent of the current hierarchy)
		AnyWindow = 1 << 2,   // IsWindowHovered() only: Return true if any window is hovered
		NoPopupHierarchy = 1 << 3,   // IsWindowHovered() only: Do not consider popup hierarchy (do not treat popup emitter as parent of popup) (when used with _ChildWindows or _RootWindow)
		DockHierarchy = 1 << 4,   // IsWindowHovered() only: Consider docking hierarchy (treat dockspace host as parent of docked window) (when used with _ChildWindows or _RootWindow)
		AllowWhenBlockedByPopup = 1 << 5,   // Return true even if a popup window is normally blocking access to this item/window
											//ImGuiHoveredFlags_AllowWhenBlockedByModal     = 1 << 6,   // Return true even if a modal popup window is normally blocking access to this item/window. FIXME-TODO: Unavailable yet.
		AllowWhenBlockedByActiveItem = 1 << 7,   // Return true even if an active item is blocking access to this item/window. Useful for Drag and Drop patterns.
		AllowWhenOverlapped = 1 << 8,   // IsItemHovered() only: Return true even if the position is obstructed or overlapped by another window
		AllowWhenDisabled = 1 << 9,   // IsItemHovered() only: Return true even if the item is disabled
		NoNavOverride = 1 << 10,  // Disable using gamepad/keyboard navigation state when active, always query mouse.
		RectOnly = AllowWhenBlockedByPopup | AllowWhenBlockedByActiveItem | AllowWhenOverlapped,
		RootAndChildWindows = RootWindow | ChildWindows
	}

	public enum ImGuiDataType : int
	{
		ImGuiDataType_S8,       // signed char / char (with sensible compilers)
		ImGuiDataType_U8,       // unsigned char
		ImGuiDataType_S16,      // short
		ImGuiDataType_U16,      // unsigned short
		ImGuiDataType_S32,      // int
		ImGuiDataType_U32,      // unsigned int
		ImGuiDataType_S64,      // long long / __int64
		ImGuiDataType_U64,      // unsigned long long / unsigned __int64
		ImGuiDataType_Float,    // float
		ImGuiDataType_Double,   // double
		ImGuiDataType_COUNT
	}

	public enum ImGuiDir : int
	{
		ImGuiDir_None = -1,
		ImGuiDir_Left = 0,
		ImGuiDir_Right = 1,
		ImGuiDir_Up = 2,
		ImGuiDir_Down = 3,
		ImGuiDir_COUNT
	}

	public enum ImGuiCond : int
	{
		ImGuiCond_None = 0,        // No condition (always set the variable), same as _Always
		ImGuiCond_Always = 1 << 0,   // No condition (always set the variable)
		ImGuiCond_Once = 1 << 1,   // Set the variable once per runtime session (only the first call will succeed)
		ImGuiCond_FirstUseEver = 1 << 2,   // Set the variable if the object/window has no persistently saved data (no entry in .ini file)
		ImGuiCond_Appearing = 1 << 3    // Set the variable if the object/window is appearing after being hidden/inactive (or the first time)
	}

	public enum ImGuiStyleVar : int
	{
		// Enum name --------------------- // Member in ImGuiStyle structure (see ImGuiStyle for descriptions)
		Alpha,               // float     Alpha
		DisabledAlpha,       // float     DisabledAlpha
		WindowPadding,       // ImVec2    WindowPadding
		WindowRounding,      // float     WindowRounding
		WindowBorderSize,    // float     WindowBorderSize
		WindowMinSize,       // ImVec2    WindowMinSize
		WindowTitleAlign,    // ImVec2    WindowTitleAlign
		ChildRounding,       // float     ChildRounding
		ChildBorderSize,     // float     ChildBorderSize
		PopupRounding,       // float     PopupRounding
		PopupBorderSize,     // float     PopupBorderSize
		FramePadding,        // ImVec2    FramePadding
		FrameRounding,       // float     FrameRounding
		FrameBorderSize,     // float     FrameBorderSize
		ItemSpacing,         // ImVec2    ItemSpacing
		ItemInnerSpacing,    // ImVec2    ItemInnerSpacing
		IndentSpacing,       // float     IndentSpacing
		CellPadding,         // ImVec2    CellPadding
		ScrollbarSize,       // float     ScrollbarSize
		ScrollbarRounding,   // float     ScrollbarRounding
		GrabMinSize,         // float     GrabMinSize
		GrabRounding,        // float     GrabRounding
		TabRounding,         // float     TabRounding
		ButtonTextAlign,     // ImVec2    ButtonTextAlign
		SelectableTextAlign, // ImVec2    SelectableTextAlign
		Count
	}

	// Flags for InvisibleButton() [extended in imgui_internal.h]
	public enum ImGuiButtonFlags : int
	{
		ImGuiButtonFlags_None = 0,
		ImGuiButtonFlags_MouseButtonLeft = 1 << 0,   // React on left mouse button (default)
		ImGuiButtonFlags_MouseButtonRight = 1 << 1,   // React on right mouse button
		ImGuiButtonFlags_MouseButtonMiddle = 1 << 2,   // React on center mouse button

		// [Internal]
		ImGuiButtonFlags_MouseButtonMask_ = ImGuiButtonFlags_MouseButtonLeft | ImGuiButtonFlags_MouseButtonRight | ImGuiButtonFlags_MouseButtonMiddle,
		ImGuiButtonFlags_MouseButtonDefault_ = ImGuiButtonFlags_MouseButtonLeft
	}

	// Flags for ColorEdit3() / ColorEdit4() / ColorPicker3() / ColorPicker4() / ColorButton()
	public enum ImGuiColorEditFlags : int
	{
		ImGuiColorEditFlags_None = 0,
		ImGuiColorEditFlags_NoAlpha = 1 << 1,   //              // ColorEdit, ColorPicker, ColorButton: ignore Alpha component (will only read 3 components from the input pointer).
		ImGuiColorEditFlags_NoPicker = 1 << 2,   //              // ColorEdit: disable picker when clicking on color square.
		ImGuiColorEditFlags_NoOptions = 1 << 3,   //              // ColorEdit: disable toggling options menu when right-clicking on inputs/small preview.
		ImGuiColorEditFlags_NoSmallPreview = 1 << 4,   //              // ColorEdit, ColorPicker: disable color square preview next to the inputs. (e.g. to show only the inputs)
		ImGuiColorEditFlags_NoInputs = 1 << 5,   //              // ColorEdit, ColorPicker: disable inputs sliders/text widgets (e.g. to show only the small preview color square).
		ImGuiColorEditFlags_NoTooltip = 1 << 6,   //              // ColorEdit, ColorPicker, ColorButton: disable tooltip when hovering the preview.
		ImGuiColorEditFlags_NoLabel = 1 << 7,   //              // ColorEdit, ColorPicker: disable display of inline text label (the label is still forwarded to the tooltip and picker).
		ImGuiColorEditFlags_NoSidePreview = 1 << 8,   //              // ColorPicker: disable bigger color preview on right side of the picker, use small color square preview instead.
		ImGuiColorEditFlags_NoDragDrop = 1 << 9,   //              // ColorEdit: disable drag and drop target. ColorButton: disable drag and drop source.
		ImGuiColorEditFlags_NoBorder = 1 << 10,  //              // ColorButton: disable border (which is enforced by default)

		// User Options (right-click on widget to change some of them).
		ImGuiColorEditFlags_AlphaBar = 1 << 16,  //              // ColorEdit, ColorPicker: show vertical alpha bar/gradient in picker.
		ImGuiColorEditFlags_AlphaPreview = 1 << 17,  //              // ColorEdit, ColorPicker, ColorButton: display preview as a transparent color over a checkerboard, instead of opaque.
		ImGuiColorEditFlags_AlphaPreviewHalf = 1 << 18,  //              // ColorEdit, ColorPicker, ColorButton: display half opaque / half checkerboard, instead of opaque.
		ImGuiColorEditFlags_HDR = 1 << 19,  //              // (WIP) ColorEdit: Currently only disable 0.0f..1.0f limits in RGBA edition (note: you probably want to use ImGuiColorEditFlags_Float flag as well).
		ImGuiColorEditFlags_DisplayRGB = 1 << 20,  // [Display]    // ColorEdit: override _display_ type among RGB/HSV/Hex. ColorPicker: select any combination using one or more of RGB/HSV/Hex.
		ImGuiColorEditFlags_DisplayHSV = 1 << 21,  // [Display]    // "
		ImGuiColorEditFlags_DisplayHex = 1 << 22,  // [Display]    // "
		ImGuiColorEditFlags_Uint8 = 1 << 23,  // [DataType]   // ColorEdit, ColorPicker, ColorButton: _display_ values formatted as 0..255.
		ImGuiColorEditFlags_Float = 1 << 24,  // [DataType]   // ColorEdit, ColorPicker, ColorButton: _display_ values formatted as 0.0f..1.0f floats instead of 0..255 integers. No round-trip of value via integers.
		ImGuiColorEditFlags_PickerHueBar = 1 << 25,  // [Picker]     // ColorPicker: bar for Hue, rectangle for Sat/Value.
		ImGuiColorEditFlags_PickerHueWheel = 1 << 26,  // [Picker]     // ColorPicker: wheel for Hue, triangle for Sat/Value.
		ImGuiColorEditFlags_InputRGB = 1 << 27,  // [Input]      // ColorEdit, ColorPicker: input and output data in RGB format.
		ImGuiColorEditFlags_InputHSV = 1 << 28,  // [Input]      // ColorEdit, ColorPicker: input and output data in HSV format.

		// Defaults Options. You can set application defaults using SetColorEditOptions(). The intent is that you probably don't want to
		// override them in most of your calls. Let the user choose via the option menu and/or call SetColorEditOptions() once during startup.
		ImGuiColorEditFlags_DefaultOptions_ = ImGuiColorEditFlags_Uint8 | ImGuiColorEditFlags_DisplayRGB | ImGuiColorEditFlags_InputRGB | ImGuiColorEditFlags_PickerHueBar,

		// [Internal] Masks
		ImGuiColorEditFlags_DisplayMask_ = ImGuiColorEditFlags_DisplayRGB | ImGuiColorEditFlags_DisplayHSV | ImGuiColorEditFlags_DisplayHex,
		ImGuiColorEditFlags_DataTypeMask_ = ImGuiColorEditFlags_Uint8 | ImGuiColorEditFlags_Float,
		ImGuiColorEditFlags_PickerMask_ = ImGuiColorEditFlags_PickerHueWheel | ImGuiColorEditFlags_PickerHueBar,
		ImGuiColorEditFlags_InputMask_ = ImGuiColorEditFlags_InputRGB | ImGuiColorEditFlags_InputHSV

		// Obsolete names (will be removed)
		// ImGuiColorEditFlags_RGB = ImGuiColorEditFlags_DisplayRGB, ImGuiColorEditFlags_HSV = ImGuiColorEditFlags_DisplayHSV, ImGuiColorEditFlags_HEX = ImGuiColorEditFlags_DisplayHex  // [renamed in 1.69]
	}

	// Flags for DragFloat(), DragInt(), SliderFloat(), SliderInt() etc.
	// We use the same sets of flags for DragXXX() and SliderXXX() functions as the features are the same and it makes it easier to swap them.
	public enum ImGuiSliderFlags : int
	{
		ImGuiSliderFlags_None = 0,
		ImGuiSliderFlags_AlwaysClamp = 1 << 4,       // Clamp value to min/max bounds when input manually with CTRL+Click. By default CTRL+Click allows going out of bounds.
		ImGuiSliderFlags_Logarithmic = 1 << 5,       // Make the widget logarithmic (linear otherwise). Consider using ImGuiSliderFlags_NoRoundToFormat with this if using a format-string with small amount of digits.
		ImGuiSliderFlags_NoRoundToFormat = 1 << 6,       // Disable rounding underlying value to match precision of the display format string (e.g. %.3f values are rounded to those 3 digits)
		ImGuiSliderFlags_NoInput = 1 << 7,       // Disable CTRL+Click or Enter key allowing to input text directly into the widget
		ImGuiSliderFlags_InvalidMask_ = 0x7000000F    // [Internal] We treat using those bits as being potentially a 'float power' argument from the previous API that has got miscast to this enum, and will trigger an assert if needed.
	}

	public struct ImTexture2D
	{
		public ushort texture;
		public byte flags;
		public byte mip;
	}

	public static class ImGui
	{
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool Begin([MarshalAs(UnmanagedType.LPStr)] string name, bool* p_open = null, ImGuiWindowFlags flags = 0);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void End();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void GetWindowPos(out Vector2 pos);
		public static Vector2 GetWindowPos() { GetWindowPos(out Vector2 pos); return pos; }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void GetWindowSize(out Vector2 size);
		public static Vector2 GetWindowSize() { GetWindowSize(out Vector2 size); return size; }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetNextWindowPos(Vector2 pos, ImGuiCond cond = 0, Vector2 pivot = default); // set next window position. call before Begin(). use pivot=(0.5f,0.5f) to center on given point, etc.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetNextWindowSize(Vector2 size, ImGuiCond cond = 0);                  // set next window size. set axis to 0.0f to force an auto-fit on this axis. call before Begin()
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetNextWindowSizeConstraints(Vector2 size_min, Vector2 size_max, void* custom_callback = null, void* custom_callback_data = null); // set next window size limits. use -1,-1 on either X/Y axis to preserve the current size. Sizes will be rounded down. Use callback to apply non-trivial programmatic constraints.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetNextWindowContentSize(Vector2 size);                               // set next window content size (~ scrollable client area, which enforce the range of scrollbars). Not including window decorations (title bar, menu bar, etc.) nor WindowPadding. set an axis to 0.0f to leave it automatic. call before Begin()
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetNextWindowCollapsed(bool collapsed, ImGuiCond cond = 0);                 // set next window collapsed state. call before Begin()
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetNextWindowFocus();                                                       // set next window to be focused / top-most. call before Begin()
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetNextWindowBgAlpha(float alpha);                                          // set next window background color alpha. helper to easily override the Alpha component of ImGuiCol_WindowBg/ChildBg/PopupBg. you may also use ImGuiWindowFlags_NoBackground.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetNextWindowViewport(uint viewport_id);                                 // set next window viewport

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void GetContentRegionAvail(out Vector2 region);
		public static Vector2 GetContentRegionAvail() { GetContentRegionAvail(out Vector2 region); return region; }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "PushStyleVarF")]
		public static extern unsafe void PushStyleVar(ImGuiStyleVar idx, float val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "PushStyleVar2")]
		public static extern unsafe void PushStyleVar(ImGuiStyleVar idx, Vector2 val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void PopStyleVar(int count = 1);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void Separator();                                                    // separator, generally horizontal. inside a menu bar or in horizontal layout mode, this becomes a vertical separator.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SameLine(float offset_from_start_x = 0.0f, float spacing = -1.0f);  // call between widgets or groups to layout them horizontally. X position given in window coordinates.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void NewLine();                                                      // undo a SameLine() or force a new line when in an horizontal-layout context.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void Spacing();                                                      // add vertical spacing.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void Dummy(Vector2 size);                                      // add a dummy item of given size. unlike InvisibleButton(), Dummy() won't take the mouse click or be navigable into.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void Indent(float indent_w = 0.0f);                                  // move content position toward the right, by indent_w, or style.IndentSpacing if indent_w <= 0
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void Unindent(float indent_w = 0.0f);                                // move content position back to the left, by indent_w, or style.IndentSpacing if indent_w <= 0
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void BeginGroup();                                                   // lock horizontal starting position
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void EndGroup();                                                     // unlock horizontal starting position + capture the whole group bounding box into one "item" (so you can use IsItemHovered() or layout primitives such as SameLine() on whole group, etc.)

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ImGuiGetCursorPos")]
		static extern unsafe void GetCursorPos(out Vector2 outPos);
		public static Vector2 GetCursorPos() { GetCursorPos(out Vector2 pos); return pos; }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe float GetFrameHeight();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void TextUnformatted([MarshalAs(UnmanagedType.LPStr)] string text, [MarshalAs(UnmanagedType.LPStr)] string text_end = null); // raw text without formatting. Roughly equivalent to Text("%s", text) but: A) doesn't require null terminated string if 'text_end' is specified, B) it's faster, no memory copy is done, no buffer size limits, recommended for long chunks of text.

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool Button([MarshalAs(UnmanagedType.LPStr)] string label, Vector2 size = default);   // button

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SmallButton([MarshalAs(UnmanagedType.LPStr)] string label);                                 // button with FramePadding=(0,0) to easily embed within text

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InvisibleButton([MarshalAs(UnmanagedType.LPStr)] string str_id, Vector2 size, ImGuiButtonFlags flags = 0); // flexible button behavior without the visuals, frequently useful to build custom behaviors using the public api (along with IsItemActive, IsItemHovered, etc.)

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool ArrowButton([MarshalAs(UnmanagedType.LPStr)] string str_id, ImGuiDir dir);                  // square button with an arrow shape

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void Image(ImTexture2D user_texture_id, Vector2 size, Vector2 uv0, Vector2 uv1, Vector4 tint_col, Vector4 border_col);
		public static void Image(Texture texture, Vector2 size, bool alphaBlend, bool mipmaps)
		{
			Image(new ImTexture2D { texture = texture.handle, flags = (byte)(alphaBlend ? 1 : 0), mip = (byte)(mipmaps ? 1 : 0) }, size, new Vector2(0, 0), new Vector2(1, 1), new Vector4(1), new Vector4(0));
		}
		public static void Image(Texture texture, Vector2 size)
		{
			Image(new ImTexture2D { texture = texture.handle, flags = 0, mip = 0 }, size, new Vector2(0, 0), new Vector2(1, 1), new Vector4(1), new Vector4(0));
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool ImageButton(void* user_texture_id, Vector2 size, Vector2 uv0, Vector2 uv1, int frame_padding, Vector4 bg_col, Vector4 tint_col);    // <0 frame_padding uses default frame padding settings. 0 for no padding

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool Checkbox([MarshalAs(UnmanagedType.LPStr)] string label, bool* v);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool CheckboxFlags([MarshalAs(UnmanagedType.LPStr)] string label, int* flags, int flags_value);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool RadioButton([MarshalAs(UnmanagedType.LPStr)] string label, bool active);                    // use with e.g. if (RadioButton("one", my_value==1)) { my_value = 1; }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ProgressBar(float fraction, Vector2 size_arg, [MarshalAs(UnmanagedType.LPStr)] string overlay = null);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void Bullet();                                                       // draw a small circle + keep the cursor on the same line. advance cursor x position by GetTreeNodeToLabelSpacing(), same distance that TreeNode() uses

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool BeginCombo([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPStr)] string preview_value, ImGuiComboFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void EndCombo(); // only call EndCombo() if BeginCombo() returns true!

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragFloat([MarshalAs(UnmanagedType.LPStr)] string label, ref float v, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiSliderFlags flags = 0);     // If v_min >= v_max we have no bound
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragFloat2([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 2)] float[] v, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragFloat3([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)] float[] v, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragFloat4([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)] float[] v, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragFloatRange2([MarshalAs(UnmanagedType.LPStr)] string label, float* v_current_min, float* v_current_max, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", [MarshalAs(UnmanagedType.LPStr)] string format_max = null, ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragInt([MarshalAs(UnmanagedType.LPStr)] string label, ref int* v, float v_speed = 1.0f, int v_min = 0, int v_max = 0, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", ImGuiSliderFlags flags = 0);  // If v_min >= v_max we have no bound
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragInt2([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 2)] int[] v, float v_speed = 1.0f, int v_min = 0, int v_max = 0, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragInt3([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 3)] int[] v, float v_speed = 1.0f, int v_min = 0, int v_max = 0, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragInt4([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 4)] int[] v, float v_speed = 1.0f, int v_min = 0, int v_max = 0, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragIntRange2([MarshalAs(UnmanagedType.LPStr)] string label, int* v_current_min, int* v_current_max, float v_speed = 1.0f, int v_min = 0, int v_max = 0, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", [MarshalAs(UnmanagedType.LPStr)] string format_max = null, ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragScalar([MarshalAs(UnmanagedType.LPStr)] string label, ImGuiDataType data_type, void* p_data, float v_speed = 1.0f, void* p_min = null, void* p_max = null, [MarshalAs(UnmanagedType.LPStr)] string format = null, ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool DragScalarN([MarshalAs(UnmanagedType.LPStr)] string label, ImGuiDataType data_type, void* p_data, int components, float v_speed = 1.0f, void* p_min = null, void* p_max = null, [MarshalAs(UnmanagedType.LPStr)] string format = null, ImGuiSliderFlags flags = 0);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderFloat([MarshalAs(UnmanagedType.LPStr)] string label, float* v, float v_min, float v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiSliderFlags flags = 0);     // adjust format to decorate the value with a prefix or a suffix for in-slider labels or unit display.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderFloat2([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 2)] float[] v, float v_min, float v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderFloat3([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)] float[] v, float v_min, float v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderFloat4([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)] float[] v, float v_min, float v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderAngle([MarshalAs(UnmanagedType.LPStr)] string label, float* v_rad, float v_degrees_min = -360.0f, float v_degrees_max = +360.0f, [MarshalAs(UnmanagedType.LPStr)] string format = "%.0f deg", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderInt([MarshalAs(UnmanagedType.LPStr)] string label, ref int v, int v_min, int v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderInt2([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 2)] int[] v, int v_min, int v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderInt3([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 3)] int[] v, int v_min, int v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderInt4([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 4)] int[] v, int v_min, int v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderScalar([MarshalAs(UnmanagedType.LPStr)] string label, ImGuiDataType data_type, void* p_data, void* p_min, void* p_max, [MarshalAs(UnmanagedType.LPStr)] string format = null, ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool SliderScalarN([MarshalAs(UnmanagedType.LPStr)] string label, ImGuiDataType data_type, void* p_data, int components, void* p_min, void* p_max, [MarshalAs(UnmanagedType.LPStr)] string format = null, ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool VSliderFloat([MarshalAs(UnmanagedType.LPStr)] string label, Vector2 size, float* v, float v_min, float v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool VSliderInt([MarshalAs(UnmanagedType.LPStr)] string label, Vector2 size, int* v, int v_min, int v_max, [MarshalAs(UnmanagedType.LPStr)] string format = "%d", ImGuiSliderFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool VSliderScalar([MarshalAs(UnmanagedType.LPStr)] string label, Vector2 size, ImGuiDataType data_type, void* p_data, void* p_min, void* p_max, [MarshalAs(UnmanagedType.LPStr)] string format = null, ImGuiSliderFlags flags = 0);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputText([MarshalAs(UnmanagedType.LPStr)] string label, char* buf, ulong buf_size, ImGuiInputTextFlags flags = 0, void* callback = null, void* user_data = null);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputTextMultiline([MarshalAs(UnmanagedType.LPStr)] string label, char* buf, ulong buf_size, Vector2 size = default, ImGuiInputTextFlags flags = 0, void* callback = null, void* user_data = null);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputTextWithHint([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPStr)] string hint, char* buf, ulong buf_size, ImGuiInputTextFlags flags = 0, void* callback = null, void* user_data = null);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputFloat([MarshalAs(UnmanagedType.LPStr)] string label, float* v, float step = 0.0f, float step_fast = 0.0f, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputFloat2([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 2)] float[] v, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputFloat3([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 2)] float[] v, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputFloat4([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 2)] float[] v, [MarshalAs(UnmanagedType.LPStr)] string format = "%.3f", ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputInt([MarshalAs(UnmanagedType.LPStr)] string label, ref int v, int step = 1, int step_fast = 100, ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputInt2([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 2)] int[] v, ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputInt3([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 3)] int[] v, ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputInt4([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeConst = 4)] int[] v, ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputDouble([MarshalAs(UnmanagedType.LPStr)] string label, double* v, double step = 0.0, double step_fast = 0.0, [MarshalAs(UnmanagedType.LPStr)] string format = "%.6f", ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputScalar([MarshalAs(UnmanagedType.LPStr)] string label, ImGuiDataType data_type, void* p_data, void* p_step = null, void* p_step_fast = null, [MarshalAs(UnmanagedType.LPStr)] string format = null, ImGuiInputTextFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool InputScalarN([MarshalAs(UnmanagedType.LPStr)] string label, ImGuiDataType data_type, void* p_data, int components, void* p_step = null, void* p_step_fast = null, [MarshalAs(UnmanagedType.LPStr)] string format = null, ImGuiInputTextFlags flags = 0);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool ColorEdit3([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)] float[] col, ImGuiColorEditFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool ColorEdit4([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)] float[] col, ImGuiColorEditFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool ColorPicker3([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)] float[] col, ImGuiColorEditFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool ColorPicker4([MarshalAs(UnmanagedType.LPStr)] string label, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)] float[] col, ImGuiColorEditFlags flags = 0, float* ref_col = null);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool ColorButton([MarshalAs(UnmanagedType.LPStr)] string desc_id, Vector4 col, ImGuiColorEditFlags flags = 0, Vector2 size = default); // display a color square/button, hover for details, return true when pressed.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void SetColorEditOptions(ImGuiColorEditFlags flags);                     // initialize current options (generally on application startup) if you want to select a default format, picker type, etc. User will be able to change many settings, unless you pass the _NoOptions flag to your calls.

		// Widgets: Trees
		// - TreeNode functions return true when the node is open, in which case you need to also call TreePop() when you are finished displaying the tree node contents.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe bool TreeNode(string label);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool TreeNodeEx([MarshalAs(UnmanagedType.LPStr)] string label, ImGuiTreeNodeFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void TreePop();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool BeginMenuBar();                                                     // append to menu-bar of current window (requires ImGuiWindowFlags_MenuBar flag set on parent window).
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void EndMenuBar();                                                       // only call EndMenuBar() if BeginMenuBar() returns true!
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool BeginMainMenuBar();                                                 // create and append to a full screen menu-bar.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void EndMainMenuBar();                                                   // only call EndMainMenuBar() if BeginMainMenuBar() returns true!
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool BeginMenu(string label, bool enabled = true);                  // create a sub-menu entry. only call EndMenu() if this returns true!
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "EndMenu_")]
		public static extern unsafe void EndMenu();                                                          // only call EndMenu() if BeginMenu() returns true!
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern unsafe bool MenuItem(string label, string shortcut = null, bool selected = false, bool enabled = true);  // return true when activated.

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe bool IsItemHovered(ImGuiHoveredFlags flags = 0);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ShowDemoWindow(bool* p_open = null);        // create Demo window. demonstrate most ImGui features. call this to learn about the library! try to make it always available in your application!
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ShowMetricsWindow(bool* p_open = null);     // create Metrics/Debugger window. display Dear ImGui internals: windows, draw commands, various internal state, etc.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ShowStackToolWindow(bool* p_open = null);   // create Stack Tool window. hover items with mouse to query information about the source of their unique ID.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ShowAboutWindow(bool* p_open = null);       // create About window. display Dear ImGui version, credits and build/system information.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ShowStyleEditor(void* reference = null);    // add style editor block (not a window). you can pass in a reference ImGuiStyle structure to compare to, revert to and save to (else it uses the default style)
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe bool ShowStyleSelector([MarshalAs(UnmanagedType.LPStr)] string label);       // add style selector block (not a window), essentially a combo listing the default styles.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ShowFontSelector([MarshalAs(UnmanagedType.LPStr)] string label);        // add font selector block (not a window), essentially a combo listing the loaded fonts.
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ShowUserGuide();                            // add basic help/info block (not a window): how to manipulate ImGui as a end-user (mouse/keyboard controls).

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool IsKeyDown(int key);                                            // is key being held.
		public static bool IsKeyDown(KeyCode key) { return IsKeyDown(ImGui_TranslateKey(key)); }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool IsKeyPressed(int key, [MarshalAs(UnmanagedType.I1)] bool repeat);
		public static bool IsKeyPressed(KeyCode key, bool repeat = true) { return IsKeyPressed(ImGui_TranslateKey(key), repeat); }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool IsKeyReleased(int key);                                        // was key released (went from Down to !Down)?
		public static bool IsKeyReleased(KeyCode key) { return IsKeyReleased(ImGui_TranslateKey(key)); }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool IsMouseDown(int button);                               // is mouse button held?
		public static bool IsMouseButtonDown(MouseButton button) { return IsMouseDown(ImGui_TranslateMouseButton(button)); }
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool IsMouseClicked(int button, bool repeat = false);       // did mouse button clicked? (went from !Down to Down). Same as GetMouseClickedCount() == 1.
		public static bool IsMouseButtonPressed(MouseButton button, bool repeat = false) { return IsMouseClicked(ImGui_TranslateMouseButton(button), repeat); }
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool IsMouseReleased(int button);                           // did mouse button released? (went from Down to !Down)
		public static bool IsMouseButtonReleased(MouseButton button) { return IsMouseReleased(ImGui_TranslateMouseButton(button)); }
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool IsMouseDoubleClicked(int button);                      // did mouse button double-clicked? Same as GetMouseClickedCount() == 2. (note that a double-click will also report IsMouseClicked() == true)
		public static bool IsMouseDoubleClicked(MouseButton button) { return IsMouseDoubleClicked(ImGui_TranslateMouseButton(button)); }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void GetMousePos(out Vector2 pos);
		public static Vector2 GetMousePos() { GetMousePos(out Vector2 pos); return pos; }

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool IsMouseDragging(int button, float lock_threshold);
		public static bool IsMouseDragging(MouseButton button, float lock_threshold = -1.0f) { return IsMouseDragging(ImGui_TranslateMouseButton(button), lock_threshold); }
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void GetMouseDragDelta_(int button, float lock_threshold, out Vector2 delta);
		public static Vector2 GetMouseDragDelta(MouseButton button = MouseButton.Left, float lock_threshold = -1.0f) { GetMouseDragDelta_(ImGui_TranslateMouseButton(button), lock_threshold, out Vector2 delta); return delta; }
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern float GetMouseScroll();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern int ImGui_TranslateKey(KeyCode key);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern int ImGui_TranslateMouseButton(MouseButton button);
	}
}
