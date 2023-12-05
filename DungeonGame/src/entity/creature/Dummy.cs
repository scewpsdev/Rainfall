using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Dummy : Creature
{
	public Dummy()
	{
		model = Resource.GetModel("res/entity/creature/dummy/dummy.gltf");
		animator = new Animator(model);
		idleState = new AnimationState(model, new AnimationLayer[] { new AnimationLayer(model, "idle", true) }, 0.2f);
		deadState = new AnimationState(model, new AnimationLayer[] { new AnimationLayer(model, "death", false) }, 0.2f);
		actionState1 = new AnimationState(model, new AnimationLayer[] { new AnimationLayer(model, "default", false) }, 0.2f);
		actionState2 = new AnimationState(model, new AnimationLayer[] { new AnimationLayer(model, "default", false) }, 0.2f);

		hitParticles = new ParticleSystem(250);
		hitParticles.spawnOffset = new Vector3(0.0f, 0.5f, 0.0f);
		hitParticles.textureAtlas = Resource.GetTexture("res/texture/particle/cloth.png");
		//hitParticles.atlasColumns = 1;
		hitParticles.frameWidth = 64;
		hitParticles.frameHeight = 64;
		hitParticles.numFrames = 1;
		hitParticles.emissionRate = 0.0f;
		//hitParticles.lifetime = 2.0f;
		hitParticles.initialVelocity = Vector3.Zero;

		stats.maxHealth = 10000;
		stats.health = 10000;

		hitSound = new Sound[] {
			Resource.GetSound("res/entity/creature/sfx/impact1.ogg"),
			Resource.GetSound("res/entity/creature/sfx/impact2.ogg"),
			Resource.GetSound("res/entity/creature/sfx/impact3.ogg"),
			Resource.GetSound("res/entity/creature/sfx/impact4.ogg"),
		};
	}

	protected override void onHit(int damage, Entity from, Vector3 force)
	{
		base.onHit(damage, from, force);

		clearActions();
		queueAction(new MobStaggerAction(MobActionType.StaggerShort));
	}

	public override void init()
	{
		base.init();

		movementBody.lockAxis(true, true, true);
		movementBody.addCapsuleCollider(0.3f, 1.8f, new Vector3(0.0f, 0.9f, 0.0f), Quaternion.Identity);
	}
}
