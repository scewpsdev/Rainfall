using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ParticleEffect : Entity
{
	Entity follow;
	Vector2 offset;

	public ParticleSystem system;
	public bool collision = false;


	public ParticleEffect(Entity follow, string file)
	{
		this.follow = follow;

		FileStream stream = new FileStream(file + ".bin", FileMode.Open);
		SceneFormat.DeserializeScene(stream, out List<SceneFormat.EntityData> entities, out uint selectedEntity);
		stream.Close();

		SceneFormat.EntityData entityData = entities[0];
		system = ParticleSystem.Create(Matrix.Identity, 100);
		system.setData(entityData.particles[0]);
	}

	public override void init()
	{
		if (follow != null)
			offset = position - follow.position;
		system.setTransform(Matrix.CreateTranslation(position.x, position.y, 0), true);
	}

	public override void destroy()
	{
		ParticleSystem.Destroy(system);
	}

	public override unsafe void update()
	{
		if (follow != null)
			position = follow.position + offset;
		system.setTransform(Matrix.CreateTranslation(position.x, position.y, 0), true);

		if (collision)
		{
			ParticleData* systemData = system.data;
			for (int i = 0; i < system.numParticles; i++)
			{
				ParticleData particle = systemData[i];
				if (particle.active)
				{
					if (GameState.instance.level.sampleTiles(particle.position.xy) != null)
						particle.active = false;
				}
				systemData[i] = particle;
			}
		}

		if (system.hasFinished)
			remove();
	}

	public override unsafe void render()
	{
		ParticleData* systemData = system.data;
		for (int i = 0; i < system.numParticles; i++)
		{
			ParticleData particle = systemData[i];
			if (particle.active)
				Renderer.DrawSprite(particle.position.x, particle.position.y, LAYER_BG, 1.0f / 16, 1.0f / 16, particle.rotation, null, false, particle.color);
		}
	}
}
