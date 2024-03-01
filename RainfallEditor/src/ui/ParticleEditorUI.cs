using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class EditorUI
{
	static byte[] renamingParticlesBuffer = new byte[256];


	static unsafe void Particles(Entity entity, EditorInstance instance)
	{
		if (ImGui.TreeNodeEx("Particles", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen))
		{
			for (int i = 0; i < entity.particles.Count; i++)
			{
				ParticleSystem particles = entity.particles[i];

				Vector2 topRight = ImGui.GetCursorPos();

				bool particlesOpen = ImGui.TreeNodeEx("##particles" + i, ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanAvailWidth);

				StringUtils.WriteString(renamingParticlesBuffer, particles.name);
				ImGui.SameLine(54);
				ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, new Vector2(0.0f));
				fixed (byte* newNamePtr = renamingParticlesBuffer)
				{
					if (ImGui.InputText("##entity_rename", newNamePtr, (ulong)renamingParticlesBuffer.Length, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
					{
						particles.name = new string((sbyte*)newNamePtr);
						instance.notifyEdit();
					}
				}
				ImGui.PopStyleVar();

				if (particlesOpen)
				{
					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					DragFloat(instance, "Lifetime", "particle_lifetime" + i, ref particles.lifetime, 0.02f, 0, 100);

					/*
					ImGui.TextUnformatted("Lifetime");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float lifetime = particles.lifetime;
					if (ImGui.DragFloat("##lifetime" + i, &lifetime, 0.02f, 0, 100))
						particles.lifetime = lifetime;
					*/

					DragFloat(instance, "Size", "particle_size" + i, ref particles.size, 0.001f, 0, 10);

					/*
					ImGui.TextUnformatted("Size");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float particleSize = particles.size;
					if (ImGui.DragFloat("##size" + i, &particleSize, 0.001f, 0, 10))
						particles.size = particleSize;
					*/

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					DragFloat(instance, "Emission Rate", "particle_emission_rate" + i, ref particles.emissionRate, 0.2f, 0, 1000);

					/*
					ImGui.TextUnformatted("Emission Rate");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float emissionRate = particles.emissionRate;
					if (ImGui.DragFloat("##emission_rate" + i, &emissionRate, 0.2f, 0, 1000))
						particles.emissionRate = emissionRate;
					*/

					Combo(instance, "Spawn Shape", "particle_spawn_shape" + i, ref particles.spawnShape, ImGuiComboFlags.HeightSmall);

					/*
					ImGui.TextUnformatted("Spawn Shape");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					if (ImGui.BeginCombo("##shawn_shape" + i, particles.spawnShape.ToString(), ImGuiComboFlags.HeightSmall))
					{
						ParticleSpawnShape spawnShape = particles.spawnShape;

						if (ImGui.Selectable_Bool("Point"))
							spawnShape = ParticleSpawnShape.Point;
						if (ImGui.Selectable_Bool("Circle"))
							spawnShape = ParticleSpawnShape.Circle;
						if (ImGui.Selectable_Bool("Sphere"))
							spawnShape = ParticleSpawnShape.Sphere;
						if (ImGui.Selectable_Bool("Line"))
							spawnShape = ParticleSpawnShape.Line;

						if (spawnShape != particles.spawnShape)
						{
							particles.spawnShape = spawnShape;
							instance.notifyEdit();
						}

						ImGui.EndCombo();
					}
					*/

					DragFloat3(instance, "Spawn Offset", "particle_spawn_offset" + i, ref particles.spawnOffset, 0.005f);

					/*
					ImGui.TextUnformatted("Spawn Offset");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					Vector3 spawnOffset = particles.spawnOffset;
					if (ImGui.DragFloat3("##spawn_offset" + i, &spawnOffset, 0.005f))
						particles.spawnOffset = spawnOffset;
					*/

					if (particles.spawnShape == ParticleSpawnShape.Circle || particles.spawnShape == ParticleSpawnShape.Sphere)
					{
						DragFloat(instance, "Spawn Radius", "particle_spawn_radius" + i, ref particles.spawnRadius, 0.01f, 0, 10);

						/*
						ImGui.TextUnformatted("Spawn Radius");
						ImGui.SameLine(SPACING_X);
						ImGui.SetNextItemWidth(ITEM_WIDTH);
						float spawnRadius = particles.spawnRadius;
						if (ImGui.DragFloat("##spawn_radius" + i, &spawnRadius, 0.01f, 0, 10))
							particles.spawnRadius = spawnRadius;
						*/
					}
					else if (particles.spawnShape == ParticleSpawnShape.Line)
					{
						DragFloat3(instance, "Line End", "particle_line_end" + i, ref particles.lineEnd, 0.005f);

						/*
						ImGui.TextUnformatted("Line End");
						ImGui.SameLine(SPACING_X);
						ImGui.SetNextItemWidth(ITEM_WIDTH);
						Vector3 lineEnd = particles.lineEnd;
						if (ImGui.DragFloat3("##line_end" + i, &lineEnd, 0.005f))
							particles.lineEnd = lineEnd;
						*/
					}

					Checkbox(instance, "Random Rotation", "particle_random_start_rotation" + i, ref particles.randomStartRotation);

					/*
					ImGui.TextUnformatted("Random Rotation");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					byte randomStartRotation = (byte)(particles.randomStartRotation ? 1 : 0);
					if (ImGui.Checkbox("##random_start_rotation" + i, &randomStartRotation))
						particles.randomStartRotation = randomStartRotation != 0;
					*/

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					Checkbox(instance, "Follow", "particle_follow" + i, ref particles.follow);

					//ImGui.TextUnformatted("Follow");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//byte follow = (byte)(particles.follow ? 1 : 0);
					//if (ImGui.Checkbox("##follow" + i, &follow))
					//	particles.follow = follow != 0;

					DragFloat(instance, "Gravity", "particle_gravity" + i, ref particles.gravity, 0.02f);

					//ImGui.TextUnformatted("Gravity");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float gravity = particles.gravity;
					//if (ImGui.DragFloat("##gravity" + i, &gravity, 0.02f))
					//	particles.gravity = gravity;

					DragFloat3(instance, "Start velocity", "particle_start_velocity" + i, ref particles.startVelocity, 0.02f);

					//ImGui.TextUnformatted("Start Velocity");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//Vector3 startVelocity = particles.startVelocity;
					//if (ImGui.DragFloat3("##start_velocity" + i, &startVelocity, 0.02f))
					//	particles.startVelocity = startVelocity;

					DragFloat(instance, "Rotation Speed", "particle_rotation_speed" + i, ref particles.rotationSpeed, 0.02f);

					//ImGui.TextUnformatted("Rotation Speed");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float rotationSpeed = particles.rotationSpeed;
					//if (ImGui.DragFloat("##rotation_speed" + i, &rotationSpeed, 0.02f))
					//	particles.rotationSpeed = rotationSpeed;

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					if (FileSelect("Texture Atlas", "particle_atlas" + i, ref particles.textureAtlasPath, "png"))
					{
						particles.reload();
						instance.notifyEdit();
					}

					if (particles.textureAtlasPath != null)
					{
						DragInt2(instance, "Atlas Size", "particle_atlas_size" + i, ref particles.atlasSize, 0.04f, 1, 10000);

						//ImGui.TextUnformatted("Atlas Size");
						//ImGui.SameLine(SPACING_X);
						//ImGui.SetNextItemWidth(ITEM_WIDTH);
						//Vector2i atlasSize = particles.atlasSize;
						//if (ImGui.DragInt2("##atlas_size" + i, &atlasSize, 0.04f, 1, 10000))
						//	particles.atlasSize = atlasSize;

						DragInt(instance, "Frame Count", "particle_frame_count" + i, ref particles.numFrames, 0.1f, 1, 100);

						//ImGui.TextUnformatted("Frame Count");
						//ImGui.SameLine(SPACING_X);
						//ImGui.SetNextItemWidth(ITEM_WIDTH);
						//int numFrames = particles.numFrames;
						//if (ImGui.DragInt("##frame_count" + i, &numFrames, 0.1f, 1, 100))
						//	particles.numFrames = numFrames;

						Checkbox(instance, "Linear Filtering", "particle_linear_filtering" + i, ref particles.linearFiltering);

						//ImGui.TextUnformatted("Linear Filtering");
						//ImGui.SameLine(SPACING_X);
						//ImGui.SetNextItemWidth(ITEM_WIDTH);
						//byte linearFiltering = (byte)(particles.linearFiltering ? 1 : 0);
						//if (ImGui.Checkbox("##linear_filtering" + i, &linearFiltering))
						//	particles.linearFiltering = linearFiltering != 0;
					}

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					ColorEdit4(instance, "Color", "particle_color" + i, ref particles.color, true);

					//ImGui.TextUnformatted("Color");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//Vector4 color = particles.color;
					//if (ImGui.ColorEdit4("##color" + i, &color, ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.NoInputs))
					//	particles.color = color;

					Checkbox(instance, "Additive", "particle_additive" + i, ref particles.additive);

					//ImGui.TextUnformatted("Additive");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//byte additive = (byte)(particles.additive ? 1 : 0);
					//if (ImGui.Checkbox("##additive" + i, &additive))
					//	particles.additive = additive != 0;

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					DragFloat(instance, "Randomize Velocity", "particle_random_velocity" + i, ref particles.randomVelocity, 0.001f, 0, 100);

					//ImGui.TextUnformatted("Randomize Velocity");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float randomVelocity = particles.randomVelocity;
					//if (ImGui.DragFloat("##random_velocity" + i, &randomVelocity, 0.001f, 0, 100))
					//	particles.randomVelocity = randomVelocity;

					DragAngle(instance, "Randomize Rotation", "particle_random_rotation" + i, ref particles.randomRotationSpeed, 1.0f, 0, 10000);

					//ImGui.TextUnformatted("Randomize Rotation");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float randomRotationSpeed = particles.randomRotationSpeed * 180 / MathF.PI;
					//if (ImGui.DragFloat("##random_rotation_speed" + i, &randomRotationSpeed, 4.0f, 0, 10000))
					//	particles.randomRotationSpeed = randomRotationSpeed / 180 * MathF.PI;

					DragFloat(instance, "Randomize Lifetime", "particle_random_lifetime" + i, ref particles.randomLifetime, 0.002f, 0, 1);

					//ImGui.TextUnformatted("Randomize Lifetime");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float randomLifetime = particles.randomLifetime;
					//if (ImGui.DragFloat("##random_lifetime" + i, &randomLifetime, 0.002f, 0, 1))
					//	particles.randomLifetime = randomLifetime;

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					bool animateSizeEnabled = particles.sizeAnim != null;
					bool animateSizeOpen = TreeNodeOptional(instance, "Animate Size", "particle_size_anim" + i, ref animateSizeEnabled);
					if (animateSizeEnabled != (particles.sizeAnim != null))
						particles.sizeAnim = animateSizeEnabled ? new Gradient<float>(0.1f, 0.0f) : null;
					if (animateSizeOpen)
					{
						if (particles.sizeAnim != null)
						{
							float startValue = particles.sizeAnim.getValue(0);
							if (DragFloat(instance, "From", "particle_size_anim_start" + i, ref startValue, 0.001f, 0, 10))
								particles.sizeAnim.setValue(0, startValue);

							float endValue = particles.sizeAnim.getValue(1);
							if (DragFloat(instance, "To", "particle_size_anim_end" + i, ref endValue, 0.001f, 0, 10))
								particles.sizeAnim.setValue(1, endValue);
						}

						ImGui.TreePop();
					}

					//ImGui.SetNextItemAllowOverlap();
					//bool animSizeSettings = ImGui.TreeNodeEx("Animate Size##particle_size_anim_settings" + i);
					//ImGui.SameLine(SPACING_X);
					//byte animateSize = (byte)(particles.sizeAnim != null ? 1 : 0);
					//ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, Vector2.Zero);
					//if (ImGui.Checkbox("##particle_size_anim" + i, &animateSize))
					//	particles.sizeAnim = animateSize != 0 ? new Gradient<float>(0.1f, 0.0f) : null;
					//ImGui.PopStyleVar();
					//if (animSizeSettings)
					//{
					//	if (particles.sizeAnim != null)
					//	{
					//		ImGui.TextUnformatted("From");
					//		ImGui.SameLine();
					//		ImGui.SetNextItemWidth(100);
					//		float startValue = particles.sizeAnim.getValue(0);
					//		if (ImGui.DragFloat("##particle_size_anim_start" + i, &startValue, 0.001f, 0, 10))
					//			particles.sizeAnim.setValue(0, startValue);

					//		ImGui.SameLine();
					//		ImGui.TextUnformatted("to");
					//		ImGui.SameLine();
					//		ImGui.SetNextItemWidth(100);
					//		float endValue = particles.sizeAnim.getValue(1);
					//		if (ImGui.DragFloat("##particle_size_anim_end" + i, &endValue, 0.001f, 0, 10))
					//			particles.sizeAnim.setValue(1, endValue);
					//	}

					//	ImGui.TreePop();
					//}

					bool animateColorEnabled = particles.colorAnim != null;
					bool animateColorOpen = TreeNodeOptional(instance, "Animate Color", "particle_color_anim" + i, ref animateColorEnabled);
					if (animateColorEnabled != (particles.colorAnim != null))
						particles.colorAnim = animateColorEnabled ? new Gradient<Vector4>(new Vector4(1.0f), new Vector4(0.5f, 0.5f, 0.5f, 1.0f)) : null;
					if (animateColorOpen)
					{
						if (particles.colorAnim != null)
						{
							Vector4 startValue = particles.colorAnim.getValue(0);
							if (ColorEdit4(instance, "From", "particle_color_anim_start" + i, ref startValue, true))
								particles.colorAnim.setValue(0, startValue);

							Vector4 endValue = particles.colorAnim.getValue(1);
							if (ColorEdit4(instance, "To", "particle_color_anim_end" + i, ref endValue, true))
								particles.colorAnim.setValue(1, endValue);
						}

						ImGui.TreePop();
					}

					//ImGui.SetNextItemAllowOverlap();
					//bool animColorSettings = ImGui.TreeNodeEx("Animate Color##particle_color_anim_settings" + i);
					//ImGui.SameLine(SPACING_X);
					//byte animateColor = (byte)(particles.colorAnim != null ? 1 : 0);
					//ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, Vector2.Zero);
					//if (ImGui.Checkbox("##particle_color_anim" + i, &animateColor))
					//	particles.colorAnim = animateColor != 0 ? new Gradient<Vector4>(new Vector4(1.0f), new Vector4(0.5f, 0.5f, 0.5f, 1.0f)) : null;
					//ImGui.PopStyleVar();
					//if (animColorSettings)
					//{
					//	if (particles.colorAnim != null)
					//	{
					//		ImGui.TextUnformatted("From");
					//		ImGui.SameLine();
					//		Vector4 startValue = particles.colorAnim.getValue(0);
					//		if (ImGui.ColorEdit4("##particle_color_anim_start" + i, &startValue, ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.NoInputs))
					//			particles.colorAnim.setValue(0, startValue);

					//		ImGui.SameLine();
					//		ImGui.TextUnformatted("to");
					//		ImGui.SameLine();
					//		Vector4 endValue = particles.colorAnim.getValue(1);
					//		if (ImGui.ColorEdit4("##particle_color_anim_end" + i, &endValue, ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.NoInputs))
					//			particles.colorAnim.setValue(1, endValue);
					//	}

					//	ImGui.TreePop();
					//}

					ImGui.TreePop();
				}

				// X Button
				Vector2 cursorPos = ImGui.GetCursorPos();
				ImGui.SetCursorPos(new Vector2(PROPERTIES_PANEL_WIDTH - RIGHT_PADDING, topRight.y));
				if (ImGui.SmallButton("X##particles_remove" + i))
				{
					entity.particles.RemoveAt(i--);
					instance.notifyEdit();
					ImGui.SetCursorPos(cursorPos);
					continue;
				}
				ImGui.SetCursorPos(cursorPos);

				ImGui.Spacing();
				ImGui.Separator();
				ImGui.Spacing();
			}

			if (ImGui.Button("Add Particle Effect"))
			{
				ParticleSystem particles = new ParticleSystem(1000);
				particles.name = entity.newParticleName();
				entity.particles.Add(particles);
				instance.notifyEdit();
			}

			ImGui.TreePop();
		}
	}
}
