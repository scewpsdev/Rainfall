using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static unsafe partial class EditorUI
{
	const int NODE_PANEL_WIDTH = 350;
	const int PROPERTIES_PANEL_WIDTH = 400;

	public static Vector2 currentViewportSize { get; private set; }

	public static EditorInstance nextSelectedTab = null;
	public static List<EditorInstance> unsavedChangesPopup = new List<EditorInstance>();

	static GuizmoManipulateOperation currentManipulateOperation = GuizmoManipulateOperation.TRANSLATE;
	static bool manipulateEdited = false;


	public static unsafe bool DragFloat(EditorInstance instance, string label, string sid, ref float f, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, ImGuiSliderFlags flags = 0)
	{
		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		float newValue = f;
		bool changed = ImGui.DragFloat("##" + sid, &newValue, v_speed, v_min, v_max, "%.3f", flags);
		if (changed)
			f = newValue;
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
		return changed;
	}

	public static unsafe bool DragAngle(EditorInstance instance, string label, string sid, ref float f, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, ImGuiSliderFlags flags = 0)
	{
		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		float newValue = f * 180 / MathF.PI;
		bool changed = ImGui.DragFloat("##" + sid, &newValue, v_speed, v_min, v_max, "%.3f", flags);
		if (changed)
			f = newValue / 180 * MathF.PI;
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
		return changed;
	}

	public static unsafe bool DragFloat3(EditorInstance instance, string label, string sid, ref Vector3 v, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, ImGuiSliderFlags flags = 0)
	{
		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector3 newValue = v;
		bool changed = ImGui.DragFloat3("##" + sid, &newValue, v_speed, v_min, v_max, "%.3f", flags);
		if (changed)
			v = newValue;
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
		return changed;
	}

	public static unsafe bool DragFloat3Rotation(EditorInstance instance, string label, string sid, ref Quaternion q, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, ImGuiSliderFlags flags = 0)
	{
		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector3 newValue = q.eulers * 180 / MathF.PI;
		bool changed = ImGui.DragFloat3("##" + sid, &newValue, v_speed, v_min, v_max, "%.3f", flags);
		if (changed)
			q = Quaternion.FromEulerAngles(newValue / 180 * MathF.PI);
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
		return changed;
	}

	public static unsafe bool DragFloat3Eulers(EditorInstance instance, string label, string sid, ref Vector3 v, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, ImGuiSliderFlags flags = 0)
	{
		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector3 newValue = v * 180 / MathF.PI;
		bool changed = ImGui.DragFloat3("##" + sid, &newValue, v_speed, v_min, v_max, "%.3f", flags);
		if (changed)
			v = newValue / 180 * MathF.PI;
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
		return changed;
	}

	public static unsafe bool DragInt(EditorInstance instance, string label, string sid, ref int i, float v_speed = 1.0f, int v_min = 0, int v_max = 0, ImGuiSliderFlags flags = 0)
	{
		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		int newValue = i;
		bool changed = ImGui.DragInt("##" + sid, &newValue, v_speed, v_min, v_max, "%d", flags);
		if (changed)
			i = newValue;
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
		return changed;
	}

	public static unsafe bool DragInt2(EditorInstance instance, string label, string sid, ref Vector2i v, float v_speed = 1.0f, int v_min = 0, int v_max = 0, ImGuiSliderFlags flags = 0)
	{
		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector2i newValue = v;
		bool changed = ImGui.DragInt2("##" + sid, &newValue, v_speed, v_min, v_max, "%d", flags);
		if (changed)
			v = newValue;
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
		return changed;
	}

	public static unsafe bool ColorEdit3(EditorInstance instance, string label, string sid, ref Vector3 v, bool hdr = false)
	{
		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector3 newValue = v;
		bool changed = ImGui.ColorEdit3("##" + sid, &newValue, ImGuiColorEditFlags.NoInputs | (hdr ? ImGuiColorEditFlags.HDR : 0));
		if (changed)
			v = newValue;
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
		return changed;
	}

	public static unsafe bool ColorEdit4(EditorInstance instance, string label, string sid, ref Vector4 v, bool hdr = false)
	{
		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector4 newValue = v;
		bool changed = ImGui.ColorEdit4("##" + sid, &newValue, ImGuiColorEditFlags.NoInputs | (hdr ? ImGuiColorEditFlags.HDR : 0));
		if (changed)
			v = newValue;
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
		return changed;
	}

	public static unsafe void Combo<T>(EditorInstance instance, string label, string sid, ref T v, ImGuiComboFlags flags = 0) where T : struct, Enum
	{
		ImGui.TextUnformatted(label);
		ImGui.SameLine(SPACING_X);
		ImGui.SetNextItemWidth(ITEM_WIDTH);
		if (ImGui.BeginCombo("##" + sid, v.ToString(), flags))
		{
			T newValue = v;

			foreach (T t in Enum.GetValues<T>())
			{
				if (ImGui.Selectable_Bool(t.ToString()))
					newValue = t;
			}

			if (!newValue.Equals(v))
			{
				v = newValue;
				instance.notifyEdit();
			}

			ImGui.EndCombo();
		}
	}

	public static unsafe bool Checkbox(EditorInstance instance, string label, string sid, ref bool v)
	{
		ImGui.TextUnformatted(label);
		ImGui.SameLine(SPACING_X);
		ImGui.SetNextItemWidth(ITEM_WIDTH);
		byte newValue = (byte)(v ? 1 : 0);
		if (ImGui.Checkbox("##" + sid, &newValue))
		{
			v = newValue != 0;
			instance.notifyEdit();
			return true;
		}
		return false;
	}

	public static unsafe bool TreeNodeOptional(EditorInstance instance, string label, string sid, ref bool enabled)
	{
		ImGui.SetNextItemAllowOverlap();
		bool openSettings = ImGui.TreeNodeEx(label + "##" + sid + "_settings", ImGuiTreeNodeFlags.SpanAvailWidth);
		ImGui.SameLine(SPACING_X);
		byte newEnabled = (byte)(enabled ? 1 : 0);
		ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, Vector2.Zero);
		if (ImGui.Checkbox("##" + sid, &newEnabled))
		{
			enabled = newEnabled != 0;
			instance.notifyEdit();
		}
		ImGui.PopStyleVar();
		return openSettings;
	}

	public static unsafe bool TreeNodeRemovable(EditorInstance instance, string label, string sid, out bool removed)
	{
		Vector2 topRight = ImGui.GetCursorPos();
		ImGui.SetNextItemAllowOverlap();
		bool openSettings = ImGui.TreeNodeEx(label + "##" + sid + "_settings");
		ImGui.SameLine(SPACING_X);
		ImGui.SetCursorPos(new Vector2(PROPERTIES_PANEL_WIDTH - RIGHT_PADDING, topRight.y));
		if (ImGui.SmallButton("X##" + sid + "_close"))
		{
			instance.notifyEdit();
			removed = true;
			return openSettings;
		}
		removed = false;
		return openSettings;
	}

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

			if (ImGui.IsKeyDown(KeyCode.Ctrl) && ImGui.IsKeyPressed(KeyCode.N))
				editor.newTab();
			if (ImGui.IsKeyDown(KeyCode.Ctrl) && ImGui.IsKeyPressed(KeyCode.O))
				editor.open();
			if (ImGui.IsKeyDown(KeyCode.Ctrl) && ImGui.IsKeyPressed(KeyCode.S))
			{
				if (ImGui.IsKeyDown(KeyCode.Shift))
				{
					if (ImGui.IsKeyDown(KeyCode.Alt))
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
				if (ImGui.IsKeyDown(KeyCode.Ctrl) && ImGui.IsKeyPressed(KeyCode.Y))
					editor.currentTab.undo();
				if (ImGui.IsKeyDown(KeyCode.Ctrl) && ImGui.IsKeyPressed(KeyCode.Z))
					editor.currentTab.redo();
				if (ImGui.IsKeyDown(KeyCode.Shift) && ImGui.IsKeyPressed(KeyCode.A))
					editor.currentTab.newEntity();
			}

			ImGui.EndMainMenuBar();
		}
	}

	static unsafe void TabPane(RainfallEditor editor)
	{
		ImGui.SetNextWindowPos(new Vector2(0, ImGui.GetFrameHeight()));
		ImGui.SetNextWindowSize(new Vector2(Display.width, ImGui.GetFrameHeight()));

		if (ImGui.IsKeyPressed(KeyCode.Tab) && ImGui.IsKeyDown(KeyCode.Ctrl))
			nextSelectedTab = editor.getNextTab(editor.currentTab);
		if (ImGui.IsKeyPressed(KeyCode.Tab) && ImGui.IsKeyDown(KeyCode.Ctrl) && ImGui.IsKeyDown(KeyCode.Shift))
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

						if (ImGui.IsKeyPressed(KeyCode.W) && ImGui.IsKeyDown(KeyCode.Ctrl) && !closedTabs.Contains(tab))
						{
							if (ImGui.IsKeyDown(KeyCode.Shift))
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

			instance.camera.updateControls(instance);

			Matrix projection = instance.camera.getProjectionMatrix((int)windowSize.x, (int)windowSize.y);
			Matrix view = instance.camera.getViewMatrix();

			//ImGuizmo.DrawGrid(view, projection, Matrix.Identity, 10);
			Matrix newView = view;
			ImGuizmo.ViewManipulate(ref newView, instance.camera.distance, topLeft + new Vector2(windowSize.x - 200, 0), new Vector2(200), 0x00000000);
			if (newView != view)
			{
				view = newView;
				instance.notifyEdit();
			}

			if (instance.selectedEntity != 0)
			{
				if (ImGui.IsWindowHovered())
				{
					if (!ImGui.IsAnyItemActive() && !ImGui.IsKeyDown(KeyCode.Ctrl) && !ImGui.IsKeyDown(KeyCode.Shift) && !ImGui.IsKeyDown(KeyCode.Alt))
					{
						if (ImGui.IsKeyPressed(KeyCode.T))
							currentManipulateOperation = GuizmoManipulateOperation.TRANSLATE;
						if (ImGui.IsKeyPressed(KeyCode.R))
							currentManipulateOperation = GuizmoManipulateOperation.ROTATE;
						if (ImGui.IsKeyPressed(KeyCode.S))
							currentManipulateOperation = GuizmoManipulateOperation.SCALE;
					}

					Vector3? snap = null;
					if (ImGui.IsKeyDown(KeyCode.Ctrl))
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
						matrix.decompose(out selectedEntity.data.position, out selectedEntity.data.rotation, out selectedEntity.data.scale);
						manipulateEdited = true;
					}
					if (ImGui.IsMouseButtonReleased(MouseButton.Left) && manipulateEdited)
					{
						// deactivated after edit
						instance.notifyEdit();
						manipulateEdited = false;
					}
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
		if (unsavedChangesPopup.Count > 0)
		{
			byte open = 1;
			ImGui.OpenPopup_Str("popup_unsaved");
			if (ImGui.BeginPopupModal("popup_unsaved", &open, ImGuiWindowFlags.NoResize))
			{
				if (unsavedChangesPopup[0].path != null)
					ImGui.TextUnformatted("Save changes to \"" + unsavedChangesPopup[0].filename + "\"?");
				else
					ImGui.TextUnformatted("Save changes to \"Untitled\"?");
				if (ImGui.Button("Yes"))
				{
					editor.save(unsavedChangesPopup[0]);

					editor.closeTab(unsavedChangesPopup[0]);
					unsavedChangesPopup.RemoveAt(0);
				}
				ImGui.SameLine();
				if (ImGui.Button("No"))
				{
					unsavedChangesPopup[0].notifySave();
					editor.closeTab(unsavedChangesPopup[0]);
					unsavedChangesPopup.RemoveAt(0);
				}
				ImGui.EndPopup();
			}
		}
	}
}
