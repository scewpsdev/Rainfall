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

	bool consumed = false;
	float currentGain = DEFAULT_GAIN;


	public Fountain()
	{
		model = Resource.GetModel("res/entity/object/fountain/fountain.gltf");
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		body.addCapsuleCollider(1, 1.5f + 2 * 1, new Vector3(0, 0.75f, 0), Quaternion.Identity);

		audio = Audio.CreateSource(position);
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

		float lightIntensity = MathHelper.Remap(MathF.Sin(Time.currentTime / 1e9f * 1.0f), -1.0f, 1.0f, 1.0f, 3.0f);
		Renderer.DrawLight(position + new Vector3(0.0f, 0.1f, 0.0f), new Vector3(0.4f, 0.4f, 1.0f) * lightIntensity);

		particles.draw(graphics);
	}
}
