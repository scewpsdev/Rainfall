using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ParticleEffect : Entity
{
	public Entity follow;
	public Vector2 offset;

	public ParticleSystem[] systems;
	public Texture[] textureAtlases;
	public bool collision = false;
	public float bounciness = 0;

	float[] emissionRates;

	public float layer = LAYER_BG;


	public unsafe ParticleEffect(Entity follow, string file)
	{
		this.follow = follow;

		FileStream stream = new FileStream(Resource.ASSET_DIRECTORY + "/" + file + ".bin", FileMode.Open);
		SceneFormat.DeserializeScene(stream, out List<SceneFormat.EntityData> entities, out uint selectedEntity);
		stream.Close();

		SceneFormat.EntityData entityData = entities[0];

		systems = new ParticleSystem[entityData.particles.Length];
		textureAtlases = new Texture[entityData.particles.Length];
		emissionRates = new float[entityData.particles.Length];
		for (int i = 0; i < entityData.particles.Length; i++)
		{
			ParticleSystem system = ParticleSystem.Create(Matrix.Identity, 250);
			system.setData(entityData.particles[i]);
			systems[i] = system;

			if (system.handle->textureAtlasPath[0] != 0)
				textureAtlases[i] = Resource.GetTexture(StringUtils.AbsolutePath(new string((sbyte*)system.handle->textureAtlasPath), file), false);

			emissionRates[i] = system.handle->emissionRate;
		}
	}

	public override void init(Level level)
	{
		if (follow != null)
			offset = position - follow.position;
		for (int i = 0; i < systems.Length; i++)
			systems[i].setTransform(Matrix.CreateTranslation(position.x, position.y, 0), true);
	}

	public override void destroy()
	{
		for (int i = 0; i < systems.Length; i++)
			ParticleSystem.Destroy(systems[i]);
	}

	public override unsafe void update()
	{
		if (follow != null)
		{
			position = follow.position + offset;
			rotation = follow.rotation;
		}

		bool hasFinished = true;
		for (int i = 0; i < systems.Length; i++)
		{
			systems[i].setTransform(Matrix.CreateTranslation(position.x, position.y, 0) * Matrix.CreateRotation(Vector3.UnitZ, rotation), true);
			systems[i].update();

			if (collision)
			{
				ParticleData* systemData = systems[i].data;
				for (int j = 0; j < systems[i].numParticles; j++)
				{
					ParticleData particle = systemData[j];
					if (particle.active)
					{
						if (GameState.instance.level.hitTiles(particle.position.xy) != null)
						{
							if (bounciness > 0)
							{
								if (MathF.Abs(particle.velocity.x) > MathF.Abs(particle.velocity.y))
									particle.velocity.x *= -bounciness;
								else
								{
									particle.velocity.y *= -bounciness;
									particle.velocity.x *= bounciness;
								}
							}
							else
							{
								particle.active = false;
							}
						}
					}
					systemData[j] = particle;
				}
			}

			if (!systems[i].hasFinished)
				hasFinished = false;
		}

		if (hasFinished)
			remove();
	}

	public override unsafe void render()
	{
		for (int j = 0; j < systems.Length; j++)
		{
			ParticleData* systemData = systems[j].data;

			for (int i = 0; i < systems[j].numParticles; i++)
			{
				ParticleData particle = systemData[i];
				if (particle.active)
				{
					float size = MathF.Floor(particle.size * 16) / 16.0f;
					size = MathF.Max(size, 1.0f / 16);
					int u0 = 0, v0 = 0, w = 1, h = 1;
					if (textureAtlases[j] != null)
					{
						int frameIdx = (int)(particle.animationFrame * systems[j].handle->atlasSize.x * systems[j].handle->atlasSize.y);
						w = textureAtlases[j].width / systems[j].handle->atlasSize.x;
						h = textureAtlases[j].height / systems[j].handle->atlasSize.y;
						u0 = (frameIdx % systems[j].handle->atlasSize.x) * w;
						v0 = (frameIdx / systems[j].handle->atlasSize.x) * h;
					}
					Renderer.DrawSprite(particle.position.x - 0.5f * size, particle.position.y - 0.5f * size, layer, size, size, particle.rotation, textureAtlases[j], u0, v0, w, h, particle.color, systems[j].handle->additive);
				}
			}
		}
	}
}
