using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class UIParticleEffect : Entity
{
	public Entity follow;
	public Vector2 offset;

	public ParticleSystem[] systems;
	public Texture[] textureAtlases;


	public unsafe UIParticleEffect(Entity follow, string file)
	{
		this.follow = follow;

		string str = Resource.GetText(file);
		SceneFormat.DeserializeScene(str, out List<SceneFormat.EntityData> entities, out uint selectedEntity);

		SceneFormat.EntityData entityData = entities[0];

		systems = new ParticleSystem[entityData.particles.Length];
		textureAtlases = new Texture[entityData.particles.Length];
		for (int i = 0; i < entityData.particles.Length; i++)
		{
			ParticleSystem system = ParticleSystem.Create(Matrix.Identity, 250);
			system.setData(entityData.particles[i]);
			systems[i] = system;

			if (system.handle->textureAtlasPath[0] != 0)
				textureAtlases[i] = Resource.GetTexture(StringUtils.AbsolutePath(new string((sbyte*)system.handle->textureAtlasPath), file), false);
		}
	}

	public override void init(Level level)
	{
		if (follow != null)
			offset = position - follow.position;
		for (int i = 0; i < systems.Length; i++)
			systems[i].setTransform(Matrix.CreateTranslation(position.x / 16.0f, (Renderer.UIHeight - position.y - 1) / 16.0f, 0), true);
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
			systems[i].setTransform(Matrix.CreateTranslation(position.x / 16.0f, (Renderer.UIHeight - position.y - 1) / 16.0f, 0) * Matrix.CreateRotation(Vector3.UnitZ, rotation), true);
			systems[i].update();

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
					float size = particle.size / 0.1f;
					size = MathF.Max(size, 1);
					int u0 = 0, v0 = 0, w = 1, h = 1;
					if (textureAtlases[j] != null)
					{
						int frameIdx = (int)particle.animationFrame;
						w = textureAtlases[j].width / systems[j].handle->atlasSize.x;
						h = textureAtlases[j].height / systems[j].handle->atlasSize.y;
						u0 = (frameIdx % systems[j].handle->atlasSize.x) * w;
						v0 = (frameIdx / systems[j].handle->atlasSize.x) * h;
					}
					Renderer.DrawUISprite((int)(particle.position.x * 16), (int)(Renderer.UIHeight - particle.position.y * 16 - 1), (int)size, (int)size, textureAtlases[j], u0, v0, w, h, MathHelper.VectorToARGB(particle.color));
				}
			}
		}
	}
}
