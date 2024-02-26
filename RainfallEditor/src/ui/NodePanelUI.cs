using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static partial class EditorUI
{
	static Entity renamingEntity = null;
	static byte[] renamingEntityBuffer = new byte[256];

	static unsafe void NodePanel(EditorInstance instance)
	{

		ImGui.SetNextWindowPos(new Vector2(0, ImGui.GetFrameHeight() * 2));
		ImGui.SetNextWindowSize(new Vector2(NODE_PANEL_WIDTH, Display.height - ImGui.GetFrameHeight() * 2));
		if (ImGui.Begin("Nodes", null, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
		{
			Entity entityToRemove = null;
			bool anyNodeHovered = false;

			if (instance.selectedNode != null && ImGui.IsKeyPressed(KeyCode.F2))
			{
				renamingEntity = instance.selectedNode;
				StringUtils.WriteString(renamingEntityBuffer, renamingEntity.name);
			}

			for (int i = 0; i < instance.entities.Count; i++)
			{
				Entity entity = instance.entities[i];
				bool selected = entity == instance.selectedNode;
				bool renaming = entity == renamingEntity;
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
								renamingEntity = null;
							}
							else if (ImGui.IsKeyPressed(KeyCode.Esc) || ImGui.IsMouseButtonPressed(MouseButton.Left) && !ImGui.IsItemHovered())
							{
								renamingEntity = null;
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
							instance.selectedNode = entity;

						if (ImGui.BeginPopupContextItem())
						{
							if (ImGui.MenuItem("Rename"))
							{
								renamingEntity = entity;
							}
							if (ImGui.MenuItem("Remove"))
							{
								entityToRemove = entity;
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

			if (entityToRemove != null)
			{
				instance.removeEntity(entityToRemove);
			}
		}
		ImGui.End();
	}
}
