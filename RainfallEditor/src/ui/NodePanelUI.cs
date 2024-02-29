using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static partial class EditorUI
{
	static uint renamingEntity = 0;
	static byte[] renamingEntityBuffer = new byte[256];

	static unsafe void NodePanel(EditorInstance instance)
	{
		ImGui.SetNextWindowPos(new Vector2(0, ImGui.GetFrameHeight() * 2));
		ImGui.SetNextWindowSize(new Vector2(NODE_PANEL_WIDTH, Display.height - ImGui.GetFrameHeight() * 2));
		if (ImGui.Begin("Nodes", null, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
		{
			uint entityToRemove = 0;
			bool anyNodeHovered = false;

			if (instance.selectedEntity != 0)
			{
				if (ImGui.IsKeyPressed(KeyCode.F2))
				{
					renamingEntity = instance.selectedEntity;
					StringUtils.WriteString(renamingEntityBuffer, instance.getEntity(renamingEntity).name);
				}
				if (ImGui.IsKeyPressed(KeyCode.Delete))
				{
					entityToRemove = instance.selectedEntity;
				}
				if (ImGui.IsKeyPressed(KeyCode.Down))
				{
					Entity nextEntity = instance.getNextEntity(instance.selectedEntity);
					instance.selectedEntity = nextEntity.id;
					instance.notifyEdit();
				}
				if (ImGui.IsKeyPressed(KeyCode.Up))
				{
					Entity nextEntity = instance.getPrevEntity(instance.selectedEntity);
					instance.selectedEntity = nextEntity.id;
					instance.notifyEdit();
				}
			}

			for (int i = 0; i < instance.entities.Count; i++)
			{
				Entity entity = instance.entities[i];
				bool selected = entity.id == instance.selectedEntity;
				bool renaming = entity.id == renamingEntity;
				if (renaming)
				{
					Vector2 cursorPos = ImGui.GetCursorPos();
					if (ImGui.TreeNodeEx("##entity_" + i, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Selected | ImGuiTreeNodeFlags.AllowOverlap))
					{
						if (ImGui.IsItemHovered())
							anyNodeHovered = true;

						ImGui.SetCursorPos(cursorPos);
						fixed (byte* bufferPtr = renamingEntityBuffer)
						{
							ImGui.SetKeyboardFocusHere();
							ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, new Vector2(0.0f));

							if (ImGui.InputText("##entity_rename", bufferPtr, (ulong)renamingEntityBuffer.Length, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
							{
								entity.name = new string((sbyte*)bufferPtr);
								instance.notifyEdit();
								renamingEntity = 0;
							}
							else if (ImGui.IsKeyPressed(KeyCode.Esc) || ImGui.IsMouseButtonPressed(MouseButton.Left) && !ImGui.IsItemHovered())
							{
								renamingEntity = 0;
							}

							ImGui.PopStyleVar();
						}

						ImGui.TreePop();
					}
				}
				else
				{
					if (ImGui.TreeNodeEx(entity.name + "##entity_" + i, ImGuiTreeNodeFlags.Leaf | (selected ? ImGuiTreeNodeFlags.Selected : 0)))
					{
						if (ImGui.IsItemHovered())
							anyNodeHovered = true;

						if (ImGui.IsItemHovered() && ImGui.IsMouseButtonPressed(MouseButton.Left))
						{
							instance.selectedEntity = entity.id;
							instance.notifyEdit();
						}

						if (ImGui.BeginPopupContextItem())
						{
							if (ImGui.MenuItem("Rename"))
							{
								renamingEntity = entity.id;
							}
							if (ImGui.MenuItem("Remove"))
							{
								entityToRemove = entity.id;
							}
							ImGui.EndPopup();
						}

						ImGui.TreePop();
					}
				}
			}

			if (!anyNodeHovered)
			{
				if (ImGui.BeginPopupContextWindow())
				{
					if (ImGui.MenuItem("New Entity"))
					{
						instance.newEntity();
					}
					ImGui.EndPopup();
				}
			}

			if (entityToRemove != 0)
			{
				instance.removeEntity(instance.getEntity(entityToRemove));
			}
		}
		ImGui.End();
	}
}
