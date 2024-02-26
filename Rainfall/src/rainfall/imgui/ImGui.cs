using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	/*
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
		ImGuiInputTextFlags_CharsDecimal = 1 << 0,   // Allow 0123456789.+-* /
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
		ImGuiInputTextFlags_CharsScientific = 1 << 17,  // Allow 0123456789.+-* /eE (Scientific notation input)
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
	*/

	public struct ImTexture2D
	{
		public ushort texture;
		public byte flags;
		public byte mip;
	}

	public static unsafe partial class ImGui
	{
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igAcceptDragDropPayload")]
		public static extern ImGuiPayload* AcceptDragDropPayload(string type, ImGuiDragDropFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igAlignTextToFramePadding")]
		public static extern void AlignTextToFramePadding();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igArrowButton")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ArrowButton(string str_id, ImGuiDir dir);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBegin")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool Begin(string name, bool* p_open, ImGuiWindowFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginChild_Str")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginChild_Str(string str_id, Vector2 size, ImGuiChildFlags child_flags, ImGuiWindowFlags window_flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginChild_ID")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginChild_ID(uint id, Vector2 size, ImGuiChildFlags child_flags, ImGuiWindowFlags window_flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginCombo")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginCombo(string label, string preview_value, ImGuiComboFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginDisabled")]
		public static extern void BeginDisabled(bool disabled);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginDragDropSource")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginDragDropSource(ImGuiDragDropFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginDragDropTarget")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginDragDropTarget();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginGroup")]
		public static extern void BeginGroup();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginItemTooltip")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginItemTooltip();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginListBox")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginListBox(string label, Vector2 size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginMainMenuBar")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginMainMenuBar();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginMenu")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginMenu(string label, bool enabled = true);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginMenuBar")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginMenuBar();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginPopup")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginPopup(string str_id, ImGuiWindowFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginPopupContextItem")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginPopupContextItem(string str_id = null, ImGuiPopupFlags popup_flags = ImGuiPopupFlags.MouseButtonDefault);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginPopupContextVoid")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginPopupContextVoid(string str_id, ImGuiPopupFlags popup_flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginPopupContextWindow")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginPopupContextWindow(string str_id = null, ImGuiPopupFlags popup_flags = ImGuiPopupFlags.MouseButtonDefault);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginPopupModal")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginPopupModal(string name, string p_open, ImGuiWindowFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginTabBar")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginTabBar(string str_id, ImGuiTabBarFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginTabItem")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginTabItem(string label, bool* p_open, ImGuiTabItemFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginTable")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginTable(string str_id, int column, ImGuiTableFlags flags, Vector2 outer_size, float inner_width);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBeginTooltip")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool BeginTooltip();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBullet")]
		public static extern void Bullet();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igBulletText")]
		public static extern void BulletText(string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igButton")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool Button(string label, Vector2 size = default);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCalcItemWidth")]
		public static extern float CalcItemWidth();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCalcTextSize")]
		public static extern void CalcTextSize(Vector2* pOut, string text, string text_end, bool hide_text_after_double_hash, float wrap_width);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCheckbox")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool Checkbox(string label, string v);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCheckboxFlags_IntPtr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool CheckboxFlags_IntPtr(string label, int* flags, int flags_value);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCheckboxFlags_UintPtr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool CheckboxFlags_UintPtr(string label, uint* flags, uint flags_value);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCloseCurrentPopup")]
		public static extern void CloseCurrentPopup();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCollapsingHeader_TreeNodeFlags")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool CollapsingHeader_TreeNodeFlags(string label, ImGuiTreeNodeFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCollapsingHeader_BoolPtr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool CollapsingHeader_BoolPtr(string label, string p_visible, ImGuiTreeNodeFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igColorButton")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ColorButton(string desc_id, Vector4 col, ImGuiColorEditFlags flags, Vector2 size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ColorConvertFloat4ToU32(Vector4 @in);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igColorConvertHSVtoRGB")]
		public static extern void ColorConvertHSVtoRGB(float h, float s, float v, float* out_r, float* out_g, float* out_b);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igColorConvertRGBtoHSV")]
		public static extern void ColorConvertRGBtoHSV(float r, float g, float b, float* out_h, float* out_s, float* out_v);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ColorConvertU32ToFloat4(Vector4* pOut, uint @in);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ColorEdit3(string label, Vector3* col, ImGuiColorEditFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ColorEdit4(string label, Vector4* col, ImGuiColorEditFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ColorPicker3(string label, Vector3* col, ImGuiColorEditFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ColorPicker4(string label, Vector4* col, ImGuiColorEditFlags flags, float* ref_col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igColumns")]
		public static extern void Columns(int count, string id, bool border);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCombo_Str_arr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool Combo_Str_arr(string label, int* current_item, byte** items, int items_count, int popup_max_height_in_items);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCombo_Str")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool Combo_Str(string label, int* current_item, string items_separated_by_zeros, int popup_max_height_in_items);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igCreateContext")]
		public static extern IntPtr CreateContext(ImFontAtlas* shared_font_atlas);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDebugCheckVersionAndDataLayout")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DebugCheckVersionAndDataLayout(string version_str, uint sz_io, uint sz_style, uint sz_vec2, uint sz_vec4, uint sz_drawvert, uint sz_drawidx);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDebugFlashStyleColor")]
		public static extern void DebugFlashStyleColor(ImGuiCol idx);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDebugTextEncoding")]
		public static extern void DebugTextEncoding(string text);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDestroyContext")]
		public static extern void DestroyContext(IntPtr ctx);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDestroyPlatformWindows")]
		public static extern void DestroyPlatformWindows();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDockSpace")]
		public static extern uint DockSpace(uint id, Vector2 size, ImGuiDockNodeFlags flags, ImGuiWindowClass* window_class);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDockSpaceOverViewport")]
		public static extern uint DockSpaceOverViewport(ImGuiViewport* viewport, ImGuiDockNodeFlags flags, ImGuiWindowClass* window_class);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDragFloat")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragFloat(string label, float* v, float v_speed, float v_min, float v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragFloat2(string label, Vector2* v, float v_speed, float v_min, float v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragFloat3(string label, Vector3* v, float v_speed, float v_min, float v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragFloat4(string label, Vector4* v, float v_speed, float v_min, float v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragFloatRange2(string label, float* v_current_min, float* v_current_max, float v_speed, float v_min, float v_max, string format, string format_max, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDragInt")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragInt(string label, int* v, float v_speed, int v_min, int v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragInt2(string label, int* v, float v_speed, int v_min, int v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragInt3(string label, int* v, float v_speed, int v_min, int v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragInt4(string label, int* v, float v_speed, int v_min, int v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragIntRange2(string label, int* v_current_min, int* v_current_max, float v_speed, int v_min, int v_max, string format, string format_max, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDragScalar")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragScalar(string label, ImGuiDataType data_type, void* p_data, float v_speed, void* p_min, void* p_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDragScalarN")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DragScalarN(string label, ImGuiDataType data_type, void* p_data, int components, float v_speed, void* p_min, void* p_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igDummy")]
		public static extern void Dummy(Vector2 size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEnd")]
		public static extern void End();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndChild")]
		public static extern void EndChild();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndCombo")]
		public static extern void EndCombo();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndDisabled")]
		public static extern void EndDisabled();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndDragDropSource")]
		public static extern void EndDragDropSource();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndDragDropTarget")]
		public static extern void EndDragDropTarget();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndFrame")]
		public static extern void EndFrame();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndGroup")]
		public static extern void EndGroup();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndListBox")]
		public static extern void EndListBox();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndMainMenuBar")]
		public static extern void EndMainMenuBar();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndMenu")]
		public static extern void EndMenu();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndMenuBar")]
		public static extern void EndMenuBar();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndPopup")]
		public static extern void EndPopup();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndTabBar")]
		public static extern void EndTabBar();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndTabItem")]
		public static extern void EndTabItem();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndTable")]
		public static extern void EndTable();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igEndTooltip")]
		public static extern void EndTooltip();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igFindViewportByID")]
		public static extern ImGuiViewport* FindViewportByID(uint id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igFindViewportByPlatformHandle")]
		public static extern ImGuiViewport* FindViewportByPlatformHandle(void* platform_handle);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetAllocatorFunctions")]
		public static extern void GetAllocatorFunctions(IntPtr* p_alloc_func, IntPtr* p_free_func, void** p_user_data);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetBackgroundDrawList_Nil")]
		public static extern ImDrawList* GetBackgroundDrawList_Nil();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetBackgroundDrawList_ViewportPtr")]
		public static extern ImDrawList* GetBackgroundDrawList_ViewportPtr(ImGuiViewport* viewport);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetClipboardText")]
		public static extern string GetClipboardText();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint GetColorU32_Col(ImGuiCol idx, float alpha_mul);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint GetColorU32_Vec4(Vector4 col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint GetColorU32_U32(uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetColumnIndex")]
		public static extern int GetColumnIndex();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetColumnOffset")]
		public static extern float GetColumnOffset(int column_index);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetColumnsCount")]
		public static extern int GetColumnsCount();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetColumnWidth")]
		public static extern float GetColumnWidth(int column_index);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetContentRegionAvail")]
		public static extern void GetContentRegionAvail(Vector2* pOut);
		public static Vector2 GetContentRegionAvail()
		{
			Vector2 region;
			GetContentRegionAvail(&region);
			return region;
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetContentRegionMax")]
		public static extern void GetContentRegionMax(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetCurrentContext")]
		public static extern IntPtr GetCurrentContext();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetCursorPos")]
		public static extern void GetCursorPos(Vector2* pOut);
		public static Vector2 GetCursorPos()
		{
			Vector2 pos;
			GetCursorPos(&pos);
			return pos;
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetCursorPosX")]
		public static extern float GetCursorPosX();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetCursorPosY")]
		public static extern float GetCursorPosY();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetCursorScreenPos")]
		public static extern void GetCursorScreenPos(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetCursorStartPos")]
		public static extern void GetCursorStartPos(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetDragDropPayload")]
		public static extern ImGuiPayload* GetDragDropPayload();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetDrawData")]
		public static extern ImDrawData* GetDrawData();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetDrawListSharedData")]
		public static extern IntPtr GetDrawListSharedData();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetFont")]
		public static extern ImFont* GetFont();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetFontSize")]
		public static extern float GetFontSize();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetFontTexUvWhitePixel")]
		public static extern void GetFontTexUvWhitePixel(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetForegroundDrawList_Nil")]
		public static extern ImDrawList* GetForegroundDrawList_Nil();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetForegroundDrawList_ViewportPtr")]
		public static extern ImDrawList* GetForegroundDrawList_ViewportPtr(ImGuiViewport* viewport);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetFrameCount")]
		public static extern int GetFrameCount();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetFrameHeight")]
		public static extern float GetFrameHeight();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetFrameHeightWithSpacing")]
		public static extern float GetFrameHeightWithSpacing();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetID_Str")]
		public static extern uint GetID_Str(string str_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetID_StrStr")]
		public static extern uint GetID_StrStr(string str_id_begin, string str_id_end);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetID_Ptr")]
		public static extern uint GetID_Ptr(void* ptr_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetIO")]
		public static extern ImGuiIO* GetIO();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetItemID")]
		public static extern uint GetItemID();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetItemRectMax")]
		public static extern void GetItemRectMax(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetItemRectMin")]
		public static extern void GetItemRectMin(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetItemRectSize")]
		public static extern void GetItemRectSize(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetKeyIndex")]
		public static extern ImGuiKey GetKeyIndex(ImGuiKey key);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetKeyName")]
		public static extern string GetKeyName(ImGuiKey key);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetKeyPressedAmount")]
		public static extern int GetKeyPressedAmount(ImGuiKey key, float repeat_delay, float rate);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetMainViewport")]
		public static extern ImGuiViewport* GetMainViewport();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetMouseClickedCount")]
		public static extern int GetMouseClickedCount(ImGuiMouseButton button);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetMouseCursor")]
		public static extern ImGuiMouseCursor GetMouseCursor();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetMouseDragDelta")]
		public static extern void GetMouseDragDelta(Vector2* pOut, ImGuiMouseButton button, float lock_threshold);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetMousePos")]
		public static extern void GetMousePos(Vector2* pOut);
		public static Vector2 GetMousePos()
		{
			Vector2 pos;
			GetMousePos(&pos);
			return pos;
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetMousePosOnOpeningCurrentPopup")]
		public static extern void GetMousePosOnOpeningCurrentPopup(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetPlatformIO")]
		public static extern ImGuiPlatformIO* GetPlatformIO();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetScrollMaxX")]
		public static extern float GetScrollMaxX();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetScrollMaxY")]
		public static extern float GetScrollMaxY();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetScrollX")]
		public static extern float GetScrollX();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetScrollY")]
		public static extern float GetScrollY();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetStateStorage")]
		public static extern ImGuiStorage* GetStateStorage();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetStyle")]
		public static extern ImGuiStyle* GetStyle();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetStyleColorName")]
		public static extern string GetStyleColorName(ImGuiCol idx);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Vector4* GetStyleColorVec4(ImGuiCol idx);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetTextLineHeight")]
		public static extern float GetTextLineHeight();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetTextLineHeightWithSpacing")]
		public static extern float GetTextLineHeightWithSpacing();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetTime")]
		public static extern double GetTime();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetTreeNodeToLabelSpacing")]
		public static extern float GetTreeNodeToLabelSpacing();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetVersion")]
		public static extern string GetVersion();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowContentRegionMax")]
		public static extern void GetWindowContentRegionMax(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowContentRegionMin")]
		public static extern void GetWindowContentRegionMin(Vector2* pOut);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowDockID")]
		public static extern uint GetWindowDockID();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowDpiScale")]
		public static extern float GetWindowDpiScale();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowDrawList")]
		public static extern ImDrawList* GetWindowDrawList();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowHeight")]
		public static extern float GetWindowHeight();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowPos")]
		public static extern void GetWindowPos(Vector2* pOut);
		public static Vector2 GetWindowPos()
		{
			Vector2 pos;
			GetWindowPos(&pos);
			return pos;
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowSize")]
		public static extern void GetWindowSize(Vector2* pOut);
		public static Vector2 GetWindowSize()
		{
			Vector2 size;
			GetWindowSize(&size);
			return size;
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowViewport")]
		public static extern ImGuiViewport* GetWindowViewport();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igGetWindowWidth")]
		public static extern float GetWindowWidth();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImage")]
		public static extern void Image(ImTexture2D user_texture_id, Vector2 image_size, Vector2 uv0, Vector2 uv1, Vector4 tint_col, Vector4 border_col);
		public static void Image(Texture texture, Vector2 image_size, Vector2 uv0, Vector2 uv1, Vector4 tint_col, Vector4 border_col)
		{
			Image(new ImTexture2D { texture = texture.handle, flags = 0, mip = 0 }, image_size, uv0, uv1, tint_col, border_col);
		}
		public static void Image(Texture texture, Vector2 image_size)
		{
			Image(texture, image_size, new Vector2(0, 0), new Vector2(1, 1), new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 0));
		}
		public static void Image(Texture texture, Vector2 image_size, bool alphaBlending, bool mipmaps)
		{
			Image(new ImTexture2D { texture = texture.handle, flags = (byte)(alphaBlending ? 1 : 0), mip = (byte)(mipmaps ? 1 : 0) }, image_size, new Vector2(0, 0), new Vector2(1, 1), new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 0));
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImageButton")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImageButton(string str_id, IntPtr user_texture_id, Vector2 image_size, Vector2 uv0, Vector2 uv1, Vector4 bg_col, Vector4 tint_col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIndent")]
		public static extern void Indent(float indent_w);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInputDouble")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputDouble(string label, double* v, double step, double step_fast, string format, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInputFloat")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputFloat(string label, float* v, float step, float step_fast, string format, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputFloat2(string label, Vector2* v, string format, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputFloat3(string label, Vector3* v, string format, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputFloat4(string label, Vector4* v, string format, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInputInt")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputInt(string label, int* v, int step, int step_fast, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputInt2(string label, int* v, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputInt3(string label, int* v, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputInt4(string label, int* v, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInputScalar")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputScalar(string label, ImGuiDataType data_type, void* p_data, void* p_step, void* p_step_fast, string format, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInputScalarN")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputScalarN(string label, ImGuiDataType data_type, void* p_data, int components, void* p_step, void* p_step_fast, string format, ImGuiInputTextFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInputText")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputText(string label, byte* buf, ulong buf_size, ImGuiInputTextFlags flags = 0, ImGuiInputTextCallback callback = null, void* user_data = null);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInputText")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputText(string label, string buf, ulong buf_size, ImGuiInputTextFlags flags = 0, ImGuiInputTextCallback callback = null, void* user_data = null);
		public static bool InputText(string label, string buf, ImGuiInputTextFlags flags = 0, ImGuiInputTextCallback callback = null, void* user_data = null)
		{
			return InputText(label, buf, (ulong)buf.Length, flags, callback, user_data);
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInputTextMultiline")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputTextMultiline(string label, string buf, uint buf_size, Vector2 size, ImGuiInputTextFlags flags, ImGuiInputTextCallback callback, void* user_data);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInputTextWithHint")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InputTextWithHint(string label, string hint, string buf, uint buf_size, ImGuiInputTextFlags flags, ImGuiInputTextCallback callback, void* user_data);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igInvisibleButton")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool InvisibleButton(string str_id, Vector2 size, ImGuiButtonFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsAnyItemActive")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsAnyItemActive();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsAnyItemFocused")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsAnyItemFocused();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsAnyItemHovered")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsAnyItemHovered();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsAnyMouseDown")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsAnyMouseDown();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemActivated")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemActivated();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemActive")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemActive();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemClicked")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemClicked(ImGuiMouseButton mouse_button);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemDeactivated")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemDeactivated();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemDeactivatedAfterEdit")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemDeactivatedAfterEdit();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemEdited")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemEdited();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemFocused")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemFocused();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemHovered")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemHovered(ImGuiHoveredFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemToggledOpen")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemToggledOpen();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsItemVisible")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsItemVisible();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsKeyChordPressed_Nil")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsKeyChordPressed_Nil(ImGuiKey key_chord);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsKeyDown_Nil")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsKeyDown_Nil(ImGuiKey key);
		public static bool IsKeyDown(KeyCode key)
		{
			return IsKeyDown_Nil((ImGuiKey)ImGui_TranslateKey(key));
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsKeyPressed_Bool")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsKeyPressed_Bool(ImGuiKey key, bool repeat);
		public static bool IsKeyPressed(KeyCode key, bool repeat = true)
		{
			return IsKeyPressed_Bool((ImGuiKey)ImGui_TranslateKey(key), repeat);
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsKeyReleased_Nil")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsKeyReleased_Nil(ImGuiKey key);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsMouseClicked_Bool")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsMouseClicked_Bool(ImGuiMouseButton button, bool repeat);
		public static bool IsMouseButtonPressed(MouseButton button, bool repeat = false)
		{
			return IsMouseClicked_Bool((ImGuiMouseButton)ImGui_TranslateMouseButton(button), repeat);
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsMouseDoubleClicked_Nil")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsMouseDoubleClicked_Nil(ImGuiMouseButton button);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsMouseDown_Nil")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsMouseDown_Nil(ImGuiMouseButton button);
		public static bool IsMouseButtonDown(MouseButton button)
		{
			return IsMouseDown_Nil((ImGuiMouseButton)ImGui_TranslateMouseButton(button));
		}
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsMouseDragging")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsMouseDragging(ImGuiMouseButton button, float lock_threshold);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsMouseHoveringRect")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsMouseHoveringRect(Vector2 r_min, Vector2 r_max, bool clip);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsMousePosValid")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsMousePosValid(Vector2* mouse_pos);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsMouseReleased_Nil")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsMouseReleased_Nil(ImGuiMouseButton button);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ImGui_GetMouseScroll")]
		public static extern float GetMouseScroll();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsPopupOpen_Str")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsPopupOpen_Str(string str_id, ImGuiPopupFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsRectVisible_Nil")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsRectVisible_Nil(Vector2 size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsRectVisible_Vec2(Vector2 rect_min, Vector2 rect_max);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsWindowAppearing")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsWindowAppearing();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsWindowCollapsed")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsWindowCollapsed();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsWindowDocked")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsWindowDocked();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsWindowFocused")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsWindowFocused(ImGuiFocusedFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igIsWindowHovered")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool IsWindowHovered(ImGuiHoveredFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igLabelText")]
		public static extern void LabelText(string label, string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igListBox_Str_arr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ListBox_Str_arr(string label, int* current_item, byte** items, int items_count, int height_in_items);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igLoadIniSettingsFromDisk")]
		public static extern void LoadIniSettingsFromDisk(string ini_filename);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igLoadIniSettingsFromMemory")]
		public static extern void LoadIniSettingsFromMemory(string ini_data, uint ini_size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igLogButtons")]
		public static extern void LogButtons();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igLogFinish")]
		public static extern void LogFinish();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igLogText")]
		public static extern void LogText(string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igLogToClipboard")]
		public static extern void LogToClipboard(int auto_open_depth);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igLogToFile")]
		public static extern void LogToFile(int auto_open_depth, string filename);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igLogToTTY")]
		public static extern void LogToTTY(int auto_open_depth);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igMemAlloc")]
		public static extern void* MemAlloc(uint size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igMemFree")]
		public static extern void MemFree(void* ptr);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igMenuItem_Bool")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool MenuItem(string label, string shortcut = null, bool selected = false, bool enabled = true);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igMenuItem_BoolPtr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool MenuItem_BoolPtr(string label, string shortcut, string p_selected, bool enabled);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igNewFrame")]
		public static extern void NewFrame();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igNewLine")]
		public static extern void NewLine();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igNextColumn")]
		public static extern void NextColumn();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igOpenPopup_Str")]
		public static extern void OpenPopup_Str(string str_id, ImGuiPopupFlags popup_flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igOpenPopup_ID")]
		public static extern void OpenPopup_ID(uint id, ImGuiPopupFlags popup_flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igOpenPopupOnItemClick")]
		public static extern void OpenPopupOnItemClick(string str_id, ImGuiPopupFlags popup_flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPlotHistogram_FloatPtr")]
		public static extern void PlotHistogram_FloatPtr(string label, float* values, int values_count, int values_offset, string overlay_text, float scale_min, float scale_max, Vector2 graph_size, int stride);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPlotLines_FloatPtr")]
		public static extern void PlotLines_FloatPtr(string label, float* values, int values_count, int values_offset, string overlay_text, float scale_min, float scale_max, Vector2 graph_size, int stride);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPopButtonRepeat")]
		public static extern void PopButtonRepeat();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPopClipRect")]
		public static extern void PopClipRect();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPopFont")]
		public static extern void PopFont();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPopID")]
		public static extern void PopID();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPopItemWidth")]
		public static extern void PopItemWidth();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPopStyleColor")]
		public static extern void PopStyleColor(int count);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPopStyleVar")]
		public static extern void PopStyleVar(int count = 1);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPopTabStop")]
		public static extern void PopTabStop();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPopTextWrapPos")]
		public static extern void PopTextWrapPos();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igProgressBar")]
		public static extern void ProgressBar(float fraction, Vector2 size_arg, string overlay);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushButtonRepeat")]
		public static extern void PushButtonRepeat(bool repeat);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushClipRect")]
		public static extern void PushClipRect(Vector2 clip_rect_min, Vector2 clip_rect_max, bool intersect_with_current_clip_rect);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushFont")]
		public static extern void PushFont(ImFont* font);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushID_Str")]
		public static extern void PushID_Str(string str_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushID_StrStr")]
		public static extern void PushID_StrStr(string str_id_begin, string str_id_end);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushID_Ptr")]
		public static extern void PushID_Ptr(void* ptr_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushID_Int")]
		public static extern void PushID_Int(int int_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushItemWidth")]
		public static extern void PushItemWidth(float item_width);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void PushStyleColor_U32(ImGuiCol idx, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void PushStyleColor_Vec4(ImGuiCol idx, Vector4 col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushStyleVar_Float")]
		public static extern void PushStyleVar_Float(ImGuiStyleVar idx, float val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushStyleVar_Vec2")]
		public static extern void PushStyleVar_Vec2(ImGuiStyleVar idx, Vector2 val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushTabStop")]
		public static extern void PushTabStop(bool tab_stop);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igPushTextWrapPos")]
		public static extern void PushTextWrapPos(float wrap_local_pos_x);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igRadioButton_Bool")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool RadioButton_Bool(string label, bool active);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igRadioButton_IntPtr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool RadioButton_IntPtr(string label, int* v, int v_button);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igRender")]
		public static extern void Render();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igRenderPlatformWindowsDefault")]
		public static extern void RenderPlatformWindowsDefault(void* platform_render_arg, void* renderer_render_arg);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igResetMouseDragDelta")]
		public static extern void ResetMouseDragDelta(ImGuiMouseButton button);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSameLine")]
		public static extern void SameLine(float offset_from_start_x = 0.0f, float spacing = -1.0f);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSaveIniSettingsToDisk")]
		public static extern void SaveIniSettingsToDisk(string ini_filename);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSaveIniSettingsToMemory")]
		public static extern string SaveIniSettingsToMemory(uint* out_ini_size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSelectable_Bool")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool Selectable_Bool(string label, bool selected, ImGuiSelectableFlags flags, Vector2 size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSelectable_BoolPtr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool Selectable_BoolPtr(string label, string p_selected, ImGuiSelectableFlags flags, Vector2 size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSeparator")]
		public static extern void Separator();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSeparatorText")]
		public static extern void SeparatorText(string label);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetAllocatorFunctions")]
		public static extern void SetAllocatorFunctions(IntPtr alloc_func, IntPtr free_func, void* user_data);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetClipboardText")]
		public static extern void SetClipboardText(string text);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetColorEditOptions")]
		public static extern void SetColorEditOptions(ImGuiColorEditFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetColumnOffset")]
		public static extern void SetColumnOffset(int column_index, float offset_x);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetColumnWidth")]
		public static extern void SetColumnWidth(int column_index, float width);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetCurrentContext")]
		public static extern void SetCurrentContext(IntPtr ctx);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetCursorPos")]
		public static extern void SetCursorPos(Vector2 local_pos);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetCursorPosX")]
		public static extern void SetCursorPosX(float local_x);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetCursorPosY")]
		public static extern void SetCursorPosY(float local_y);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetCursorScreenPos")]
		public static extern void SetCursorScreenPos(Vector2 pos);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetDragDropPayload")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SetDragDropPayload(string type, void* data, uint sz, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetItemDefaultFocus")]
		public static extern void SetItemDefaultFocus();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetItemTooltip")]
		public static extern void SetItemTooltip(string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetKeyboardFocusHere")]
		public static extern void SetKeyboardFocusHere(int offset = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetMouseCursor")]
		public static extern void SetMouseCursor(ImGuiMouseCursor cursor_type);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextFrameWantCaptureKeyboard")]
		public static extern void SetNextFrameWantCaptureKeyboard(bool want_capture_keyboard);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextFrameWantCaptureMouse")]
		public static extern void SetNextFrameWantCaptureMouse(bool want_capture_mouse);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextItemAllowOverlap")]
		public static extern void SetNextItemAllowOverlap();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextItemOpen")]
		public static extern void SetNextItemOpen(bool is_open, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextItemWidth")]
		public static extern void SetNextItemWidth(float item_width);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowBgAlpha")]
		public static extern void SetNextWindowBgAlpha(float alpha);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowClass")]
		public static extern void SetNextWindowClass(ImGuiWindowClass* window_class);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowCollapsed")]
		public static extern void SetNextWindowCollapsed(bool collapsed, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowContentSize")]
		public static extern void SetNextWindowContentSize(Vector2 size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowDockID")]
		public static extern void SetNextWindowDockID(uint dock_id, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowFocus")]
		public static extern void SetNextWindowFocus();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowPos")]
		public static extern void SetNextWindowPos(Vector2 pos, ImGuiCond cond = 0, Vector2 pivot = default);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowScroll")]
		public static extern void SetNextWindowScroll(Vector2 scroll);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowSize")]
		public static extern void SetNextWindowSize(Vector2 size, ImGuiCond cond = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowSizeConstraints")]
		public static extern void SetNextWindowSizeConstraints(Vector2 size_min, Vector2 size_max, ImGuiSizeCallback custom_callback, void* custom_callback_data);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetNextWindowViewport")]
		public static extern void SetNextWindowViewport(uint viewport_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetScrollFromPosX_Float")]
		public static extern void SetScrollFromPosX_Float(float local_x, float center_x_ratio);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetScrollFromPosY_Float")]
		public static extern void SetScrollFromPosY_Float(float local_y, float center_y_ratio);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetScrollHereX")]
		public static extern void SetScrollHereX(float center_x_ratio);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetScrollHereY")]
		public static extern void SetScrollHereY(float center_y_ratio);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetScrollX_Float")]
		public static extern void SetScrollX_Float(float scroll_x);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetScrollY_Float")]
		public static extern void SetScrollY_Float(float scroll_y);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetStateStorage")]
		public static extern void SetStateStorage(ImGuiStorage* storage);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetTabItemClosed")]
		public static extern void SetTabItemClosed(string tab_or_docked_window_label);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetTooltip")]
		public static extern void SetTooltip(string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetWindowCollapsed_Bool")]
		public static extern void SetWindowCollapsed_Bool(bool collapsed, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetWindowCollapsed_Str")]
		public static extern void SetWindowCollapsed_Str(string name, bool collapsed, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetWindowFocus_Nil")]
		public static extern void SetWindowFocus_Nil();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetWindowFocus_Str")]
		public static extern void SetWindowFocus_Str(string name);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetWindowFontScale")]
		public static extern void SetWindowFontScale(float scale);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetWindowPos_Vec2(Vector2 pos, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetWindowPos_Str")]
		public static extern void SetWindowPos_Str(string name, Vector2 pos, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetWindowSize_Vec2(Vector2 size, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSetWindowSize_Str")]
		public static extern void SetWindowSize_Str(string name, Vector2 size, ImGuiCond cond);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igShowAboutWindow")]
		public static extern void ShowAboutWindow(string p_open);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igShowDebugLogWindow")]
		public static extern void ShowDebugLogWindow(string p_open);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igShowDemoWindow")]
		public static extern void ShowDemoWindow(string p_open);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igShowFontSelector")]
		public static extern void ShowFontSelector(string label);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igShowIDStackToolWindow")]
		public static extern void ShowIDStackToolWindow(string p_open);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igShowMetricsWindow")]
		public static extern void ShowMetricsWindow(string p_open);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igShowStyleEditor")]
		public static extern void ShowStyleEditor(ImGuiStyle* @ref);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igShowStyleSelector")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ShowStyleSelector(string label);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igShowUserGuide")]
		public static extern void ShowUserGuide();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSliderAngle")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderAngle(string label, float* v_rad, float v_degrees_min, float v_degrees_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSliderFloat")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderFloat(string label, float* v, float v_min, float v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderFloat2(string label, Vector2* v, float v_min, float v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderFloat3(string label, Vector3* v, float v_min, float v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderFloat4(string label, Vector4* v, float v_min, float v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSliderInt")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderInt(string label, int* v, int v_min, int v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderInt2(string label, int* v, int v_min, int v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderInt3(string label, int* v, int v_min, int v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderInt4(string label, int* v, int v_min, int v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSliderScalar")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderScalar(string label, ImGuiDataType data_type, void* p_data, void* p_min, void* p_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSliderScalarN")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SliderScalarN(string label, ImGuiDataType data_type, void* p_data, int components, void* p_min, void* p_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSmallButton")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SmallButton(string label);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igSpacing")]
		public static extern void Spacing();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igStyleColorsClassic")]
		public static extern void StyleColorsClassic(ImGuiStyle* dst);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igStyleColorsDark")]
		public static extern void StyleColorsDark(ImGuiStyle* dst);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igStyleColorsLight")]
		public static extern void StyleColorsLight(ImGuiStyle* dst);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTabItemButton")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TabItemButton(string label, ImGuiTabItemFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableAngledHeadersRow")]
		public static extern void TableAngledHeadersRow();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableGetColumnCount")]
		public static extern int TableGetColumnCount();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableGetColumnFlags")]
		public static extern ImGuiTableColumnFlags TableGetColumnFlags(int column_n);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableGetColumnIndex")]
		public static extern int TableGetColumnIndex();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableGetColumnName_Int")]
		public static extern string TableGetColumnName_Int(int column_n);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableGetRowIndex")]
		public static extern int TableGetRowIndex();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableGetSortSpecs")]
		public static extern ImGuiTableSortSpecs* TableGetSortSpecs();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableHeader")]
		public static extern void TableHeader(string label);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableHeadersRow")]
		public static extern void TableHeadersRow();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableNextColumn")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TableNextColumn();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableNextRow")]
		public static extern void TableNextRow(ImGuiTableRowFlags row_flags, float min_row_height);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableSetBgColor")]
		public static extern void TableSetBgColor(ImGuiTableBgTarget target, uint color, int column_n);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableSetColumnEnabled")]
		public static extern void TableSetColumnEnabled(int column_n, bool v);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableSetColumnIndex")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TableSetColumnIndex(int column_n);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableSetupColumn")]
		public static extern void TableSetupColumn(string label, ImGuiTableColumnFlags flags, float init_width_or_weight, uint user_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTableSetupScrollFreeze")]
		public static extern void TableSetupScrollFreeze(int cols, int rows);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igText")]
		public static extern void Text(string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTextColored")]
		public static extern void TextColored(Vector4 col, string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTextDisabled")]
		public static extern void TextDisabled(string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTextUnformatted")]
		public static extern void TextUnformatted(string text, string text_end = null);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTextWrapped")]
		public static extern void TextWrapped(string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTreeNode_Str")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TreeNode_Str(string label);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTreeNode_StrStr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TreeNode_StrStr(string str_id, string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTreeNode_Ptr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TreeNode_Ptr(void* ptr_id, string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTreeNodeEx_Str")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TreeNodeEx(string label, ImGuiTreeNodeFlags flags = 0);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTreeNodeEx_StrStr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TreeNodeEx_StrStr(string str_id, ImGuiTreeNodeFlags flags, string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTreeNodeEx_Ptr")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TreeNodeEx_Ptr(void* ptr_id, ImGuiTreeNodeFlags flags, string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTreePop")]
		public static extern void TreePop();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTreePush_Str")]
		public static extern void TreePush_Str(string str_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igTreePush_Ptr")]
		public static extern void TreePush_Ptr(void* ptr_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igUnindent")]
		public static extern void Unindent(float indent_w);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igUpdatePlatformWindows")]
		public static extern void UpdatePlatformWindows();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igValue_Bool")]
		public static extern void Value_Bool(string prefix, bool b);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igValue_Int")]
		public static extern void Value_Int(string prefix, int v);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igValue_Uint")]
		public static extern void Value_Uint(string prefix, uint v);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igValue_Float")]
		public static extern void Value_Float(string prefix, float v, string float_format);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igVSliderFloat")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool VSliderFloat(string label, Vector2 size, float* v, float v_min, float v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igVSliderInt")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool VSliderInt(string label, Vector2 size, int* v, int v_min, int v_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igVSliderScalar")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool VSliderScalar(string label, Vector2 size, ImGuiDataType data_type, void* p_data, void* p_min, void* p_max, string format, ImGuiSliderFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImColor_destroy")]
		public static extern void ImColor_destroy(ImColor* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImColor_HSV")]
		public static extern void ImColor_HSV(ImColor* pOut, float h, float s, float v, float a);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImColor_ImColor_Nil")]
		public static extern ImColor* ImColor_ImColor_Nil();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImColor_ImColor_Float")]
		public static extern ImColor* ImColor_ImColor_Float(float r, float g, float b, float a);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern ImColor* ImColor_ImColor_Vec4(Vector4 col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImColor_ImColor_Int")]
		public static extern ImColor* ImColor_ImColor_Int(int r, int g, int b, int a);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern ImColor* ImColor_ImColor_U32(uint rgba);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImColor_SetHSV")]
		public static extern void ImColor_SetHSV(ImColor* self, float h, float s, float v, float a);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawCmd_destroy")]
		public static extern void ImDrawCmd_destroy(ImDrawCmd* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawCmd_GetTexID")]
		public static extern IntPtr ImDrawCmd_GetTexID(ImDrawCmd* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawCmd_ImDrawCmd")]
		public static extern ImDrawCmd* ImDrawCmd_ImDrawCmd();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawData_AddDrawList")]
		public static extern void ImDrawData_AddDrawList(ImDrawData* self, ImDrawList* draw_list);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawData_Clear")]
		public static extern void ImDrawData_Clear(ImDrawData* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawData_DeIndexAllBuffers")]
		public static extern void ImDrawData_DeIndexAllBuffers(ImDrawData* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawData_destroy")]
		public static extern void ImDrawData_destroy(ImDrawData* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawData_ImDrawData")]
		public static extern ImDrawData* ImDrawData_ImDrawData();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawData_ScaleClipRects")]
		public static extern void ImDrawData_ScaleClipRects(ImDrawData* self, Vector2 fb_scale);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__CalcCircleAutoSegmentCount")]
		public static extern int ImDrawList__CalcCircleAutoSegmentCount(ImDrawList* self, float radius);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__ClearFreeMemory")]
		public static extern void ImDrawList__ClearFreeMemory(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__OnChangedClipRect")]
		public static extern void ImDrawList__OnChangedClipRect(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__OnChangedTextureID")]
		public static extern void ImDrawList__OnChangedTextureID(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__OnChangedVtxOffset")]
		public static extern void ImDrawList__OnChangedVtxOffset(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__PathArcToFastEx")]
		public static extern void ImDrawList__PathArcToFastEx(ImDrawList* self, Vector2 center, float radius, int a_min_sample, int a_max_sample, int a_step);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__PathArcToN")]
		public static extern void ImDrawList__PathArcToN(ImDrawList* self, Vector2 center, float radius, float a_min, float a_max, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__PopUnusedDrawCmd")]
		public static extern void ImDrawList__PopUnusedDrawCmd(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__ResetForNewFrame")]
		public static extern void ImDrawList__ResetForNewFrame(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList__TryMergeDrawCmds")]
		public static extern void ImDrawList__TryMergeDrawCmds(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddBezierCubic")]
		public static extern void ImDrawList_AddBezierCubic(ImDrawList* self, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, uint col, float thickness, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddBezierQuadratic")]
		public static extern void ImDrawList_AddBezierQuadratic(ImDrawList* self, Vector2 p1, Vector2 p2, Vector2 p3, uint col, float thickness, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddCallback")]
		public static extern void ImDrawList_AddCallback(ImDrawList* self, IntPtr callback, void* callback_data);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddCircle")]
		public static extern void ImDrawList_AddCircle(ImDrawList* self, Vector2 center, float radius, uint col, int num_segments, float thickness);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddCircleFilled")]
		public static extern void ImDrawList_AddCircleFilled(ImDrawList* self, Vector2 center, float radius, uint col, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddConvexPolyFilled")]
		public static extern void ImDrawList_AddConvexPolyFilled(ImDrawList* self, Vector2* points, int num_points, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddDrawCmd")]
		public static extern void ImDrawList_AddDrawCmd(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddEllipse")]
		public static extern void ImDrawList_AddEllipse(ImDrawList* self, Vector2 center, float radius_x, float radius_y, uint col, float rot, int num_segments, float thickness);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddEllipseFilled")]
		public static extern void ImDrawList_AddEllipseFilled(ImDrawList* self, Vector2 center, float radius_x, float radius_y, uint col, float rot, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddImage")]
		public static extern void ImDrawList_AddImage(ImDrawList* self, IntPtr user_texture_id, Vector2 p_min, Vector2 p_max, Vector2 uv_min, Vector2 uv_max, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddImageQuad")]
		public static extern void ImDrawList_AddImageQuad(ImDrawList* self, IntPtr user_texture_id, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddImageRounded")]
		public static extern void ImDrawList_AddImageRounded(ImDrawList* self, IntPtr user_texture_id, Vector2 p_min, Vector2 p_max, Vector2 uv_min, Vector2 uv_max, uint col, float rounding, ImDrawFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddLine")]
		public static extern void ImDrawList_AddLine(ImDrawList* self, Vector2 p1, Vector2 p2, uint col, float thickness);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddNgon")]
		public static extern void ImDrawList_AddNgon(ImDrawList* self, Vector2 center, float radius, uint col, int num_segments, float thickness);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddNgonFilled")]
		public static extern void ImDrawList_AddNgonFilled(ImDrawList* self, Vector2 center, float radius, uint col, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddPolyline")]
		public static extern void ImDrawList_AddPolyline(ImDrawList* self, Vector2* points, int num_points, uint col, ImDrawFlags flags, float thickness);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddQuad")]
		public static extern void ImDrawList_AddQuad(ImDrawList* self, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, uint col, float thickness);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddQuadFilled")]
		public static extern void ImDrawList_AddQuadFilled(ImDrawList* self, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddRect")]
		public static extern void ImDrawList_AddRect(ImDrawList* self, Vector2 p_min, Vector2 p_max, uint col, float rounding, ImDrawFlags flags, float thickness);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddRectFilled")]
		public static extern void ImDrawList_AddRectFilled(ImDrawList* self, Vector2 p_min, Vector2 p_max, uint col, float rounding, ImDrawFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddRectFilledMultiColor")]
		public static extern void ImDrawList_AddRectFilledMultiColor(ImDrawList* self, Vector2 p_min, Vector2 p_max, uint col_upr_left, uint col_upr_right, uint col_bot_right, uint col_bot_left);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ImDrawList_AddText_Vec2(ImDrawList* self, Vector2 pos, uint col, string text_begin, string text_end);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddText_FontPtr")]
		public static extern void ImDrawList_AddText_FontPtr(ImDrawList* self, ImFont* font, float font_size, Vector2 pos, uint col, string text_begin, string text_end, float wrap_width, Vector4* cpu_fine_clip_rect);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddTriangle")]
		public static extern void ImDrawList_AddTriangle(ImDrawList* self, Vector2 p1, Vector2 p2, Vector2 p3, uint col, float thickness);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_AddTriangleFilled")]
		public static extern void ImDrawList_AddTriangleFilled(ImDrawList* self, Vector2 p1, Vector2 p2, Vector2 p3, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_ChannelsMerge")]
		public static extern void ImDrawList_ChannelsMerge(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_ChannelsSetCurrent")]
		public static extern void ImDrawList_ChannelsSetCurrent(ImDrawList* self, int n);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_ChannelsSplit")]
		public static extern void ImDrawList_ChannelsSplit(ImDrawList* self, int count);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_CloneOutput")]
		public static extern ImDrawList* ImDrawList_CloneOutput(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_destroy")]
		public static extern void ImDrawList_destroy(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_GetClipRectMax")]
		public static extern void ImDrawList_GetClipRectMax(Vector2* pOut, ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_GetClipRectMin")]
		public static extern void ImDrawList_GetClipRectMin(Vector2* pOut, ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_ImDrawList")]
		public static extern ImDrawList* ImDrawList_ImDrawList(IntPtr shared_data);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathArcTo")]
		public static extern void ImDrawList_PathArcTo(ImDrawList* self, Vector2 center, float radius, float a_min, float a_max, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathArcToFast")]
		public static extern void ImDrawList_PathArcToFast(ImDrawList* self, Vector2 center, float radius, int a_min_of_12, int a_max_of_12);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathBezierCubicCurveTo")]
		public static extern void ImDrawList_PathBezierCubicCurveTo(ImDrawList* self, Vector2 p2, Vector2 p3, Vector2 p4, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathBezierQuadraticCurveTo")]
		public static extern void ImDrawList_PathBezierQuadraticCurveTo(ImDrawList* self, Vector2 p2, Vector2 p3, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathClear")]
		public static extern void ImDrawList_PathClear(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathEllipticalArcTo")]
		public static extern void ImDrawList_PathEllipticalArcTo(ImDrawList* self, Vector2 center, float radius_x, float radius_y, float rot, float a_min, float a_max, int num_segments);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathFillConvex")]
		public static extern void ImDrawList_PathFillConvex(ImDrawList* self, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathLineTo")]
		public static extern void ImDrawList_PathLineTo(ImDrawList* self, Vector2 pos);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathLineToMergeDuplicate")]
		public static extern void ImDrawList_PathLineToMergeDuplicate(ImDrawList* self, Vector2 pos);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathRect")]
		public static extern void ImDrawList_PathRect(ImDrawList* self, Vector2 rect_min, Vector2 rect_max, float rounding, ImDrawFlags flags);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PathStroke")]
		public static extern void ImDrawList_PathStroke(ImDrawList* self, uint col, ImDrawFlags flags, float thickness);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PopClipRect")]
		public static extern void ImDrawList_PopClipRect(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PopTextureID")]
		public static extern void ImDrawList_PopTextureID(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PrimQuadUV")]
		public static extern void ImDrawList_PrimQuadUV(ImDrawList* self, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 uv_a, Vector2 uv_b, Vector2 uv_c, Vector2 uv_d, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PrimRect")]
		public static extern void ImDrawList_PrimRect(ImDrawList* self, Vector2 a, Vector2 b, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PrimRectUV")]
		public static extern void ImDrawList_PrimRectUV(ImDrawList* self, Vector2 a, Vector2 b, Vector2 uv_a, Vector2 uv_b, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PrimReserve")]
		public static extern void ImDrawList_PrimReserve(ImDrawList* self, int idx_count, int vtx_count);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PrimUnreserve")]
		public static extern void ImDrawList_PrimUnreserve(ImDrawList* self, int idx_count, int vtx_count);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PrimVtx")]
		public static extern void ImDrawList_PrimVtx(ImDrawList* self, Vector2 pos, Vector2 uv, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PrimWriteIdx")]
		public static extern void ImDrawList_PrimWriteIdx(ImDrawList* self, ushort idx);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PrimWriteVtx")]
		public static extern void ImDrawList_PrimWriteVtx(ImDrawList* self, Vector2 pos, Vector2 uv, uint col);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PushClipRect")]
		public static extern void ImDrawList_PushClipRect(ImDrawList* self, Vector2 clip_rect_min, Vector2 clip_rect_max, bool intersect_with_current_clip_rect);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PushClipRectFullScreen")]
		public static extern void ImDrawList_PushClipRectFullScreen(ImDrawList* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawList_PushTextureID")]
		public static extern void ImDrawList_PushTextureID(ImDrawList* self, IntPtr texture_id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawListSplitter_Clear")]
		public static extern void ImDrawListSplitter_Clear(ImDrawListSplitter* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawListSplitter_ClearFreeMemory")]
		public static extern void ImDrawListSplitter_ClearFreeMemory(ImDrawListSplitter* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawListSplitter_destroy")]
		public static extern void ImDrawListSplitter_destroy(ImDrawListSplitter* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawListSplitter_ImDrawListSplitter")]
		public static extern ImDrawListSplitter* ImDrawListSplitter_ImDrawListSplitter();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawListSplitter_Merge")]
		public static extern void ImDrawListSplitter_Merge(ImDrawListSplitter* self, ImDrawList* draw_list);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawListSplitter_SetCurrentChannel")]
		public static extern void ImDrawListSplitter_SetCurrentChannel(ImDrawListSplitter* self, ImDrawList* draw_list, int channel_idx);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImDrawListSplitter_Split")]
		public static extern void ImDrawListSplitter_Split(ImDrawListSplitter* self, ImDrawList* draw_list, int count);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_AddGlyph")]
		public static extern void ImFont_AddGlyph(ImFont* self, ImFontConfig* src_cfg, ushort c, float x0, float y0, float x1, float y1, float u0, float v0, float u1, float v1, float advance_x);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_AddRemapChar")]
		public static extern void ImFont_AddRemapChar(ImFont* self, ushort dst, ushort src, bool overwrite_dst);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_BuildLookupTable")]
		public static extern void ImFont_BuildLookupTable(ImFont* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_CalcTextSizeA")]
		public static extern void ImFont_CalcTextSizeA(Vector2* pOut, ImFont* self, float size, float max_width, float wrap_width, string text_begin, string text_end, byte** remaining);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_CalcWordWrapPositionA")]
		public static extern string ImFont_CalcWordWrapPositionA(ImFont* self, float scale, string text, string text_end, float wrap_width);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_ClearOutputData")]
		public static extern void ImFont_ClearOutputData(ImFont* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_destroy")]
		public static extern void ImFont_destroy(ImFont* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_FindGlyph")]
		public static extern ImFontGlyph* ImFont_FindGlyph(ImFont* self, ushort c);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_FindGlyphNoFallback")]
		public static extern ImFontGlyph* ImFont_FindGlyphNoFallback(ImFont* self, ushort c);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_GetCharAdvance")]
		public static extern float ImFont_GetCharAdvance(ImFont* self, ushort c);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_GetDebugName")]
		public static extern string ImFont_GetDebugName(ImFont* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_GrowIndex")]
		public static extern void ImFont_GrowIndex(ImFont* self, int new_size);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_ImFont")]
		public static extern ImFont* ImFont_ImFont();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_IsGlyphRangeUnused")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImFont_IsGlyphRangeUnused(ImFont* self, uint c_begin, uint c_last);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_IsLoaded")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImFont_IsLoaded(ImFont* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_RenderChar")]
		public static extern void ImFont_RenderChar(ImFont* self, ImDrawList* draw_list, float size, Vector2 pos, uint col, ushort c);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_RenderText")]
		public static extern void ImFont_RenderText(ImFont* self, ImDrawList* draw_list, float size, Vector2 pos, uint col, Vector4 clip_rect, string text_begin, string text_end, float wrap_width, bool cpu_fine_clip);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFont_SetGlyphVisible")]
		public static extern void ImFont_SetGlyphVisible(ImFont* self, ushort c, bool visible);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_AddCustomRectFontGlyph")]
		public static extern int ImFontAtlas_AddCustomRectFontGlyph(ImFontAtlas* self, ImFont* font, ushort id, int width, int height, float advance_x, Vector2 offset);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_AddCustomRectRegular")]
		public static extern int ImFontAtlas_AddCustomRectRegular(ImFontAtlas* self, int width, int height);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_AddFont")]
		public static extern ImFont* ImFontAtlas_AddFont(ImFontAtlas* self, ImFontConfig* font_cfg);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_AddFontDefault")]
		public static extern ImFont* ImFontAtlas_AddFontDefault(ImFontAtlas* self, ImFontConfig* font_cfg);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_AddFontFromFileTTF")]
		public static extern ImFont* ImFontAtlas_AddFontFromFileTTF(ImFontAtlas* self, string filename, float size_pixels, ImFontConfig* font_cfg, ushort* glyph_ranges);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern ImFont* ImFontAtlas_AddFontFromMemoryCompressedBase85TTF(ImFontAtlas* self, string compressed_font_data_base85, float size_pixels, ImFontConfig* font_cfg, ushort* glyph_ranges);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_AddFontFromMemoryCompressedTTF")]
		public static extern ImFont* ImFontAtlas_AddFontFromMemoryCompressedTTF(ImFontAtlas* self, void* compressed_font_data, int compressed_font_data_size, float size_pixels, ImFontConfig* font_cfg, ushort* glyph_ranges);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_AddFontFromMemoryTTF")]
		public static extern ImFont* ImFontAtlas_AddFontFromMemoryTTF(ImFontAtlas* self, void* font_data, int font_data_size, float size_pixels, ImFontConfig* font_cfg, ushort* glyph_ranges);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_Build")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImFontAtlas_Build(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_CalcCustomRectUV")]
		public static extern void ImFontAtlas_CalcCustomRectUV(ImFontAtlas* self, ImFontAtlasCustomRect* rect, Vector2* out_uv_min, Vector2* out_uv_max);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_Clear")]
		public static extern void ImFontAtlas_Clear(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_ClearFonts")]
		public static extern void ImFontAtlas_ClearFonts(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_ClearInputData")]
		public static extern void ImFontAtlas_ClearInputData(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_ClearTexData")]
		public static extern void ImFontAtlas_ClearTexData(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_destroy")]
		public static extern void ImFontAtlas_destroy(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetCustomRectByIndex")]
		public static extern ImFontAtlasCustomRect* ImFontAtlas_GetCustomRectByIndex(ImFontAtlas* self, int index);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetGlyphRangesChineseFull")]
		public static extern ushort* ImFontAtlas_GetGlyphRangesChineseFull(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetGlyphRangesChineseSimplifiedCommon")]
		public static extern ushort* ImFontAtlas_GetGlyphRangesChineseSimplifiedCommon(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetGlyphRangesCyrillic")]
		public static extern ushort* ImFontAtlas_GetGlyphRangesCyrillic(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetGlyphRangesDefault")]
		public static extern ushort* ImFontAtlas_GetGlyphRangesDefault(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetGlyphRangesGreek")]
		public static extern ushort* ImFontAtlas_GetGlyphRangesGreek(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetGlyphRangesJapanese")]
		public static extern ushort* ImFontAtlas_GetGlyphRangesJapanese(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetGlyphRangesKorean")]
		public static extern ushort* ImFontAtlas_GetGlyphRangesKorean(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetGlyphRangesThai")]
		public static extern ushort* ImFontAtlas_GetGlyphRangesThai(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetGlyphRangesVietnamese")]
		public static extern ushort* ImFontAtlas_GetGlyphRangesVietnamese(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_GetMouseCursorTexData")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImFontAtlas_GetMouseCursorTexData(ImFontAtlas* self, ImGuiMouseCursor cursor, Vector2* out_offset, Vector2* out_size, Vector2* out_uv_border, Vector2* out_uv_fill);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ImFontAtlas_GetTexDataAsAlpha8(ImFontAtlas* self, byte** out_pixels, int* out_width, int* out_height, int* out_bytes_per_pixel);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ImFontAtlas_GetTexDataAsAlpha8(ImFontAtlas* self, IntPtr* out_pixels, int* out_width, int* out_height, int* out_bytes_per_pixel);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ImFontAtlas_GetTexDataAsRGBA32(ImFontAtlas* self, byte** out_pixels, int* out_width, int* out_height, int* out_bytes_per_pixel);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ImFontAtlas_GetTexDataAsRGBA32(ImFontAtlas* self, IntPtr* out_pixels, int* out_width, int* out_height, int* out_bytes_per_pixel);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_ImFontAtlas")]
		public static extern ImFontAtlas* ImFontAtlas_ImFontAtlas();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_IsBuilt")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImFontAtlas_IsBuilt(ImFontAtlas* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlas_SetTexID")]
		public static extern void ImFontAtlas_SetTexID(ImFontAtlas* self, IntPtr id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlasCustomRect_destroy")]
		public static extern void ImFontAtlasCustomRect_destroy(ImFontAtlasCustomRect* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlasCustomRect_ImFontAtlasCustomRect")]
		public static extern ImFontAtlasCustomRect* ImFontAtlasCustomRect_ImFontAtlasCustomRect();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontAtlasCustomRect_IsPacked")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImFontAtlasCustomRect_IsPacked(ImFontAtlasCustomRect* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontConfig_destroy")]
		public static extern void ImFontConfig_destroy(ImFontConfig* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontConfig_ImFontConfig")]
		public static extern ImFontConfig* ImFontConfig_ImFontConfig();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontGlyphRangesBuilder_AddChar")]
		public static extern void ImFontGlyphRangesBuilder_AddChar(ImFontGlyphRangesBuilder* self, ushort c);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontGlyphRangesBuilder_AddRanges")]
		public static extern void ImFontGlyphRangesBuilder_AddRanges(ImFontGlyphRangesBuilder* self, ushort* ranges);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontGlyphRangesBuilder_AddText")]
		public static extern void ImFontGlyphRangesBuilder_AddText(ImFontGlyphRangesBuilder* self, string text, string text_end);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontGlyphRangesBuilder_BuildRanges")]
		public static extern void ImFontGlyphRangesBuilder_BuildRanges(ImFontGlyphRangesBuilder* self, ImVector* out_ranges);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontGlyphRangesBuilder_Clear")]
		public static extern void ImFontGlyphRangesBuilder_Clear(ImFontGlyphRangesBuilder* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontGlyphRangesBuilder_destroy")]
		public static extern void ImFontGlyphRangesBuilder_destroy(ImFontGlyphRangesBuilder* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontGlyphRangesBuilder_GetBit")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImFontGlyphRangesBuilder_GetBit(ImFontGlyphRangesBuilder* self, uint n);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder")]
		public static extern ImFontGlyphRangesBuilder* ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImFontGlyphRangesBuilder_SetBit")]
		public static extern void ImFontGlyphRangesBuilder_SetBit(ImFontGlyphRangesBuilder* self, uint n);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiInputTextCallbackData_ClearSelection")]
		public static extern void ImGuiInputTextCallbackData_ClearSelection(ImGuiInputTextCallbackData* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiInputTextCallbackData_DeleteChars")]
		public static extern void ImGuiInputTextCallbackData_DeleteChars(ImGuiInputTextCallbackData* self, int pos, int bytes_count);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiInputTextCallbackData_destroy")]
		public static extern void ImGuiInputTextCallbackData_destroy(ImGuiInputTextCallbackData* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiInputTextCallbackData_HasSelection")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiInputTextCallbackData_HasSelection(ImGuiInputTextCallbackData* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiInputTextCallbackData_ImGuiInputTextCallbackData")]
		public static extern ImGuiInputTextCallbackData* ImGuiInputTextCallbackData_ImGuiInputTextCallbackData();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiInputTextCallbackData_InsertChars")]
		public static extern void ImGuiInputTextCallbackData_InsertChars(ImGuiInputTextCallbackData* self, int pos, string text, string text_end);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiInputTextCallbackData_SelectAll")]
		public static extern void ImGuiInputTextCallbackData_SelectAll(ImGuiInputTextCallbackData* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_AddFocusEvent")]
		public static extern void ImGuiIO_AddFocusEvent(ImGuiIO* self, bool focused);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_AddInputCharacter")]
		public static extern void ImGuiIO_AddInputCharacter(ImGuiIO* self, uint c);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ImGuiIO_AddInputCharactersUTF8(ImGuiIO* self, string str);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ImGuiIO_AddInputCharacterUTF16(ImGuiIO* self, ushort c);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_AddKeyAnalogEvent")]
		public static extern void ImGuiIO_AddKeyAnalogEvent(ImGuiIO* self, ImGuiKey key, bool down, float v);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_AddKeyEvent")]
		public static extern void ImGuiIO_AddKeyEvent(ImGuiIO* self, ImGuiKey key, bool down);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_AddMouseButtonEvent")]
		public static extern void ImGuiIO_AddMouseButtonEvent(ImGuiIO* self, int button, bool down);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_AddMousePosEvent")]
		public static extern void ImGuiIO_AddMousePosEvent(ImGuiIO* self, float x, float y);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_AddMouseSourceEvent")]
		public static extern void ImGuiIO_AddMouseSourceEvent(ImGuiIO* self, ImGuiMouseSource source);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_AddMouseViewportEvent")]
		public static extern void ImGuiIO_AddMouseViewportEvent(ImGuiIO* self, uint id);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_AddMouseWheelEvent")]
		public static extern void ImGuiIO_AddMouseWheelEvent(ImGuiIO* self, float wheel_x, float wheel_y);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_ClearEventsQueue")]
		public static extern void ImGuiIO_ClearEventsQueue(ImGuiIO* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_ClearInputKeys")]
		public static extern void ImGuiIO_ClearInputKeys(ImGuiIO* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_destroy")]
		public static extern void ImGuiIO_destroy(ImGuiIO* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_ImGuiIO")]
		public static extern ImGuiIO* ImGuiIO_ImGuiIO();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_SetAppAcceptingEvents")]
		public static extern void ImGuiIO_SetAppAcceptingEvents(ImGuiIO* self, bool accepting_events);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiIO_SetKeyEventNativeData")]
		public static extern void ImGuiIO_SetKeyEventNativeData(ImGuiIO* self, ImGuiKey key, int native_keycode, int native_scancode, int native_legacy_index);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiListClipper_Begin")]
		public static extern void ImGuiListClipper_Begin(ImGuiListClipper* self, int items_count, float items_height);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiListClipper_destroy")]
		public static extern void ImGuiListClipper_destroy(ImGuiListClipper* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiListClipper_End")]
		public static extern void ImGuiListClipper_End(ImGuiListClipper* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiListClipper_ImGuiListClipper")]
		public static extern ImGuiListClipper* ImGuiListClipper_ImGuiListClipper();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiListClipper_IncludeItemByIndex")]
		public static extern void ImGuiListClipper_IncludeItemByIndex(ImGuiListClipper* self, int item_index);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiListClipper_IncludeItemsByIndex")]
		public static extern void ImGuiListClipper_IncludeItemsByIndex(ImGuiListClipper* self, int item_begin, int item_end);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiListClipper_Step")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiListClipper_Step(ImGuiListClipper* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiOnceUponAFrame_destroy")]
		public static extern void ImGuiOnceUponAFrame_destroy(ImGuiOnceUponAFrame* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiOnceUponAFrame_ImGuiOnceUponAFrame")]
		public static extern ImGuiOnceUponAFrame* ImGuiOnceUponAFrame_ImGuiOnceUponAFrame();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPayload_Clear")]
		public static extern void ImGuiPayload_Clear(ImGuiPayload* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPayload_destroy")]
		public static extern void ImGuiPayload_destroy(ImGuiPayload* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPayload_ImGuiPayload")]
		public static extern ImGuiPayload* ImGuiPayload_ImGuiPayload();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPayload_IsDataType")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiPayload_IsDataType(ImGuiPayload* self, string type);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPayload_IsDelivery")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiPayload_IsDelivery(ImGuiPayload* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPayload_IsPreview")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiPayload_IsPreview(ImGuiPayload* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPlatformImeData_destroy")]
		public static extern void ImGuiPlatformImeData_destroy(ImGuiPlatformImeData* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPlatformImeData_ImGuiPlatformImeData")]
		public static extern ImGuiPlatformImeData* ImGuiPlatformImeData_ImGuiPlatformImeData();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPlatformIO_destroy")]
		public static extern void ImGuiPlatformIO_destroy(ImGuiPlatformIO* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPlatformIO_ImGuiPlatformIO")]
		public static extern ImGuiPlatformIO* ImGuiPlatformIO_ImGuiPlatformIO();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPlatformMonitor_destroy")]
		public static extern void ImGuiPlatformMonitor_destroy(ImGuiPlatformMonitor* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiPlatformMonitor_ImGuiPlatformMonitor")]
		public static extern ImGuiPlatformMonitor* ImGuiPlatformMonitor_ImGuiPlatformMonitor();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_BuildSortByKey")]
		public static extern void ImGuiStorage_BuildSortByKey(ImGuiStorage* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_Clear")]
		public static extern void ImGuiStorage_Clear(ImGuiStorage* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_GetBool")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiStorage_GetBool(ImGuiStorage* self, uint key, bool default_val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_GetBoolRef")]
		public static extern string ImGuiStorage_GetBoolRef(ImGuiStorage* self, uint key, bool default_val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_GetFloat")]
		public static extern float ImGuiStorage_GetFloat(ImGuiStorage* self, uint key, float default_val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_GetFloatRef")]
		public static extern float* ImGuiStorage_GetFloatRef(ImGuiStorage* self, uint key, float default_val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_GetInt")]
		public static extern int ImGuiStorage_GetInt(ImGuiStorage* self, uint key, int default_val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_GetIntRef")]
		public static extern int* ImGuiStorage_GetIntRef(ImGuiStorage* self, uint key, int default_val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_GetVoidPtr")]
		public static extern void* ImGuiStorage_GetVoidPtr(ImGuiStorage* self, uint key);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void** ImGuiStorage_GetVoidPtrRef(ImGuiStorage* self, uint key, void* default_val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_SetAllInt")]
		public static extern void ImGuiStorage_SetAllInt(ImGuiStorage* self, int val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_SetBool")]
		public static extern void ImGuiStorage_SetBool(ImGuiStorage* self, uint key, bool val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_SetFloat")]
		public static extern void ImGuiStorage_SetFloat(ImGuiStorage* self, uint key, float val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_SetInt")]
		public static extern void ImGuiStorage_SetInt(ImGuiStorage* self, uint key, int val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStorage_SetVoidPtr")]
		public static extern void ImGuiStorage_SetVoidPtr(ImGuiStorage* self, uint key, void* val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStoragePair_destroy")]
		public static extern void ImGuiStoragePair_destroy(ImGuiStoragePair* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStoragePair_ImGuiStoragePair_Int")]
		public static extern ImGuiStoragePair* ImGuiStoragePair_ImGuiStoragePair_Int(uint _key, int _val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStoragePair_ImGuiStoragePair_Float")]
		public static extern ImGuiStoragePair* ImGuiStoragePair_ImGuiStoragePair_Float(uint _key, float _val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStoragePair_ImGuiStoragePair_Ptr")]
		public static extern ImGuiStoragePair* ImGuiStoragePair_ImGuiStoragePair_Ptr(uint _key, void* _val);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStyle_destroy")]
		public static extern void ImGuiStyle_destroy(ImGuiStyle* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStyle_ImGuiStyle")]
		public static extern ImGuiStyle* ImGuiStyle_ImGuiStyle();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiStyle_ScaleAllSizes")]
		public static extern void ImGuiStyle_ScaleAllSizes(ImGuiStyle* self, float scale_factor);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTableColumnSortSpecs_destroy")]
		public static extern void ImGuiTableColumnSortSpecs_destroy(ImGuiTableColumnSortSpecs* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTableColumnSortSpecs_ImGuiTableColumnSortSpecs")]
		public static extern ImGuiTableColumnSortSpecs* ImGuiTableColumnSortSpecs_ImGuiTableColumnSortSpecs();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTableSortSpecs_destroy")]
		public static extern void ImGuiTableSortSpecs_destroy(ImGuiTableSortSpecs* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTableSortSpecs_ImGuiTableSortSpecs")]
		public static extern ImGuiTableSortSpecs* ImGuiTableSortSpecs_ImGuiTableSortSpecs();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_append")]
		public static extern void ImGuiTextBuffer_append(ImGuiTextBuffer* self, string str, string str_end);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_appendf")]
		public static extern void ImGuiTextBuffer_appendf(ImGuiTextBuffer* self, string fmt);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_begin")]
		public static extern string ImGuiTextBuffer_begin(ImGuiTextBuffer* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_c_str")]
		public static extern string ImGuiTextBuffer_c_str(ImGuiTextBuffer* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_clear")]
		public static extern void ImGuiTextBuffer_clear(ImGuiTextBuffer* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_destroy")]
		public static extern void ImGuiTextBuffer_destroy(ImGuiTextBuffer* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_empty")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiTextBuffer_empty(ImGuiTextBuffer* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_end")]
		public static extern string ImGuiTextBuffer_end(ImGuiTextBuffer* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_ImGuiTextBuffer")]
		public static extern ImGuiTextBuffer* ImGuiTextBuffer_ImGuiTextBuffer();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_reserve")]
		public static extern void ImGuiTextBuffer_reserve(ImGuiTextBuffer* self, int capacity);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextBuffer_size")]
		public static extern int ImGuiTextBuffer_size(ImGuiTextBuffer* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextFilter_Build")]
		public static extern void ImGuiTextFilter_Build(ImGuiTextFilter* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextFilter_Clear")]
		public static extern void ImGuiTextFilter_Clear(ImGuiTextFilter* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextFilter_destroy")]
		public static extern void ImGuiTextFilter_destroy(ImGuiTextFilter* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextFilter_Draw")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiTextFilter_Draw(ImGuiTextFilter* self, string label, float width);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextFilter_ImGuiTextFilter")]
		public static extern ImGuiTextFilter* ImGuiTextFilter_ImGuiTextFilter(string default_filter);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextFilter_IsActive")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiTextFilter_IsActive(ImGuiTextFilter* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextFilter_PassFilter")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiTextFilter_PassFilter(ImGuiTextFilter* self, string text, string text_end);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextRange_destroy")]
		public static extern void ImGuiTextRange_destroy(ImGuiTextRange* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextRange_empty")]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGuiTextRange_empty(ImGuiTextRange* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextRange_ImGuiTextRange_Nil")]
		public static extern ImGuiTextRange* ImGuiTextRange_ImGuiTextRange_Nil();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextRange_ImGuiTextRange_Str")]
		public static extern ImGuiTextRange* ImGuiTextRange_ImGuiTextRange_Str(string _b, string _e);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiTextRange_split")]
		public static extern void ImGuiTextRange_split(ImGuiTextRange* self, bool separator, ImVector* @out);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiViewport_destroy")]
		public static extern void ImGuiViewport_destroy(ImGuiViewport* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiViewport_GetCenter")]
		public static extern void ImGuiViewport_GetCenter(Vector2* pOut, ImGuiViewport* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiViewport_GetWorkCenter")]
		public static extern void ImGuiViewport_GetWorkCenter(Vector2* pOut, ImGuiViewport* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiViewport_ImGuiViewport")]
		public static extern ImGuiViewport* ImGuiViewport_ImGuiViewport();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiWindowClass_destroy")]
		public static extern void ImGuiWindowClass_destroy(ImGuiWindowClass* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "igImGuiWindowClass_ImGuiWindowClass")]
		public static extern ImGuiWindowClass* ImGuiWindowClass_ImGuiWindowClass();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ImVec2_destroy(Vector2* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Vector2* ImVec2_ImVec2_Nil();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Vector2* ImVec2_ImVec2_Float(float _x, float _y);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ImVec4_destroy(Vector4* self);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Vector4* ImVec4_ImVec4_Nil();
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Vector4* ImVec4_ImVec4_Float(float _x, float _y, float _z, float _w);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern int ImGui_TranslateKey(KeyCode key);
		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern int ImGui_TranslateMouseButton(MouseButton button);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool ImGui_Tab(string label, Vector2 size_arg, ImGuiButtonFlags flags);
	}
}
