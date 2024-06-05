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
		DragFloat3(instance, "Position", "transform_position", ref entity.data.position, 0.02f);
		DragFloat3Rotation(instance, "Rotation", "transform_rotation", ref entity.data.rotation, 1.0f);
		DragFloat3(instance, "Scale", "transform_scale", ref entity.data.scale, 0.02f);

		/*
		ImGui.TextUnformatted("Position");
		ImGui.SameLine(SPACING_X);
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x - RIGHT_PADDING);
		Vector3 newPosition = entity.position;
		if (ImGui.DragFloat3("##transform_position", &newPosition, 0.02f))
			entity.position = newPosition;
		if (ImGui.IsItemDeactivatedAfterEdit())
			instance.notifyEdit();
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
		*/
	}

	static unsafe void Model(Entity entity, EditorInstance instance)
	{
		if (ImGui.TreeNodeEx("Model", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen))
		{
			if (FileSelect(null, "model", ref entity.data.modelPath, "gltf"))
			{
				entity.reload();
				instance.notifyEdit();
			}

			ImGui.TreePop();
		}
	}

	static unsafe void Colliders(Entity entity, EditorInstance instance)
	{
		entity.showDebugColliders = false;
		if (ImGui.TreeNodeEx("Collider", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen))
		{
			entity.showDebugColliders = true;

			if (entity.data.colliders.Count > 0)
			{
				Combo(instance, "Rigid Body Type", "body_type", ref entity.data.rigidBodyType, ImGuiComboFlags.HeightRegular, 1);
				ImGui.Separator();
			}

			for (int i = 0; i < entity.data.colliders.Count; i++)
			{
				SceneFormat.ColliderData collider = entity.data.colliders[i];

				Vector2 topRight = ImGui.GetCursorPos();

				Combo(instance, "Collider Type", "collider_type" + i, ref collider.type, ImGuiComboFlags.HeightSmall);

				Checkbox(instance, "Trigger", "collider_trigger" + i, ref collider.trigger);

				if (collider.type == SceneFormat.ColliderType.Box)
				{
					DragFloat3(instance, "Size", "collider_size" + i, ref collider.size, 0.02f, 0, 100);
				}
				else if (collider.type == SceneFormat.ColliderType.Sphere)
				{
					float newRadius = collider.radius;
					if (DragFloat(instance, "Radius", "collider_radius" + i, ref newRadius, 0.02f, 0, 1000))
						collider.radius = newRadius;
				}
				else if (collider.type == SceneFormat.ColliderType.Capsule)
				{
					float newRadius = collider.radius;
					if (DragFloat(instance, "Radius", "collider_radius" + i, ref newRadius, 0.02f, 0, 1000))
						collider.radius = newRadius;

					float newHeight = collider.height;
					if (DragFloat(instance, "Height", "collider_height" + i, ref newHeight, 0.02f, 2 * collider.radius, 1000))
						collider.height = newHeight;
				}
				else if (collider.type == SceneFormat.ColliderType.Mesh || collider.type == SceneFormat.ColliderType.ConvexMesh)
				{
					if (FileSelect(null, "mesh_collider" + i, ref collider.meshColliderPath, "gltf"))
					{
						collider.meshCollider = collider.meshColliderPath != null ? Resource.GetModel(RainfallEditor.CompileAsset(collider.meshColliderPath)) : null;
						instance.notifyEdit();
					}
				}

				DragFloat3(instance, "Offset", "collider_offset" + i, ref collider.offset, 0.02f);

				if (collider.type != SceneFormat.ColliderType.Sphere)
				{
					DragFloat3Eulers(instance, "Rotation", "collider_rotation" + i, ref collider.eulers);
				}

				// X Button
				Vector2 cursorPos = ImGui.GetCursorPos();
				ImGui.SetCursorPos(new Vector2(PROPERTIES_PANEL_WIDTH - RIGHT_PADDING, topRight.y));
				if (ImGui.SmallButton("X##collider_remove" + i))
				{
					entity.data.colliders.RemoveAt(i--);
					instance.notifyEdit();
					ImGui.SetCursorPos(cursorPos);
					continue;
				}
				ImGui.SetCursorPos(cursorPos);

				entity.data.colliders[i] = collider;

				ImGui.Spacing();
				ImGui.Separator();
				ImGui.Spacing();
			}

			if (ImGui.Button("Add Collider"))
			{
				SceneFormat.ColliderData collider = new SceneFormat.ColliderData(new Vector3(2.0f));
				entity.data.colliders.Add(collider);
				instance.notifyEdit();
			}

			ImGui.Separator();

			if (entity.data.boneColliders != null)
			{
				if (entity.data.model != null)
				{
					foreach (string nodeName in entity.data.boneColliders.Keys)
					{
						SceneFormat.ColliderData collider = entity.data.boneColliders[nodeName];
						Node node = entity.data.model.skeleton.getNode(nodeName);
						int nodeID = node != null ? node.id : -1;

						if (nodeID != -1)
							entity.showDebugBoneColliders[nodeID] = false;
						if (TreeNodeRemovable(instance, nodeName, "bone_collider" + nodeName, out bool colliderRemoved))
						{
							if (nodeID != -1)
								entity.showDebugBoneColliders[nodeID] = true;

							Combo(instance, "Collider Type", "bone_collider_type" + nodeName, ref collider.type, ImGuiComboFlags.HeightSmall);

							Checkbox(instance, "Trigger", "bone_collider_trigger" + nodeName, ref collider.trigger);

							if (collider.type == SceneFormat.ColliderType.Box)
							{
								DragFloat3(instance, "Size", "bone_collider_size" + nodeName, ref collider.size, 0.02f, 0, 100);
							}
							else if (collider.type == SceneFormat.ColliderType.Sphere)
							{
								float newRadius = collider.radius;
								if (DragFloat(instance, "Radius", "bone_collider_radius" + nodeName, ref newRadius, 0.02f, 0, 1000))
									collider.radius = newRadius;
							}
							else if (collider.type == SceneFormat.ColliderType.Capsule)
							{
								float newRadius = collider.radius;
								if (DragFloat(instance, "Radius", "bone_collider_radius" + nodeName, ref newRadius, 0.02f, 0, 1000))
									collider.radius = newRadius;

								float newHeight = collider.height;
								if (DragFloat(instance, "Height", "bone_collider_height" + nodeName, ref newHeight, 0.02f, 2 * collider.radius, 1000))
									collider.height = newHeight;
							}
							else if (collider.type == SceneFormat.ColliderType.Mesh || collider.type == SceneFormat.ColliderType.ConvexMesh)
							{
								ImGui.TextUnformatted("Mesh bone collider not supported.");
							}

							DragFloat3(instance, "Offset", "bone_collider_offset" + nodeName, ref collider.offset, 0.02f);

							if (collider.type != SceneFormat.ColliderType.Sphere)
							{
								DragFloat3Eulers(instance, "Rotation", "bone_collider_rotation" + nodeName, ref collider.eulers);
							}

							entity.data.boneColliders[nodeName] = collider;

							ImGui.TreePop();
						}
						if (colliderRemoved)
						{
							entity.data.boneColliders.Remove(nodeName);
							if (entity.data.boneColliders.Count == 0)
								entity.data.boneColliders = null;
						}
					}
				}
			}
			else
			{
				if (entity.data.model != null && entity.data.model.scene->numAnimations > 0 && entity.data.boneColliders == null)
				{
					if (ImGui.Button("Create Bone Colliders"))
					{
						entity.data.boneColliders = new Dictionary<string, SceneFormat.ColliderData>();
						for (int i = 0; i < entity.data.model.skeleton.nodes.Length; i++)
						{
							Node node = entity.data.model.skeleton.nodes[i];
							bool deforming = !(node.name.IndexOf("ik", StringComparison.OrdinalIgnoreCase) >= 0
								|| node.name.IndexOf("pole_target", StringComparison.OrdinalIgnoreCase) >= 0
								|| node.name.IndexOf("poletarget", StringComparison.OrdinalIgnoreCase) >= 0);
							if (deforming)
							{
								float radius = 0.05f;
								float distanceToEnd;
								bool isLeafNode = node.children.Length == 0;
								if (isLeafNode)
									distanceToEnd = 0.05f;
								else
									distanceToEnd = node.children[0].transform.translation.length;
								SceneFormat.ColliderData collider = new SceneFormat.ColliderData(new Vector3(radius, distanceToEnd, radius), new Vector3(0.0f, 0.5f * distanceToEnd, 0.0f));
								if (!entity.data.boneColliders.ContainsKey(node.name))
									entity.data.boneColliders.Add(node.name, collider);
							}
						}
					}
				}
			}

			ImGui.TreePop();
		}
	}

	static unsafe void Lights(Entity entity, EditorInstance instance)
	{
		if (ImGui.TreeNodeEx("Lights", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen))
		{
			Matrix pv = Renderer.pv;

			for (int i = 0; i < entity.data.lights.Count; i++)
			{
				SceneFormat.LightData light = entity.data.lights[i];

				Vector2 topRight = ImGui.GetCursorPos();

				ColorEdit3(instance, "Color", "light_color" + i, ref light.color);

				DragFloat(instance, "Intensity", "light_intensity" + i, ref light.intensity, 0.02f, 0, 10000);

				DragFloat3(instance, "Offset", "light_offset" + i, ref light.offset, 0.02f);

				// X Button
				Vector2 cursorPos = ImGui.GetCursorPos();
				ImGui.SetCursorPos(new Vector2(PROPERTIES_PANEL_WIDTH - RIGHT_PADDING, topRight.y));
				if (ImGui.SmallButton("X##light_remove" + i))
				{
					entity.data.lights.RemoveAt(i--);
					instance.notifyEdit();
					ImGui.SetCursorPos(cursorPos);
					continue;
				}
				ImGui.SetCursorPos(cursorPos);

				entity.data.lights[i] = light;

				ImGui.Spacing();
				ImGui.Separator();
				ImGui.Spacing();

				Vector3 lightGlobalPosition = Matrix.CreateTransform(entity.data.position, entity.data.rotation) * light.offset;
				Vector2i screenPosition = MathHelper.WorldToScreenSpace(lightGlobalPosition, pv, Display.viewportSize);
				int width = 64;
				int height = 64;
				Texture lightGizmo = Resource.GetTexture("res/textures/light_gizmo.png");
				GUI.Texture(screenPosition.x - width / 2, screenPosition.y - height / 2, width, height, lightGizmo);
			}

			if (ImGui.Button("Add Light"))
			{
				SceneFormat.LightData light = new SceneFormat.LightData(new Vector3(1.0f), 1.0f);
				entity.data.lights.Add(light);
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
					StringUtils.WriteString(renameBuffer, selectedEntity.data.name);
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
						selectedEntity.data.name = new string((sbyte*)renameBufferPtr);
						instance.notifyEdit();
					}
				}

				Checkbox(instance, "Static", "entity_static", ref selectedEntity.data.isStatic);

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
