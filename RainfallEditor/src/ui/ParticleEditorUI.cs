using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class EditorUI
{
	static unsafe void Particles(Entity entity, EditorInstance instance)
	{
		if (ImGui.TreeNodeEx("Particles", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen))
		{
			for (int i = 0; i < entity.particles.Count; i++)
			{
				ParticleSystem particles = new ParticleSystem(0);
				particles.copyData(entity.particles[i]);

				Vector2 topRight = ImGui.GetCursorPos();

				if (ImGui.TreeNodeEx(particles.name + "##particles" + i, ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanAvailWidth))
				{
					ImGui.TextUnformatted("Lifetime");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float lifetime = particles.lifetime;
					if (ImGui.DragFloat("##lifetime" + i, &lifetime, 0.02f, 0, 100))
						particles.lifetime = lifetime;

					ImGui.TextUnformatted("Size");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float particleSize = particles.particleSize;
					if (ImGui.DragFloat("##size" + i, &particleSize, 0.001f, 0, 10))
						particles.particleSize = particleSize;

					ImGui.TextUnformatted("Emission Rate");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float emissionRate = particles.emissionRate;
					if (ImGui.DragFloat("##emission_rate" + i, &emissionRate, 0.2f, 0, 1000))
						particles.emissionRate = emissionRate;

					ImGui.TextUnformatted("Spawn Shape");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					if (ImGui.BeginCombo("##shawn_shape" + i, particles.spawnShape.ToString(), ImGuiComboFlags.HeightSmall))
					{
						if (ImGui.Selectable_Bool("Point"))
							particles.spawnShape = ParticleSpawnShape.Point;
						if (ImGui.Selectable_Bool("Circle"))
							particles.spawnShape = ParticleSpawnShape.Circle;
						if (ImGui.Selectable_Bool("Sphere"))
							particles.spawnShape = ParticleSpawnShape.Sphere;
						if (ImGui.Selectable_Bool("Line"))
							particles.spawnShape = ParticleSpawnShape.Line;

						ImGui.EndCombo();
					}

					ImGui.TextUnformatted("Spawn Offset");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					Vector3 spawnOffset = particles.spawnOffset;
					if (ImGui.DragFloat3("##spawn_offset" + i, &spawnOffset, 0.005f))
						particles.spawnOffset = spawnOffset;

					if (particles.spawnShape == ParticleSpawnShape.Circle || particles.spawnShape == ParticleSpawnShape.Sphere)
					{
						ImGui.TextUnformatted("Spawn Radius");
						ImGui.SameLine(SPACING_X);
						ImGui.SetNextItemWidth(ITEM_WIDTH);
						float spawnRadius = particles.spawnRadius;
						if (ImGui.DragFloat("##spawn_radius" + i, &spawnRadius, 0.01f, 0, 10))
							particles.spawnRadius = spawnRadius;
					}
					else if (particles.spawnShape == ParticleSpawnShape.Line)
					{
						ImGui.TextUnformatted("Line End");
						ImGui.SameLine(SPACING_X);
						ImGui.SetNextItemWidth(ITEM_WIDTH);
						Vector3 lineEnd = particles.lineEnd;
						if (ImGui.DragFloat3("##line_end" + i, &lineEnd, 0.005f))
							particles.lineEnd = lineEnd;
					}

					ImGui.TextUnformatted("Random Rotation");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					byte randomStartRotation = (byte)(particles.randomStartRotation ? 1 : 0);
					if (ImGui.Checkbox("##random_start_rotation" + i, &randomStartRotation))
						particles.randomStartRotation = randomStartRotation != 0;

					ImGui.TextUnformatted("Follow");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					byte follow = (byte)(particles.follow ? 1 : 0);
					if (ImGui.Checkbox("##follow" + i, &follow))
						particles.follow = follow != 0;

					ImGui.TextUnformatted("Gravity");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float gravity = particles.gravity;
					if (ImGui.DragFloat("##gravity" + i, &gravity, 0.02f))
						particles.gravity = gravity;

					ImGui.TextUnformatted("Initial Velocity");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					Vector3 initialVelocity = particles.initialVelocity;
					if (ImGui.DragFloat3("##initial_velocity" + i, &initialVelocity, 0.02f))
						particles.initialVelocity = initialVelocity;

					ImGui.TextUnformatted("Rotation Speed");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float rotationSpeed = particles.rotationSpeed;
					if (ImGui.DragFloat("##rotation_speed" + i, &rotationSpeed, 0.02f))
						particles.rotationSpeed = rotationSpeed;

					if (FileSelect("Texture Atlas", "particle_atlas" + i, ref particles.textureAtlasPath, "png"))
					{
						particles.reload();
						instance.notifyEdit();
					}

					if (particles.textureAtlasPath != null)
					{
						ImGui.TextUnformatted("Atlas Size");
						ImGui.SameLine(SPACING_X);
						ImGui.SetNextItemWidth(ITEM_WIDTH);
						Vector2i atlasSize = particles.atlasSize;
						if (ImGui.DragInt2("##atlas_size" + i, &atlasSize, 0.04f, 1, 10000))
							particles.atlasSize = atlasSize;

						ImGui.TextUnformatted("Frame Count");
						ImGui.SameLine(SPACING_X);
						ImGui.SetNextItemWidth(ITEM_WIDTH);
						int numFrames = particles.numFrames;
						if (ImGui.DragInt("##frame_count" + i, &numFrames, 0.1f, 1, 100))
							particles.numFrames = numFrames;

						ImGui.TextUnformatted("Linear Filtering");
						ImGui.SameLine(SPACING_X);
						ImGui.SetNextItemWidth(ITEM_WIDTH);
						byte linearFiltering = (byte)(particles.linearFiltering ? 1 : 0);
						if (ImGui.Checkbox("##linear_filtering" + i, &linearFiltering))
							particles.linearFiltering = linearFiltering != 0;
					}

					ImGui.TextUnformatted("Color");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					Vector4 color = particles.spriteTint;
					if (ImGui.ColorEdit4("##color" + i, &color, ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.NoInputs))
						particles.spriteTint = color;

					ImGui.TextUnformatted("Additive");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					byte additive = (byte)(particles.additive ? 1 : 0);
					if (ImGui.Checkbox("##additive" + i, &additive))
						particles.additive = additive != 0;

					ImGui.TextUnformatted("Randomize Velocity");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float randomVelocity = particles.randomVelocity;
					if (ImGui.DragFloat("##random_velocity" + i, &randomVelocity, 0.001f, 0, 100))
						particles.randomVelocity = randomVelocity;

					ImGui.TextUnformatted("Randomize Rotation");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float randomRotationSpeed = particles.randomRotationSpeed * 180 / MathF.PI;
					if (ImGui.DragFloat("##random_rotation_speed" + i, &randomRotationSpeed, 4.0f, 0, 10000))
						particles.randomRotationSpeed = randomRotationSpeed / 180 * MathF.PI;

					ImGui.TextUnformatted("Randomize Lifetime");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float randomLifetime = particles.randomLifetime;
					if (ImGui.DragFloat("##random_lifetime" + i, &randomLifetime, 0.002f, 0, 1))
						particles.randomLifetime = randomLifetime;


					ImGui.SetNextItemAllowOverlap();
					bool animSizeSettings = ImGui.TreeNodeEx("Animate Size##particle_size_anim_settings" + i);
					ImGui.SameLine(SPACING_X);
					byte animateSize = (byte)(particles.particleSizeAnim != null ? 1 : 0);
					ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, Vector2.Zero);
					if (ImGui.Checkbox("##particle_size_anim" + i, &animateSize))
						particles.particleSizeAnim = animateSize != 0 ? new Gradient<float>(0.1f, 0.0f) : null;
					ImGui.PopStyleVar();
					if (animSizeSettings)
					{
						if (particles.particleSizeAnim != null)
						{
							ImGui.TextUnformatted("From");
							ImGui.SameLine();
							ImGui.SetNextItemWidth(100);
							float startValue = particles.particleSizeAnim.getValue(0);
							if (ImGui.DragFloat("##particle_size_anim_start" + i, &startValue, 0.001f, 0, 10))
								particles.particleSizeAnim.setValue(0, startValue);

							ImGui.SameLine();
							ImGui.TextUnformatted("to");
							ImGui.SameLine();
							ImGui.SetNextItemWidth(100);
							float endValue = particles.particleSizeAnim.getValue(1);
							if (ImGui.DragFloat("##particle_size_anim_end" + i, &endValue, 0.001f, 0, 10))
								particles.particleSizeAnim.setValue(1, endValue);
						}

						ImGui.TreePop();
					}

					ImGui.SetNextItemAllowOverlap();
					bool animColorSettings = ImGui.TreeNodeEx("Animate Color##particle_color_anim_settings" + i);
					ImGui.SameLine(SPACING_X);
					byte animateColor = (byte)(particles.colorAnim != null ? 1 : 0);
					ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, Vector2.Zero);
					if (ImGui.Checkbox("##particle_color_anim" + i, &animateColor))
						particles.colorAnim = animateColor != 0 ? new Gradient<Vector4>(new Vector4(1.0f), new Vector4(0.5f, 0.5f, 0.5f, 1.0f)) : null;
					ImGui.PopStyleVar();
					if (animColorSettings)
					{
						if (particles.colorAnim != null)
						{
							ImGui.TextUnformatted("From");
							ImGui.SameLine();
							Vector4 startValue = particles.colorAnim.getValue(0);
							if (ImGui.ColorEdit4("##particle_color_anim_start" + i, &startValue, ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.NoInputs))
								particles.colorAnim.setValue(0, startValue);

							ImGui.SameLine();
							ImGui.TextUnformatted("to");
							ImGui.SameLine();
							Vector4 endValue = particles.colorAnim.getValue(1);
							if (ImGui.ColorEdit4("##particle_color_anim_end" + i, &endValue, ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.NoInputs))
								particles.colorAnim.setValue(1, endValue);
						}

						ImGui.TreePop();
					}

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

				if (!particles.Equals(entity.particles[i]))
				{
					entity.particles[i].copyData(particles);
					instance.notifyEdit();
				}

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
