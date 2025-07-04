﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

internal class Jerry : Creature
{
	public Jerry()
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

		animator.setAnimation(idleState);

		hitParticles = new ParticleSystem(64);
		//hitParticles.spawnOffset = new Vector3(0.0f, 0.8f, 0.0f);
		hitParticles.spawnShape = ParticleSpawnShape.Circle;
		hitParticles.spawnRadius = 0.25f;
		hitParticles.emissionRate = 0.0f;
		hitParticles.lifetime = 0.5f;
		hitParticles.gravity = 0.0f;
		//hitParticles.startVelocity = Vector3.Zero;
		hitParticles.color = new Vector4(0.125f, 0.0f, 0.0f, 1.0f);
		hitParticles.size = 0.5f;
		hitParticles.textureAtlas = Resource.GetTexture("res/texture/particle/blood.png");
		hitParticles.sizeAnim = new Gradient<float>(0.05f, 0.8f);
		//hitParticles.atlasColumns = 1;
		//hitParticles.atlasSize = new Vector2i(1);
		//hitParticles.numFrames = 1;
		hitParticles.randomRotation = 1.0f;

		stats.maxHealth = 1000;
		stats.health = 1000;

		walkSpeed = 5.0f;

		name = "Jerry";

		hitboxData.Add("Hip", new BoneHitbox(0.1f));
		hitboxData.Add("Spine", new BoneHitbox(0.1f));
		hitboxData.Add("Chest", new BoneHitbox(0.1f));
		hitboxData.Add("Neck", new BoneHitbox(0.1f, 0.0f, -0.1f));
		//hitboxData.Add("Shoulder.R", new BoneHitbox(0.05f, 0.1f));
		//hitboxData.Add("Shoulder.L", new BoneHitbox(0.05f, 0.1f));
		hitboxData.Add("ArmUpper.R", new BoneHitbox(0.05f));
		hitboxData.Add("ArmUpper.L", new BoneHitbox(0.05f));
		hitboxData.Add("ArmLower.R", new BoneHitbox(0.04f, 0.0f, 0.05f));
		hitboxData.Add("ArmLower.L", new BoneHitbox(0.04f, 0.0f, 0.05f));
		hitboxData.Add("Hand.R", new BoneHitbox(0.05f));
		hitboxData.Add("Hand.L", new BoneHitbox(0.05f));
		hitboxData.Add("LegUpper.R", new BoneHitbox(0.06f, 0.05f, 0.05f));
		hitboxData.Add("LegUpper.L", new BoneHitbox(0.06f, 0.05f, 0.05f));
		hitboxData.Add("LegLower.R", new BoneHitbox(0.05f));
		hitboxData.Add("LegLower.L", new BoneHitbox(0.05f));

		ai = new EnemyAI(this);
		ai.detectionRange = 20.0f;
		ai.detectionAngle = MathHelper.ToRadians(360.0f);

		itemDrops.Add(new ItemDrop(Item.Get("flask").id, 1, 0.25f));

		hitSound = new Sound[] {
			Resource.GetSound("res/entity/creature/skeleton/sfx/hit.ogg")
		};


		setItem(0, Item.Get("shortsword"));
	}

	public override void init()
	{
		base.init();

		movementBody.addCapsuleCollider(0.3f, 2.0f, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.Identity, 0.0f);
	}
}
