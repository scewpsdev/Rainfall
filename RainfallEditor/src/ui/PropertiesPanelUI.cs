using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static partial class EditorUI
{
	static uint lastSelectedEntity = 0;
	static byte[] renameBuffer = new byte[256];


	static unsafe void Transform(Entity entity, EditorInstance instance)
	{
		ImGui.TextUnformatted("Position");
		ImGui.SameLine();
		Vector3 newPosition = entity.position;
		if (ImGui.DragFloat3("##transform_position", &newPosition, 0.02f))
		{
			entity.position = newPosition;
			instance.notifyEdit();
		}
		ImGui.TextUnformatted("Rotation");
		ImGui.SameLine();
		Vector3 newEulers = entity.rotation.eulers / MathF.PI * 180;
		if (ImGui.DragFloat3("##transform_eulers", &newEulers, 1.0f))
		{
			entity.rotation = Quaternion.FromEulerAngles(newEulers / 180 * MathF.PI);
			instance.notifyEdit();
		}
		ImGui.TextUnformatted("Scale");
		ImGui.SameLine();
		Vector3 newScale = entity.scale;
		if (ImGui.DragFloat3("##transform_scale", &newScale, 0.02f))
		{
			entity.scale = newScale;
			instance.notifyEdit();
		}
	}

	static unsafe void Model(Entity entity, EditorInstance instance)
	{
		// Model
		if (ImGui.TreeNodeEx("Model", ImGuiTreeNodeFlags.DefaultOpen))
		{
			ImGui.SetNextItemAllowOverlap();
			ImGui.InputText("##entity_model", entity.modelPath != null ? StringUtils.GetFilenameFromPath(entity.modelPath) : "", ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.AutoSelectAll);
			ImGui.SameLine();
			if (entity.modelPath != null)
			{
				float cursorX = ImGui.GetCursorPosX();
				ImGui.SetCursorPosX(cursorX - 28);
				if (ImGui.SmallButton("X##model_remove"))
				{
					entity.modelPath = null;
					entity.reload();
					instance.notifyEdit();
				}
				ImGui.SameLine();
				ImGui.SetCursorPosX(cursorX);
			}
			if (ImGui.Button("Browse"))
			{
				byte* outPath = null;
				NFDResult result = NFD.NFD_OpenDialog("gltf", null, &outPath);
				if (result == NFDResult.NFD_OKAY)
				{
					entity.modelPath = new string((sbyte*)outPath);
					entity.reload();
					NFD.NFDi_Free(outPath);
					instance.notifyEdit();
				}
			}

			ImGui.TreePop();
		}
	}

	static unsafe void Colliders(Entity entity, EditorInstance instance)
	{
		// Collider
		if (ImGui.TreeNodeEx("Collider", ImGuiTreeNodeFlags.DefaultOpen))
		{
			for (int i = 0; i < entity.colliders.Count; i++)
			{
				ColliderData collider = entity.colliders[i];

				Vector2 topRight = ImGui.GetCursorPos();

				if (ImGui.BeginCombo("Type##collider_type" + i, collider.type.ToString(), ImGuiComboFlags.HeightSmall))
				{
					if (ImGui.Selectable_Bool("Box"))
						collider.type = ColliderType.Box;
					if (ImGui.Selectable_Bool("Sphere"))
						collider.type = ColliderType.Sphere;
					if (ImGui.Selectable_Bool("Capsule"))
						collider.type = ColliderType.Capsule;
					if (ImGui.Selectable_Bool("Mesh"))
						collider.type = ColliderType.Mesh;
					ImGui.EndCombo();
				}

				if (collider.type == ColliderType.Box)
				{
					Vector3 newSize = collider.size;
					if (ImGui.DragFloat3("Size##collider_size" + i, &newSize, 0.02f))
						collider.size = newSize;
				}
				else if (collider.type == ColliderType.Sphere)
				{
					float newRadius = collider.radius;
					if (ImGui.DragFloat("Radius##collider_radius" + i, &newRadius, 0.02f, 0, 1000))
						collider.radius = newRadius;
				}
				else if (collider.type == ColliderType.Capsule)
				{
					float newRadius = collider.radius;
					if (ImGui.DragFloat("Radius##collider_radius" + i, &newRadius, 0.02f, 0, 1000))
						collider.radius = newRadius;

					float newHeight = collider.height;
					if (ImGui.DragFloat("Height##collider_height" + i, &newHeight, 0.02f, 2 * collider.radius, 1000))
						collider.height = newHeight;
				}
				else if (collider.type == ColliderType.Mesh)
				{
					ImGui.SetNextItemAllowOverlap();
					ImGui.InputText("##collider_mesh", collider.meshColliderPath != null ? StringUtils.GetFilenameFromPath(collider.meshColliderPath) : "", ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.AutoSelectAll);
					ImGui.SameLine();
					if (collider.meshColliderPath != null)
					{
						float cursorX = ImGui.GetCursorPosX();
						ImGui.SetCursorPosX(cursorX - 28);
						if (ImGui.SmallButton("X##collider_mesh_remove"))
						{
							collider.meshColliderPath = null;
							collider.reload();
							instance.notifyEdit();
						}
						ImGui.SameLine();
						ImGui.SetCursorPosX(cursorX);
					}
					if (ImGui.Button("Browse"))
					{
						byte* outPath = null;
						NFDResult result = NFD.NFD_OpenDialog("gltf", null, &outPath);
						if (result == NFDResult.NFD_OKAY)
						{
							collider.meshColliderPath = new string((sbyte*)outPath);
							collider.reload();
							NFD.NFDi_Free(outPath);
							instance.notifyEdit();
						}
					}
				}

				Vector3 newOffset = collider.offset;
				if (ImGui.DragFloat3("Offset##collider_offset" + i, &newOffset, 0.02f))
					collider.offset = newOffset;

				if (collider.type != ColliderType.Sphere)
				{
					Vector3 newEulers = collider.eulers * 180 / MathF.PI;
					if (ImGui.DragFloat3("Rotation##collider_rotation" + i, &newEulers, 1.0f))
						collider.eulers = newEulers / 180 * MathF.PI;
				}

				// X Button
				Vector2 cursorPos = ImGui.GetCursorPos();
				ImGui.SetCursorPos(new Vector2(320, topRight.y));
				if (ImGui.SmallButton("X##collider_remove" + i))
				{
					entity.colliders.RemoveAt(i--);
					instance.notifyEdit();
					ImGui.SetCursorPos(cursorPos);
					continue;
				}
				ImGui.SetCursorPos(cursorPos);

				if (entity.colliders[i] != collider)
				{
					entity.colliders[i] = collider;
					instance.notifyEdit();
				}

				ImGui.Separator();
			}

			if (ImGui.Button("Add Collider"))
			{
				ColliderData collider = new ColliderData(new Vector3(2.0f));
				entity.colliders.Add(collider);
				instance.notifyEdit();
			}

			ImGui.TreePop();
		}
	}

	static unsafe void Lights(Entity entity, EditorInstance instance)
	{
		// Lights
		if (ImGui.TreeNodeEx("Lights", ImGuiTreeNodeFlags.DefaultOpen))
		{
			for (int i = 0; i < entity.lights.Count; i++)
			{
				LightData light = entity.lights[i];

				Vector2 topRight = ImGui.GetCursorPos();

				Vector3 newColor = light.color;
				if (ImGui.ColorEdit3("Color", &newColor))
					light.color = newColor;

				float newIntensity = light.intensity;
				if (ImGui.DragFloat("Intensity", &newIntensity, 0.02f))
					light.intensity = newIntensity;

				Vector3 newOffset = light.offset;
				if (ImGui.DragFloat3("Offset", &newOffset, 0.02f))
					light.offset = newOffset;

				// X Button
				Vector2 cursorPos = ImGui.GetCursorPos();
				ImGui.SetCursorPos(new Vector2(320, topRight.y));
				if (ImGui.SmallButton("X##light_remove" + i))
				{
					entity.lights.RemoveAt(i--);
					instance.notifyEdit();
					ImGui.SetCursorPos(cursorPos);
					continue;
				}
				ImGui.SetCursorPos(cursorPos);

				if (entity.lights[i] != light)
				{
					entity.lights[i] = light;
					instance.notifyEdit();
				}


				ImGui.Separator();
			}

			if (ImGui.Button("Add Light"))
			{
				LightData light = new LightData(new Vector3(1.0f), 1.0f);
				entity.lights.Add(light);
				instance.notifyEdit();
			}

			ImGui.TreePop();
		}
	}

	static unsafe void PropertiesPanel(EditorInstance instance, RainfallEditor editor)
	{
		ImGui.SetNextWindowPos(new Vector2(Display.width - PROPERTIES_PANEL_WIDTH, ImGui.GetFrameHeight() * 2));
		ImGui.SetNextWindowSize(new Vector2(PROPERTIES_PANEL_WIDTH, Display.height - ImGui.GetFrameHeight() * 2));
		if (ImGui.Begin("Properties", null, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
		{
			if (instance.selectedEntity != 0)
			{
				Entity selectedEntity = instance.getSelectedEntity();

				if (instance.selectedEntity != lastSelectedEntity)
				{
					StringUtils.WriteString(renameBuffer, selectedEntity.name);
					lastSelectedEntity = instance.selectedEntity;
				}

				// Name
				ImGui.TextUnformatted("Name");
				ImGui.SameLine();
				ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x);
				fixed (byte* renameBufferPtr = renameBuffer)
				{
					if (ImGui.InputText("##entity_name", renameBufferPtr, (ulong)renameBuffer.Length, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
					{
						selectedEntity.name = new string((sbyte*)renameBufferPtr);
						instance.notifyEdit();
					}
				}

				Transform(selectedEntity, instance);

				ImGui.Separator();

				Model(selectedEntity, instance);

				ImGui.Separator();

				Colliders(selectedEntity, instance);

				ImGui.Separator();

				Lights(selectedEntity, instance);
			}
			else
			{
				lastSelectedEntity = 0;
			}
		}
		ImGui.End();
	}
}
