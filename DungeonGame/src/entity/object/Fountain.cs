using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Fountain : Entity, Interactable
{
	const float DEFAULT_GAIN = 0.1f;


	Model model;
	RigidBody body;
	AudioSource audio;
	ParticleSystem particles;
	PointLight light;

	bool consumed = false;
	float currentGain = DEFAULT_GAIN;


	public Fountain()
	{
		model = Resource.GetModel("res/entity/object/fountain/fountain.gltf");
		model.configureLODs(LOD.DISTANCE_MEDIUM);
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		body.addCapsuleCollider(1, 1.5f + 2 * 1, new Vector3(0, 0.75f, 0), Quaternion.Identity);

		audio = new AudioSource(position);
		Sound sound = Resource.GetSound("res/entity/object/fountain/sfx/fountain.ogg");
		audio.playSound(sound, DEFAULT_GAIN);

		particles = new ParticleSystem(32);
		particles.transform = getModelMatrix();
		particles.emissionRate = 1.5f;
		particles.lifetime = 5.0f;
		particles.spawnShape = ParticleSpawnShape.Circle;
		particles.spawnRadius = 1.0f;
		particles.spriteTint = new Vector4(0.5f, 0.675f, 1.0f, 1.0f);
		particles.particleSize = 0.02f;
		particles.gravity = 0.0f;
		particles.initialVelocity = new Vector3(0.0f, 1.0f, 0.0f);

		light = new PointLight(position + new Vector3(0.0f, 0.1f, 0.0f), Vector3.One, Renderer.graphics);
	}

	public override void destroy()
	{
		body.destroy();
		audio.destroy();
	}

	public bool canInteract(Entity by)
	{
		return !consumed;
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = by as Player;
			player.stats.addEffect(new HealEffect(100, 3.0f));
			consumed = true;
			currentGain = DEFAULT_GAIN * 20;
		}
	}

	public override void update()
	{
		currentGain = MathHelper.Lerp(currentGain, DEFAULT_GAIN, 1 * Time.deltaTime);
		audio.gain = currentGain;

		particles.update();
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix());

		particles.draw(graphics);

		float lightIntensity = MathHelper.Remap(MathF.Sin(Time.currentTime / 1e9f * 1.0f), -1.0f, 1.0f, 1.0f, 3.0f);
		light.color = new Vector3(0.4f, 0.4f, 1.0f) * lightIntensity;
		Renderer.DrawPointLight(light);

		//Renderer.DrawLight(light.position, light.color);
	}
}
