using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static partial class EditorUI
{
	const int SPACING_X = 180;
	const int ITEM_WIDTH = 180;
	const int RIGHT_PADDING = 35;

	static uint lastSelectedEntity = 0;
	static byte[] renameBuffer = new byte[256];


	static unsafe bool FileSelect(string label, string id, ref string path, string filterList)
	{
		bool changed = false;

		if (label != null)
		{
			ImGui.TextUnformatted(label);
			ImGui.SameLine(SPACING_X);
		}

		ImGui.SetNextItemAllowOverlap();
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - 60);
		ImGui.InputText("##input_" + id, path != null ? StringUtils.GetFilenameFromPath(path) : "", ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.AutoSelectAll);
		ImGui.SameLine();
		if (path != null)
		{
			float cursorX = ImGui.GetCursorPosX();
			ImGui.SetCursorPosX(cursorX - 28);
			if (ImGui.SmallButton("X##remove_" + id))
			{
				path = null;
				changed = true;
			}
			ImGui.SameLine();
			ImGui.SetCursorPosX(cursorX);
		}

		if (ImGui.Button("Browse##" + id))
		{
			byte* outPath = null;
			NFDResult result = NFD.NFD_OpenDialog(filterList, null, &outPath);
			if (result == NFDResult.NFD_OKAY)
			{
				path = new string((sbyte*)outPath);
				NFD.NFDi_Free(outPath);
				changed = true;
			}
		}

		return changed;
	}

	static unsafe void Transform(Entity entity, EditorInstance instance)
	{
		ImGui.TextUnformatted("Position");
		ImGui.SameLine(SPACING_X);
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector3 newPosition = entity.position;
		if (ImGui.DragFloat3("##transform_position", &newPosition, 0.02f))
		{
			entity.position = newPosition;
			instance.notifyEdit();
		}
		ImGui.TextUnformatted("Rotation");
		ImGui.SameLine(SPACING_X);
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector3 newEulers = entity.rotation.eulers / MathF.PI * 180;
		if (ImGui.DragFloat3("##transform_eulers", &newEulers, 1.0f))
		{
			entity.rotation = Quaternion.FromEulerAngles(newEulers / 180 * MathF.PI);
			instance.notifyEdit();
		}
		ImGui.TextUnformatted("Scale");
		ImGui.SameLine(SPACING_X);
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector3 newScale = entity.scale;
		if (ImGui.DragFloat3("##transform_scale", &newScale, 0.02f))
		{
			entity.scale = newScale;
			instance.notifyEdit();
		}
	}

	static unsafe void Model(Entity entity, EditorInstance instance)
	{
		if (ImGui.TreeNodeEx("Model", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen))
		{
			if (FileSelect(null, "model", ref entity.modelPath, "gltf"))
			{
				entity.reload();
				instance.notifyEdit();
			}

			ImGui.TreePop();
		}
	}

	static unsafe void Colliders(Entity entity, EditorInstance instance)
	{
		if (ImGui.TreeNodeEx("Collider", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen))
		{
			for (int i = 0; i < entity.colliders.Count; i++)
			{
				ColliderData collider = entity.colliders[i];

				Vector2 topRight = ImGui.GetCursorPos();

				ImGui.TextUnformatted("Collider Type");
				ImGui.SameLine(SPACING_X);
				ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
				if (ImGui.BeginCombo("##collider_type" + i, collider.type.ToString(), ImGuiComboFlags.HeightSmall))
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
					ImGui.TextUnformatted("Size");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
					Vector3 newSize = collider.size;
					if (ImGui.DragFloat3("##collider_size" + i, &newSize, 0.02f))
						collider.size = newSize;
				}
				else if (collider.type == ColliderType.Sphere)
				{
					ImGui.TextUnformatted("Radius");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
					float newRadius = collider.radius;
					if (ImGui.DragFloat("##collider_radius" + i, &newRadius, 0.02f, 0, 1000))
						collider.radius = newRadius;
				}
				else if (collider.type == ColliderType.Capsule)
				{
					ImGui.TextUnformatted("Radius");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
					float newRadius = collider.radius;
					if (ImGui.DragFloat("##collider_radius" + i, &newRadius, 0.02f, 0, 1000))
						collider.radius = newRadius;

					ImGui.TextUnformatted("Height");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
					float newHeight = collider.height;
					if (ImGui.DragFloat("##collider_height" + i, &newHeight, 0.02f, 2 * collider.radius, 1000))
						collider.height = newHeight;
				}
				else if (collider.type == ColliderType.Mesh)
				{
					if (FileSelect(null, "mesh_collider" + i, ref collider.meshColliderPath, "gltf"))
					{
						collider.reload();
						instance.notifyEdit();
					}
				}

				ImGui.TextUnformatted("Offset");
				ImGui.SameLine(SPACING_X);
				ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
				Vector3 newOffset = collider.offset;
				if (ImGui.DragFloat3("##collider_offset" + i, &newOffset, 0.02f))
					collider.offset = newOffset;

				if (collider.type != ColliderType.Sphere)
				{
					ImGui.TextUnformatted("Rotation");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
					Vector3 newEulers = collider.eulers * 180 / MathF.PI;
					if (ImGui.DragFloat3("##collider_rotation" + i, &newEulers, 1.0f))
						collider.eulers = newEulers / 180 * MathF.PI;
				}

				// X Button
				Vector2 cursorPos = ImGui.GetCursorPos();
				ImGui.SetCursorPos(new Vector2(PROPERTIES_PANEL_WIDTH - RIGHT_PADDING, topRight.y));
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

				ImGui.Spacing();
				ImGui.Separator();
				ImGui.Spacing();
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
		if (ImGui.TreeNodeEx("Lights", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen))
		{
			for (int i = 0; i < entity.lights.Count; i++)
			{
				LightData light = entity.lights[i];

				Vector2 topRight = ImGui.GetCursorPos();

				ImGui.TextUnformatted("Color");
				ImGui.SameLine(SPACING_X);
				ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
				Vector3 newColor = light.color;
				if (ImGui.ColorEdit3("##light_color" + i, &newColor, ImGuiColorEditFlags.NoInputs))
					light.color = newColor;

				ImGui.TextUnformatted("Intensity");
				ImGui.SameLine(SPACING_X);
				ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
				float newIntensity = light.intensity;
				if (ImGui.DragFloat("##light_intensity", &newIntensity, 0.02f))
					light.intensity = newIntensity;

				ImGui.TextUnformatted("Offset");
				ImGui.SameLine(SPACING_X);
				ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
				Vector3 newOffset = light.offset;
				if (ImGui.DragFloat3("##light_intensity", &newOffset, 0.02f))
					light.offset = newOffset;

				// X Button
				Vector2 cursorPos = ImGui.GetCursorPos();
				ImGui.SetCursorPos(new Vector2(PROPERTIES_PANEL_WIDTH - RIGHT_PADDING, topRight.y));
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

				ImGui.Spacing();
				ImGui.Separator();
				ImGui.Spacing();
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
				ImGui.SameLine(SPACING_X);
				ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
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

				ImGui.Separator();

				Particles(selectedEntity, instance);
			}
			else
			{
				lastSelectedEntity = 0;
			}
		}
		ImGui.End();
	}
}
