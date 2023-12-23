using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


enum TorchState
{
	Off,
	Glimming,
	Burning,
	Looted,
}

internal class WallTorch : Entity, Interactable
{
	static readonly Vector3 FIRE_COLOR = MathHelper.SRGBToLinear(0.965f, 0.604f, 0.329f);
	static readonly Vector3 GLIM_COLOR = MathHelper.SRGBToLinear(0.965f, 0.604f * 0.8f, 0.329f * 0.8f);

	static readonly Vector3 ORIGIN = new Vector3(0.0f, 0.265f, 0.25f);


	Model model;
	RigidBody body;
	ParticleSystem fireParticles, glimParticles, sparkParticles, smokeParticles;

	AudioSource audio;
	Sound igniteSound;

	Simplex flickerNoise;

	public TorchState state;


	public WallTorch(TorchState state = TorchState.Burning)
	{
		this.state = state;

		model = Resource.GetModel("res/entity/object/wall_torch/wall_torch.gltf");

		fireParticles = new ParticleSystem(250);
		//fireParticles.textureAtlas = Resource.GetTexture("res/texture/particle/torch_flame.png");
		//fireParticles.atlasColumns = 4;
		//fireParticles.frameWidth = 32;
		//fireParticles.frameHeight = 32;
		//fireParticles.numFrames = 12;
		fireParticles.emissionRate = 120.0f;
		fireParticles.lifetime = 0.5f;
		fireParticles.spawnOffset = ORIGIN;
		fireParticles.spawnRadius = 0.02f;
		fireParticles.spawnShape = ParticleSpawnShape.Sphere;
		//fireParticles.particleSize = 0.1f;
		fireParticles.particleSizeAnim = new Gradient<float>(0.1f, 0.02f);
		fireParticles.randomRotation = true;
		fireParticles.randomRotationSpeed = true;
		fireParticles.randomVelocity = true;
		fireParticles.randomVelocityMultiplier = 0.1f;
		fireParticles.gravity = 2.0f;
		fireParticles.additive = true;
		//fireParticles.spriteTint = new Vector4(, 1.0f);
		fireParticles.colorAnim = new Gradient<Vector4>(new Vector4(GLIM_COLOR, 1.0f), new Vector4(FIRE_COLOR * 3, 1.0f));

		glimParticles = new ParticleSystem(64);
		glimParticles.emissionRate = 10;
		glimParticles.lifetime = 8;
		glimParticles.spawnOffset = ORIGIN;
		glimParticles.spawnRadius = 0.06f;
		glimParticles.spawnShape = ParticleSpawnShape.Sphere;
		glimParticles.particleSize = 0.02f;
		glimParticles.gravity = 0.0f;
		glimParticles.follow = true;
		glimParticles.additive = true;
		glimParticles.spriteTint = new Vector4(GLIM_COLOR * 3, 1.0f);
		glimParticles.randomRotation = true;

		sparkParticles = new ParticleSystem(64);
		sparkParticles.emissionRate = 0.3f;
		sparkParticles.lifetime = 3;
		sparkParticles.spawnOffset = ORIGIN;
		sparkParticles.particleSize = 0.02f;
		sparkParticles.gravity = -5.0f;
		sparkParticles.additive = true;
		sparkParticles.spriteTint = new Vector4(GLIM_COLOR * 1.5f, 1.0f);
		sparkParticles.randomRotation = true;
		sparkParticles.randomVelocity = true;
		sparkParticles.randomVelocityMultiplier = 3.0f;

		smokeParticles = new ParticleSystem(32);
		smokeParticles.emissionRate = 0;

		igniteSound = Resource.GetSound("res/item/utility/torch/sfx/ignite.ogg");

		flickerNoise = new Simplex((uint)Time.currentTime, 3);
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static, (uint)(PhysicsFilterGroup.Default | PhysicsFilterGroup.Interactable));
		body.addCapsuleCollider(0.1f, 0.6f, new Vector3(0.0f, 0.1f, 0.1f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.ToRadians(19.0f)));

		audio = Audio.CreateSource(position);
	}

	public override void update()
	{
		base.update();

		Matrix transform = getModelMatrix();

		if (state == TorchState.Burning || state == TorchState.Glimming)
		{
			glimParticles.transform = transform;
			glimParticles.update();

			sparkParticles.transform = transform;
			sparkParticles.update();

			if (state == TorchState.Burning)
			{
				fireParticles.transform = transform;
				fireParticles.update();

				smokeParticles.transform = transform;
				smokeParticles.update();
			}
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();

		Renderer.DrawSubModelStaticInstanced(model, 1, transform);

		if (state != TorchState.Looted)
		{
			Renderer.DrawSubModel(model, 0, transform);

			if (state == TorchState.Burning || state == TorchState.Glimming)
			{
				glimParticles.draw(graphics);
				sparkParticles.draw(graphics);

				if (state == TorchState.Burning)
				{
					fireParticles.draw(graphics);
					smokeParticles.draw(graphics);
				}

				Vector3 lightPosition = transform * new Vector3(0.0f, 0.3f, 0.25f);
				Vector3 lightColor = state == TorchState.Burning ? FIRE_COLOR * 8.0f : GLIM_COLOR * 1.5f;

				float lightFlicker = 1.0f + 0.5f * flickerNoise.sample1f(Time.currentTime / 1e9f);
				lightColor *= lightFlicker;

				Renderer.DrawLight(lightPosition, lightColor);
			}
		}
	}

	public bool canInteract(Entity by)
	{
		return state != TorchState.Looted || (by is Player && ((Player)by).inventory.hasItemEquipped(Item.Get("torch")));
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = by as Player;

			if (state == TorchState.Off || state == TorchState.Glimming)
			{
				state = TorchState.Burning;
				audio.playSound(igniteSound);
			}
			else if (state == TorchState.Burning)
			{
				player.giveItem(Item.Get("torch"), 1);
				//fireParticles = null;
				state = TorchState.Looted;
				// TODO grab sound
			}
			else if (state == TorchState.Looted)
			{
				Debug.Assert(player.inventory.hasItemEquipped(Item.Get("torch"), out ItemSlot torchSlot));
				player.inventory.removeItem(torchSlot);
				state = TorchState.Burning;
				audio.playSound(igniteSound);
				// TODO place sound
			}
		}
	}
}
