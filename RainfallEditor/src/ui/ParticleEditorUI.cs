using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public partial class EditorUI
{
	static byte[] renamingParticlesBuffer = new byte[256];


	static unsafe void Particles(Entity entity, EditorInstance instance)
	{
		if (ImGui.TreeNodeEx("Particles", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen))
		{
			for (int i = 0; entity.particles != null && i < entity.particles.Length; i++)
			{
				//ParticleSystemData particles = entity.data.particles[i];
				ParticleSystemData* particles = entity.particles[i].handle;

				Vector2 topRight = ImGui.GetCursorPos();

				bool particlesOpen = ImGui.TreeNodeEx("##particles" + i, ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanAvailWidth);

				StringUtils.WriteString(renamingParticlesBuffer, particles->name, StringUtils.StringLength(particles->name));
				ImGui.SameLine(54);
				ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, new Vector2(0.0f));
				fixed (byte* newNamePtr = renamingParticlesBuffer)
				{
					if (ImGui.InputText("##particle_rename" + i, newNamePtr, (ulong)renamingParticlesBuffer.Length, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
					{
						StringUtils.WriteString(particles->name, new string((sbyte*)newNamePtr));
						instance.notifyEdit();
					}
				}
				ImGui.PopStyleVar();

				if (particlesOpen)
				{
					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					DragFloat(instance, "Lifetime", "particle_lifetime" + i, ref particles->lifetime, 0.02f, 0, 100);

					/*
					ImGui.TextUnformatted("Lifetime");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float lifetime = particles->lifetime;
					if (ImGui.DragFloat("##lifetime" + i, &lifetime, 0.02f, 0, 100))
						particles->lifetime = lifetime;
					*/

					DragFloat(instance, "Size", "particle_size" + i, ref particles->size, 0.001f, 0, 10);

					/*
					ImGui.TextUnformatted("Size");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float particleSize = particles->size;
					if (ImGui.DragFloat("##size" + i, &particleSize, 0.001f, 0, 10))
						particles->size = particleSize;
					*/

					bool follow = particles->follow != 0;
					Checkbox(instance, "Follow", "particle_follow" + i, ref follow);
					particles->follow = (byte)(follow ? 1 : 0);

					//ImGui.TextUnformatted("Follow");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//byte follow = (byte)(particles->follow ? 1 : 0);
					//if (ImGui.Checkbox("##follow" + i, &follow))
					//	particles->follow = follow != 0;

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					DragFloat(instance, "Emission Rate", "particle_emission_rate" + i, ref particles->emissionRate, 0.2f, 0, 1000);

					/*
					ImGui.TextUnformatted("Emission Rate");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					float emissionRate = particles->emissionRate;
					if (ImGui.DragFloat("##emission_rate" + i, &emissionRate, 0.2f, 0, 1000))
						particles->emissionRate = emissionRate;
					*/

					Combo(instance, "Spawn Shape", "particle_spawn_shape" + i, ref particles->spawnShape, ImGuiComboFlags.HeightSmall);

					/*
					ImGui.TextUnformatted("Spawn Shape");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					if (ImGui.BeginCombo("##shawn_shape" + i, particles->spawnShape.ToString(), ImGuiComboFlags.HeightSmall))
					{
						ParticleSpawnShape spawnShape = particles->spawnShape;

						if (ImGui.Selectable_Bool("Point"))
							spawnShape = ParticleSpawnShape.Point;
						if (ImGui.Selectable_Bool("Circle"))
							spawnShape = ParticleSpawnShape.Circle;
						if (ImGui.Selectable_Bool("Sphere"))
							spawnShape = ParticleSpawnShape.Sphere;
						if (ImGui.Selectable_Bool("Line"))
							spawnShape = ParticleSpawnShape.Line;

						if (spawnShape != particles->spawnShape)
						{
							particles->spawnShape = spawnShape;
							instance.notifyEdit();
						}

						ImGui.EndCombo();
					}
					*/

					if (particles->spawnShape == ParticleSpawnShape.Circle || particles->spawnShape == ParticleSpawnShape.Sphere || particles->spawnShape == ParticleSpawnShape.Line)
					{
						DragFloat(instance, "Spawn Radius", "particle_spawn_radius" + i, ref particles->spawnRadius, 0.01f, 0, 10);

						/*
						ImGui.TextUnformatted("Spawn Radius");
						ImGui.SameLine(SPACING_X);
						ImGui.SetNextItemWidth(ITEM_WIDTH);
						float spawnRadius = particles->spawnRadius;
						if (ImGui.DragFloat("##spawn_radius" + i, &spawnRadius, 0.01f, 0, 10))
							particles->spawnRadius = spawnRadius;
						*/
					}
					if (particles->spawnShape == ParticleSpawnShape.Line)
					{
						DragFloat3(instance, "Line End", "particle_line_end" + i, ref particles->lineSpawnEnd, 0.005f);

						/*
						ImGui.TextUnformatted("Line End");
						ImGui.SameLine(SPACING_X);
						ImGui.SetNextItemWidth(ITEM_WIDTH);
						Vector3 lineEnd = particles->lineEnd;
						if (ImGui.DragFloat3("##line_end" + i, &lineEnd, 0.005f))
							particles->lineEnd = lineEnd;
						*/
					}

					DragFloat3(instance, "Spawn Offset", "particle_spawn_offset" + i, ref particles->spawnOffset, 0.005f);

					/*
					ImGui.TextUnformatted("Spawn Offset");
					ImGui.SameLine(SPACING_X);
					ImGui.SetNextItemWidth(ITEM_WIDTH);
					Vector3 spawnOffset = particles->spawnOffset;
					if (ImGui.DragFloat3("##spawn_offset" + i, &spawnOffset, 0.005f))
						particles->spawnOffset = spawnOffset;
					*/

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					DragFloat(instance, "Gravity", "particle_gravity" + i, ref particles->gravity, 0.02f);

					//ImGui.TextUnformatted("Gravity");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float gravity = particles->gravity;
					//if (ImGui.DragFloat("##gravity" + i, &gravity, 0.02f))
					//	particles->gravity = gravity;

					DragFloat(instance, "Drag", "particle_drag" + i, ref particles->drag, 0.0001f, 0, 1);

					DragFloat3(instance, "Start Velocity", "particle_start_velocity" + i, ref particles->startVelocity, 0.02f);

					//ImGui.TextUnformatted("Start Velocity");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//Vector3 startVelocity = particles->startVelocity;
					//if (ImGui.DragFloat3("##start_velocity" + i, &startVelocity, 0.02f))
					//	particles->startVelocity = startVelocity;

					DragFloat(instance, "Radial Velocity", "particle_radial_velocity" + i, ref particles->radialVelocity, 0.02f);

					DragAngle(instance, "Start Rotation", "particle_start_rotation" + i, ref particles->startRotation, 1, 0, 360);

					DragFloat(instance, "Rotation Speed", "particle_rotation_speed" + i, ref particles->rotationSpeed, 0.02f);

					bool rotateAlongMovement = particles->rotateAlongMovement;
					Checkbox(instance, "Rotate Along Movement", "particle_rotate_along_movement" + i, ref rotateAlongMovement);
					particles->rotateAlongMovement = rotateAlongMovement;

					if (rotateAlongMovement)
					{
						DragFloat(instance, "Movement Stretch", "particle_movement_stretch" + i, ref particles->movementStretch, 0.01f, -10, 10);
					}

					//ImGui.TextUnformatted("Rotation Speed");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float rotationSpeed = particles->rotationSpeed;
					//if (ImGui.DragFloat("##rotation_speed" + i, &rotationSpeed, 0.02f))
					//	particles->rotationSpeed = rotationSpeed;

					bool applyEntityVelocity = particles->applyEntityVelocity;
					Checkbox(instance, "Apply Entity Velocity", "particle_apply_entity_velocity" + i, ref applyEntityVelocity);
					particles->applyEntityVelocity = applyEntityVelocity;

					bool applyCentrifugalForce = particles->applyCentrifugalForce;
					Checkbox(instance, "Apply Centrifugal Force", "particle_apply_centrifugal_force" + i, ref applyCentrifugalForce);
					particles->applyCentrifugalForce = applyCentrifugalForce;

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					string textureAtlasPath = new string((sbyte*)particles->textureAtlasPath);
					if (FileSelect("Texture Atlas", "particle_atlas" + i, ref textureAtlasPath, "png"))
					{
						StringUtils.WriteString(particles->textureAtlasPath, textureAtlasPath != null ? textureAtlasPath : "");
						if (textureAtlasPath != null)
							particles->textureAtlas = Resource.GetTexture(RainfallEditor.CompileAsset(textureAtlasPath)).handle;
						else
							particles->textureAtlas = ushort.MaxValue;

						instance.notifyEdit();
					}

					if (particles->textureAtlasPath != null)
					{
						DragInt2(instance, "Atlas Size", "particle_atlas_size" + i, ref particles->atlasSize, 0.04f, 1, 10000);

						//ImGui.TextUnformatted("Atlas Size");
						//ImGui.SameLine(SPACING_X);
						//ImGui.SetNextItemWidth(ITEM_WIDTH);
						//Vector2i atlasSize = particles->atlasSize;
						//if (ImGui.DragInt2("##atlas_size" + i, &atlasSize, 0.04f, 1, 10000))
						//	particles->atlasSize = atlasSize;

						DragInt(instance, "Frame Count", "particle_frame_count" + i, ref particles->numFrames, 0.1f, 1, 100);

						//ImGui.TextUnformatted("Frame Count");
						//ImGui.SameLine(SPACING_X);
						//ImGui.SetNextItemWidth(ITEM_WIDTH);
						//int numFrames = particles->numFrames;
						//if (ImGui.DragInt("##frame_count" + i, &numFrames, 0.1f, 1, 100))
						//	particles->numFrames = numFrames;

						bool randomFrame = particles->randomFrame != 0;
						Checkbox(instance, "Random Frame", "random_frame" + i, ref randomFrame);
						particles->randomFrame = (byte)(randomFrame ? 1 : 0);

						bool linearFiltering = particles->linearFiltering != 0;
						Checkbox(instance, "Linear Filtering", "particle_linear_filtering" + i, ref linearFiltering);
						particles->linearFiltering = (byte)(linearFiltering ? 1 : 0);

						//ImGui.TextUnformatted("Linear Filtering");
						//ImGui.SameLine(SPACING_X);
						//ImGui.SetNextItemWidth(ITEM_WIDTH);
						//byte linearFiltering = (byte)(particles->linearFiltering ? 1 : 0);
						//if (ImGui.Checkbox("##linear_filtering" + i, &linearFiltering))
						//	particles->linearFiltering = linearFiltering != 0;
					}

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					ColorEdit4(instance, "Color", "particle_color" + i, ref particles->color, true);

					//ImGui.TextUnformatted("Color");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//Vector4 color = particles->color;
					//if (ImGui.ColorEdit4("##color" + i, &color, ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.NoInputs))
					//	particles->color = color;

					bool additive = particles->additive;
					Checkbox(instance, "Additive", "particle_additive" + i, ref additive);
					particles->additive = additive;

					//ImGui.TextUnformatted("Additive");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//byte additive = (byte)(particles->additive ? 1 : 0);
					//if (ImGui.Checkbox("##additive" + i, &additive))
					//	particles->additive = additive != 0;

					DragFloat(instance, "Emissive Intensity", "particle_emissive_intensity" + i, ref particles->emissiveIntensity, 0.005f, 0, 100);

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					DragFloat3(instance, "Randomize Velocity", "particle_random_velocity" + i, ref particles->randomVelocity, 0.001f, 0, 100);

					//ImGui.TextUnformatted("Randomize Velocity");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float randomVelocity = particles->randomVelocity;
					//if (ImGui.DragFloat("##random_velocity" + i, &randomVelocity, 0.001f, 0, 100))
					//	particles->randomVelocity = randomVelocity;

					DragFloat(instance, "Randomize Rotation", "particle_random_rotation" + i, ref particles->randomRotation, 0.002f, 0, 1);

					DragAngle(instance, "Randomize Rotation Speed", "particle_random_rotation_speed" + i, ref particles->randomRotationSpeed, 1.0f, 0, 10000);

					//ImGui.TextUnformatted("Randomize Rotation");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float randomRotationSpeed = particles->randomRotationSpeed * 180 / MathF.PI;
					//if (ImGui.DragFloat("##random_rotation_speed" + i, &randomRotationSpeed, 4.0f, 0, 10000))
					//	particles->randomRotationSpeed = randomRotationSpeed / 180 * MathF.PI;

					DragFloat(instance, "Randomize Lifetime", "particle_random_lifetime" + i, ref particles->randomLifetime, 0.002f, 0, 1);

					DragFloat(instance, "Velocity Noise", "particle_velocity_noise" + i, ref particles->velocityNoise, 0.002f, 0, 10);

					//ImGui.TextUnformatted("Randomize Lifetime");
					//ImGui.SameLine(SPACING_X);
					//ImGui.SetNextItemWidth(ITEM_WIDTH);
					//float randomLifetime = particles->randomLifetime;
					//if (ImGui.DragFloat("##random_lifetime" + i, &randomLifetime, 0.002f, 0, 1))
					//	particles->randomLifetime = randomLifetime;

					ImGui.Spacing();
					ImGui.Spacing();
					ImGui.Spacing();

					bool animateSizeEnabled = particles->sizeAnim.count > 0;
					bool animateSizeOpen = TreeNodeOptional(instance, "Animate Size", "particle_size_anim" + i, ref animateSizeEnabled);
					if (animateSizeEnabled != (particles->sizeAnim.count > 0))
					{
						if (animateSizeEnabled)
							particles->sizeAnim = new Gradient_float_3 { value0 = new Gradient_float_3.Value { value = 0.1f, position = 0 }, value1 = new Gradient_float_3.Value { value = 0.05f, position = 0.5f }, value2 = new Gradient_float_3.Value { value = 0.0f, position = 1 }, count = 3 };
						else
							particles->sizeAnim = new Gradient_float_3 { count = 0 };
					}
					if (animateSizeOpen)
					{
						if (particles->sizeAnim.count > 0)
						{
							float value0 = particles->sizeAnim.value0.value;
							if (DragFloat(instance, "Size 0", "particle_size_anim0" + i, ref value0, 0.001f, 0, 10))
								particles->sizeAnim.value0 = new Gradient_float_3.Value { value = value0, position = 0 };

							float value1 = particles->sizeAnim.value1.value;
							if (DragFloat(instance, "Size 1", "particle_size_anim1" + i, ref value1, 0.001f, 0, 10))
								particles->sizeAnim.value1 = new Gradient_float_3.Value { value = value1, position = 0.5f };

							float value2 = particles->sizeAnim.value2.value;
							if (DragFloat(instance, "Size 2", "particle_size_anim2" + i, ref value2, 0.001f, 0, 10))
								particles->sizeAnim.value2 = new Gradient_float_3.Value { value = value2, position = 1 };

							particles->sizeAnim.count = 3;
						}

						ImGui.TreePop();
					}

					//ImGui.SetNextItemAllowOverlap();
					//bool animSizeSettings = ImGui.TreeNodeEx("Animate Size##particle_size_anim_settings" + i);
					//ImGui.SameLine(SPACING_X);
					//byte animateSize = (byte)(particles->sizeAnim != null ? 1 : 0);
					//ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, Vector2.Zero);
					//if (ImGui.Checkbox("##particle_size_anim" + i, &animateSize))
					//	particles->sizeAnim = animateSize != 0 ? new Gradient<float>(0.1f, 0.0f) : null;
					//ImGui.PopStyleVar();
					//if (animSizeSettings)
					//{
					//	if (particles->sizeAnim != null)
					//	{
					//		ImGui.TextUnformatted("From");
					//		ImGui.SameLine();
					//		ImGui.SetNextItemWidth(100);
					//		float startValue = particles->sizeAnim.getValue(0);
					//		if (ImGui.DragFloat("##particle_size_anim_start" + i, &startValue, 0.001f, 0, 10))
					//			particles->sizeAnim.setValue(0, startValue);

					//		ImGui.SameLine();
					//		ImGui.TextUnformatted("to");
					//		ImGui.SameLine();
					//		ImGui.SetNextItemWidth(100);
					//		float endValue = particles->sizeAnim.getValue(1);
					//		if (ImGui.DragFloat("##particle_size_anim_end" + i, &endValue, 0.001f, 0, 10))
					//			particles->sizeAnim.setValue(1, endValue);
					//	}

					//	ImGui.TreePop();
					//}

					bool animateColorEnabled = particles->colorAnim.count > 0;
					bool animateColorOpen = TreeNodeOptional(instance, "Animate Color", "particle_color_anim" + i, ref animateColorEnabled);
					if (animateColorEnabled != (particles->colorAnim.count > 0))
					{
						if (animateColorEnabled)
							particles->colorAnim = new Gradient_Vector4_3 { value0 = new Gradient_Vector4_3.Value { value = new Vector4(1.0f), position = 0 }, value1 = new Gradient_Vector4_3.Value { value = new Vector4(1.0f), position = 0.5f }, value2 = new Gradient_Vector4_3.Value { value = new Vector4(1.0f), position = 1 }, count = 3 };
						else
							particles->colorAnim = new Gradient_Vector4_3 { count = 0 };
					}
					if (animateColorOpen)
					{
						if (particles->colorAnim.count > 0)
						{
							Vector4 value0 = particles->colorAnim.value0.value;
							if (ColorEdit4(instance, "Color 0", "particle_color_anim0" + i, ref value0, true))
								particles->colorAnim.value0 = new Gradient_Vector4_3.Value { value = value0, position = 0 };

							Vector4 value1 = particles->colorAnim.value1.value;
							if (ColorEdit4(instance, "Color 1", "particle_color_anim1" + i, ref value1, true))
								particles->colorAnim.value1 = new Gradient_Vector4_3.Value { value = value1, position = 0.5f };

							Vector4 value2 = particles->colorAnim.value2.value;
							if (ColorEdit4(instance, "Color 2", "particle_color_anim2" + i, ref value2, true))
								particles->colorAnim.value2 = new Gradient_Vector4_3.Value { value = value2, position = 1.0f };

							particles->colorAnim.count = 3;
						}

						ImGui.TreePop();
					}

					//ImGui.SetNextItemAllowOverlap();
					//bool animColorSettings = ImGui.TreeNodeEx("Animate Color##particle_color_anim_settings" + i);
					//ImGui.SameLine(SPACING_X);
					//byte animateColor = (byte)(particles->colorAnim != null ? 1 : 0);
					//ImGui.PushStyleVar_Vec2(ImGuiStyleVar.FramePadding, Vector2.Zero);
					//if (ImGui.Checkbox("##particle_color_anim" + i, &animateColor))
					//	particles->colorAnim = animateColor != 0 ? new Gradient<Vector4>(new Vector4(1.0f), new Vector4(0.5f, 0.5f, 0.5f, 1.0f)) : null;
					//ImGui.PopStyleVar();
					//if (animColorSettings)
					//{
					//	if (particles->colorAnim != null)
					//	{
					//		ImGui.TextUnformatted("From");
					//		ImGui.SameLine();
					//		Vector4 startValue = particles->colorAnim.getValue(0);
					//		if (ImGui.ColorEdit4("##particle_color_anim_start" + i, &startValue, ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.NoInputs))
					//			particles->colorAnim.setValue(0, startValue);

					//		ImGui.SameLine();
					//		ImGui.TextUnformatted("to");
					//		ImGui.SameLine();
					//		Vector4 endValue = particles->colorAnim.getValue(1);
					//		if (ImGui.ColorEdit4("##particle_color_anim_end" + i, &endValue, ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.NoInputs))
					//			particles->colorAnim.setValue(1, endValue);
					//	}

					//	ImGui.TreePop();
					//}

					bool burstsEnabled = particles->bursts != null;
					bool burstsOpen = TreeNodeOptional(instance, "Bursts", "particle_bursts" + i, ref burstsEnabled);
					if (burstsEnabled != (particles->bursts != null))
						particles->bursts = burstsEnabled ? ((ParticleBurst*)Marshal.AllocHGlobal(sizeof(ParticleBurst) * particles->numBursts)) : null;
					if (burstsOpen)
					{
						if (particles->bursts != null)
						{
							for (int j = 0; j < particles->numBursts; j++)
							{
								ParticleBurst burst = particles->bursts[j];

								DragFloat(instance, "Time", "particle_bursts" + i + "_time" + j, ref burst.time, 0.01f, 0, 100);
								DragInt(instance, "Count", "particle_bursts" + i + "_count" + j, ref burst.count, 0.2f, 0, 1000);
								DragFloat(instance, "Duration", "particle_bursts" + i + "_duration" + j, ref burst.duration, 0.002f, 0, 100);

								particles->bursts[j] = burst;

								ImGui.Separator();
							}

							if (ImGui.Button("Add Burst##particle_bursts" + i + "_add"))
							{
								ParticleBurst burst = new ParticleBurst { time = 0.0f, count = 5, duration = 0 };
								ParticleBurst* oldBuffer = particles->bursts;
								particles->bursts = (ParticleBurst*)Marshal.AllocHGlobal(sizeof(ParticleBurst) * (particles->numBursts + 1));
								Unsafe.CopyBlock(oldBuffer, particles->bursts, (uint)(particles->numBursts * sizeof(ParticleBurst)));
								Marshal.FreeHGlobal((IntPtr)oldBuffer);
								particles->numBursts++;
								instance.notifyEdit();
							}
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
					ParticleSystem.Destroy(entity.particles[i]);
					entity.particles = ArrayUtils.RemoveAt(entity.particles, i--);
					//entity.data.particles->RemoveAt(i--);
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
				//ParticleSystemData particles = new ParticleSystemData(0) { transform = entity.getModelMatrix() };
				ParticleSystem particles = ParticleSystem.Create(entity.getModelMatrix());
				entity.particles = ArrayUtils.Add(entity.particles, particles);
				//entity.data.particles->Add(particles);
				instance.notifyEdit();
			}

			ImGui.TreePop();
		}
	}
}
