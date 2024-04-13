using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

internal class SkeletonEnemy : Creature
{
	public SkeletonEnemy()
	{
		model = Resource.GetModel("res/entity/creature/skeleton/skeleton.gltf");
		rootNode = model.skeleton.getNode("Root");
		rightWeaponNode = model.skeleton.getNode("handHold.R");
		leftWeaponNode = model.skeleton.getNode("handHold.L");
		animator = new Animator(model);
		idleState = new AnimationState(model, new AnimationLayer[] { new AnimationLayer(model, "idle", true) }, 0.2f);
		runState = new AnimationState(model, new AnimationLayer[] { new AnimationLayer(model, "run", true) }, 0.2f);
		deadState = new AnimationState(model, new AnimationLayer[] { new AnimationLayer(model, "death", false) }, 0.2f);
		actionState1 = new AnimationState(model, new AnimationLayer[] { new AnimationLayer(model, "default", false) }, 0.2f);
		actionState2 = new AnimationState(model, new AnimationLayer[] { new AnimationLayer(model, "default", false) }, 0.2f);

		animator.setState(idleState);

		hitParticles = new ParticleSystem(64);
		hitParticles.spawnOffset = new Vector3(0.0f, 0.8f, 0.0f);
		hitParticles.spawnShape = ParticleSpawnShape.Circle;
		hitParticles.spawnRadius = 0.25f;
		hitParticles.emissionRate = 0.0f;
		hitParticles.lifetime = 1.0f;
		hitParticles.initialVelocity = Vector3.Zero;
		hitParticles.spriteTint = 0xff220000;
		hitParticles.particleSize = 0.5f;
		hitParticles.textureAtlas = Resource.GetTexture("res/texture/particle/blood.png");
		hitParticles.atlasColumns = 1;
		hitParticles.frameWidth = 256;
		hitParticles.frameHeight = 256;
		hitParticles.numFrames = 1;

		stats.maxHealth = 200;
		stats.health = 200;

		//nameTag = "Jerry";

		walkSpeed = 3.0f;

		behavior = new HostileBehavior(this);

		hitSound = Resource.GetSound("res/entity/creature/skeleton/sfx/hit.ogg");


		setItem(0, Item.Get("shortsword"));
	}

	public override void init()
	{
		base.init();

		body = new RigidBody(this, RigidBodyType.Dynamic);
		body.addCapsuleCollider(0.3f, 2.0f, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.Identity, 0.0f);
		body.lockRotationAxis(true, true, true);

		yaw = 0.0f;
	}

	protected override void onDeath()
	{
		float dropChance = 0.25f;
		if (Random.Shared.NextSingle() < dropChance)
		{
			ItemPickup item = new ItemPickup(Item.Get("quemick"));
			OntoNewHorizons.instance.world.addEntity(item, position + new Vector3(0.0f, 1.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.One.normalized, Random.Shared.NextSingle() * MathF.PI * 2.0f));
		}
	}
}
