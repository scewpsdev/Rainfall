using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static unsafe partial class EditorUI
{
	const int NODE_PANEL_WIDTH = 350;
	const int PROPERTIES_PANEL_WIDTH = 350;

	public static Vector2 currentViewportSize { get; private set; }

	public static EditorInstance nextSelectedTab = null;
	public static EditorInstance unsavedChangesPopup = null;

	static GuizmoManipulateOperation currentManipulateOperation = GuizmoManipulateOperation.TRANSLATE;


	static unsafe void MenuBar(RainfallEditor editor)
	{
		if (ImGui.BeginMainMenuBar())
		{
			if (ImGui.BeginMenu("File"))
			{
				if (ImGui.MenuItem("New", "Ctrl+N"))
					editor.newTab();
				if (ImGui.MenuItem("Open", "Ctrl+O"))
					editor.open();
				if (ImGui.MenuItem("Save", "Ctrl+S"))
					editor.save();
				if (ImGui.MenuItem("Save As", "Ctrl+Shift+S"))
					editor.saveAs();
				if (ImGui.MenuItem("Save All", "Ctrl+Shift+Alt+S"))
					editor.saveAll();

				ImGui.EndMenu();
			}
			if (ImGui.BeginMenu("Edit"))
			{
				if (ImGui.MenuItem("Undo", "Ctrl+Z", false, editor.currentTab != null && editor.currentTab.undoStack.Count > 0))
					editor.currentTab?.undo();
				if (ImGui.MenuItem("Redo", "Ctrl+Y", false, editor.currentTab != null && editor.currentTab.redoStack.Count > 0))
					editor.currentTab?.redo();
				if (ImGui.MenuItem("Add Entity", "Shift+A", false, editor.currentTab != null && editor.currentTab.redoStack.Count > 0))
					editor.currentTab?.newEntity();

				ImGui.EndMenu();
			}

			if (ImGui.IsKeyDown(KeyCode.LeftCtrl) && ImGui.IsKeyPressed(KeyCode.KeyN))
				editor.newTab();
			if (ImGui.IsKeyDown(KeyCode.LeftCtrl) && ImGui.IsKeyPressed(KeyCode.KeyO))
				editor.open();
			if (ImGui.IsKeyDown(KeyCode.LeftCtrl) && ImGui.IsKeyPressed(KeyCode.KeyS))
			{
				if (ImGui.IsKeyDown(KeyCode.LeftShift))
				{
					if (ImGui.IsKeyDown(KeyCode.LeftAlt))
						editor.saveAll();
					else
						editor.saveAs();
				}
				else
				{
					editor.save();
				}
			}

			if (editor.currentTab != null)
			{
				if (ImGui.IsKeyDown(KeyCode.LeftCtrl) && ImGui.IsKeyPressed(KeyCode.KeyY))
					editor.currentTab.undo();
				if (ImGui.IsKeyDown(KeyCode.LeftCtrl) && ImGui.IsKeyPressed(KeyCode.KeyZ))
					editor.currentTab.redo();
				if (ImGui.IsKeyDown(KeyCode.LeftShift) && ImGui.IsKeyPressed(KeyCode.KeyA))
					editor.currentTab.newEntity();
			}

			ImGui.EndMainMenuBar();
		}
	}

	static unsafe void TabPane(RainfallEditor editor)
	{
		ImGui.SetNextWindowPos(new Vector2(0, ImGui.GetFrameHeight()));
		ImGui.SetNextWindowSize(new Vector2(Display.width, ImGui.GetFrameHeight()));

		if (ImGui.IsKeyPressed(KeyCode.Tab) && ImGui.IsKeyDown(KeyCode.LeftCtrl))
			nextSelectedTab = editor.getNextTab(editor.currentTab);
		if (ImGui.IsKeyPressed(KeyCode.Tab) && ImGui.IsKeyDown(KeyCode.LeftCtrl) && ImGui.IsKeyDown(KeyCode.LeftShift))
			nextSelectedTab = editor.getPrevTab(editor.currentTab);

		if (ImGui.Begin("tab_pane", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
		{
			ImGui.SetCursorPos(Vector2.Zero);
			// TODO make reorderable
			if (ImGui.BeginTabBar("tab_bar"))
			{
				HashSet<EditorInstance> closedTabs = new HashSet<EditorInstance>();
				for (int i = 0; i < editor.tabs.Count; i++)
				{
					EditorInstance tab = editor.tabs[i];

					bool isOpen = true;
					ImGuiTabItemFlags flags = ImGuiTabItemFlags.NoAssumedClosure;
					if (nextSelectedTab == tab)
					{
						flags |= ImGuiTabItemFlags.SetSelected;
						nextSelectedTab = null;
					}
					if (tab.unsavedChanges)
						flags |= ImGuiTabItemFlags.UnsavedDocument;

					string label = tab.path != null ? tab.filename : "Untitled";
					if (ImGui.BeginTabItem(label + "##tab" + i, &isOpen, flags))
					{
						editor.currentTab = tab;

						if (ImGui.IsKeyPressed(KeyCode.KeyW) && ImGui.IsKeyDown(KeyCode.LeftCtrl) && !closedTabs.Contains(tab))
						{
							if (ImGui.IsKeyDown(KeyCode.LeftShift))
							{
								foreach (EditorInstance otherTab in editor.tabs)
								{
									if (otherTab != tab)
										closedTabs.Add(otherTab);
								}
							}
							else
							{
								closedTabs.Add(tab);
							}
						}

						ImGui.EndTabItem();
					}

					if (!isOpen && !closedTabs.Contains(tab))
						closedTabs.Add(tab);
				}

				foreach (EditorInstance tab in closedTabs)
				{
					editor.closeTab(tab);
				}

				ImGui.EndTabBar();
			}
		}
		ImGui.End();
	}

	static unsafe void Viewport(EditorInstance instance)
	{
		ImGui.SetNextWindowPos(new Vector2(NODE_PANEL_WIDTH, ImGui.GetFrameHeight() * 2));
		ImGui.SetNextWindowSize(new Vector2(Display.width - NODE_PANEL_WIDTH - PROPERTIES_PANEL_WIDTH, Display.height - ImGui.GetFrameHeight() * 2));
		if (ImGui.Begin("viewport_window", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
		{
			Vector2 topLeft = ImGui.GetWindowPos() + ImGui.GetCursorPos();
			Vector2 windowSize = ImGui.GetContentRegionAvail();
			currentViewportSize = windowSize;

			if (instance.frame != null)
				ImGui.Image(instance.frame, windowSize, true, false);

			ImGuizmo.SetRect(topLeft.x, topLeft.y, windowSize.x, windowSize.y);
			ImGuizmo.SetOrthographic(false);
			ImGuizmo.SetDrawlist();

			instance.camera.updateControls();

			Matrix projection = instance.camera.getProjectionMatrix((int)windowSize.x, (int)windowSize.y);
			Matrix view = instance.camera.getViewMatrix();

			//ImGuizmo.DrawGrid(view, projection, Matrix.Identity, 10);
			ImGuizmo.ViewManipulate(ref view, instance.camera.distance, topLeft + new Vector2(windowSize.x - 200, 0), new Vector2(200), 0x00000000);

			if (instance.selectedEntity != 0)
			{
				if (ImGui.IsKeyPressed(KeyCode.KeyT))
					currentManipulateOperation = GuizmoManipulateOperation.TRANSLATE;
				if (ImGui.IsKeyPressed(KeyCode.KeyR))
					currentManipulateOperation = GuizmoManipulateOperation.ROTATE;
				if (ImGui.IsKeyPressed(KeyCode.KeyS))
					currentManipulateOperation = GuizmoManipulateOperation.SCALE;

				Vector3? snap = null;
				if (ImGui.IsKeyDown(KeyCode.LeftCtrl))
				{
					if (currentManipulateOperation == GuizmoManipulateOperation.TRANSLATE)
						snap = new Vector3(0.5f);
					else if (currentManipulateOperation == GuizmoManipulateOperation.ROTATE)
						snap = new Vector3(15, 0.0f, 0.0f);
					else if (currentManipulateOperation == GuizmoManipulateOperation.SCALE)
						snap = new Vector3(1.5f);
				}

				Entity selectedEntity = instance.getSelectedEntity();
				Matrix matrix = selectedEntity.getModelMatrix();
				if (ImGuizmo.Manipulate(view, projection, currentManipulateOperation, GuizmoManipulateMode.LOCAL, ref matrix, null, snap))
				{
					matrix.decompose(out selectedEntity.position, out selectedEntity.rotation, out selectedEntity.scale);
				}
			}
		}
		ImGui.End();
	}

	public static void Draw(RainfallEditor editor)
	{
		MenuBar(editor);
		TabPane(editor);
		if (editor.currentTab != null)
		{
			NodePanel(editor.currentTab);
			PropertiesPanel(editor.currentTab, editor);
			Viewport(editor.currentTab);
		}
		else
		{
			// Clear background since imgui cant do it themselves
			ImGui.SetNextWindowPos(new Vector2(0.0f, ImGui.GetFrameHeight() * 2));
			ImGui.SetNextWindowSize(Display.viewportSize - ImGui.GetCursorPos());
			ImGui.Begin("background_clear", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
			ImGui.End();
		}
		if (unsavedChangesPopup != null)
		{
			byte open = 1;
			ImGui.OpenPopup_Str("popup_unsaved");
			if (ImGui.BeginPopupModal("popup_unsaved", &open, ImGuiWindowFlags.NoResize))
			{
				if (unsavedChangesPopup.path != null)
					ImGui.TextUnformatted("Save changes to \"" + unsavedChangesPopup.filename + "\"?");
				else
					ImGui.TextUnformatted("Save changes to \"Untitled\"?");
				if (ImGui.Button("Yes"))
				{
					editor.save(unsavedChangesPopup);

					editor.closeTab(unsavedChangesPopup);
					unsavedChangesPopup = null;
				}
				ImGui.SameLine();
				if (ImGui.Button("No"))
				{
					unsavedChangesPopup.notifySave();
					editor.closeTab(unsavedChangesPopup);
					unsavedChangesPopup = null;
				}
				ImGui.EndPopup();
			}
		}
	}
}
