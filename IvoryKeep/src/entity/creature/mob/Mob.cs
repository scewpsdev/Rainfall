using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


public struct MobItemDrop
{
	public Item item;
	public int amount;
	public float dropChance;
}

public class Mob : Creature
{
	const int AI_UPDATE_RATE = 5;

	const float HEALTHBAR_SHOW_DURATION = 5.0f;
	const float HEALTHBAR_DMG_INDICATOR_DURATION = 2.0f;


	public readonly MobActionQueue actions;
	public Vector3 fsu;
	public Vector3 rotationTarget = Vector3.Zero;
	public bool running = false;
	int lastStep;

	public const float runSpeed = 2.0f;
	public const float walkSpeed = 1.0f;
	public const float maxRotationSpeed = 2.0f;
	Vector3 velocity;
	float rotationVelocity;

	Vector3 lastRootMotionPosition = Vector3.Zero;
	Quaternion lastRootMotionRotation = Quaternion.Identity;
	AnimationState lastRootMotionAnim = null;

	public readonly MobStats stats = new MobStats();

	public AI ai = null;

	public Item rightHandItem;
	Item leftHandItem;

	Node rightWeaponNode;
	Node leftWeaponNode;

	public Matrix rightWeaponTransform;

	Item blockingItem;

	List<MobItemDrop> itemDrops = new List<MobItemDrop>();

	AnimationState idleAnim;
	AnimationState runAnim;
	AnimationState actionAnim1, actionAnim2;
	AnimationState currentActionAnim;

	Ragdoll ragdoll;
	Animator ragdollAnimator;
	protected bool spawnRagdoll = true;

	bool renderHealthbar = false;
	Font healthbarFont;
	long lastHitTime = -10000000000;
	float lastHealth;


	public Mob(EntityType type)
		: base(type)
	{
		body?.lockRotationAxis(true, true, true);

		actions = new MobActionQueue(this);

		if (type.health != 0)
			stats.maxHealth = stats.health = type.health;
		if (type.poise != 0)
			stats.maxPoise = stats.poise = type.poise;

		if (type.ai == "hostile")
		{
			ai = HostileAI.Create(this);
		}

		rightHandItem = type.rightHandItem;
		leftHandItem = type.leftHandItem;

		if (type.file.getArray("itemDrops", out DatArray items))
		{
			for (int i = 0; i < items.size; i++)
			{
				MobItemDrop itemDrop = new MobItemDrop();
				if (items[i].obj.getIdentifier("item", out string itemName))
					itemDrop.item = Item.Get(itemName);
				if (items[i].obj.getInteger("amount", out int amount))
					itemDrop.amount = amount;
				else
					itemDrop.amount = 1;
				if (items[i].obj.getNumber("dropChance", out float dropChance))
					itemDrop.dropChance = dropChance;
				else
					itemDrop.dropChance = 1.0f;
				itemDrops.Add(itemDrop);
			}
		}

		rightWeaponNode = model.skeleton.getNode("Weapon.R");
		leftWeaponNode = model.skeleton.getNode("Weapon.L");

		healthbarFont = FontManager.GetFont("default", 18, true);
	}

	public override void init()
	{
		base.init();

		idleAnim = Animator.CreateAnimation(model, "idle", true);
		runAnim = Animator.CreateAnimation(model, "run", true);
		runAnim.layers[0].rootMotion = true;
		runAnim.layers[0].rootMotionNode = rootMotionNode;

		actionAnim1 = Animator.CreateAnimation(model, "default", false, 0.1f);
		actionAnim2 = Animator.CreateAnimation(model, "default", false, 0.1f);
	}

	public override void destroy()
	{
		base.destroy();

		if (ragdoll != null)
		{
			ragdoll.destroy();
			ragdoll = null;
		}
		if (ragdollAnimator != null)
		{
			Animator.Destroy(ragdollAnimator);
			ragdollAnimator = null;
		}

		HostileAI.Destroy((HostileAI)ai);
	}

	public override bool hit(float damage, float poiseDamage, Entity from, Item item, Vector3 hitPosition, Vector3 hitDirection, RigidBody hitbox)
	{
		if (stats.isDead)
		{
			if (hitbox != null)
			{
				Vector3 force = hitDirection * 4;
				hitbox.addForce(force);
			}
			return false;
		}

		Node hitNode = getHitboxNode(hitbox);
		bool criticalHit = hitNode != null && hitNode.name.IndexOf("head", StringComparison.OrdinalIgnoreCase) != -1;

		if (blockingItem != null)
		{
			float damageMultiplier = blockingItem.getAbsorptionDamageModifier();
			damage = damage * damageMultiplier;
		}
		else
		{
			float damageMultiplier = 1.0f;

			if (criticalHit && item != null)
				damageMultiplier += item.getCriticalDamageModifier();

			damage = damage * damageMultiplier;
		}

		if ((Time.currentTime - lastHitTime) / 1e9f > HEALTHBAR_DMG_INDICATOR_DURATION)
			lastHealth = stats.health;
		lastHitTime = Time.currentTime;

		stats.damage(damage);
		stats.poiseDamage(poiseDamage);

		if (hitSound != null)
			Audio.PlayOrganic(hitSound, hitPosition);

		if (stats.isDead)
		{
			float knockbackForce = poiseDamage / stats.maxPoise * 0.7f;
			if (criticalHit)
				knockbackForce *= 2;
			onDeath(hitDirection * knockbackForce, hitbox);

			if (from is Player)
				((Player)from).onEnemyKill(this);
		}
		else
		{
			if (blockingItem != null)
			{
				/*
				Debug.Assert(blockingHand != -1);
				float staminaCost = damage * (1 - item.blockStability / 100.0f);
				actions.queueAction(new BlockHitAction(item, blockingHand, staminaCost));
				*/
			}
			else
			{
				if (stats.poise < -0.5f * stats.maxPoise)
				{
					actions.cancelAllActions();
					actions.queueAction(new MobStaggerAction(StaggerType.Long, this));
					stats.poise = stats.maxPoise;
				}
				else if (stats.poise <= 0)
				{
					actions.cancelAllActions();
					actions.queueAction(new MobStaggerAction(criticalHit ? StaggerType.Headshot : StaggerType.Short, this));
					stats.poise = stats.maxPoise;
				}
			}

			ai?.onHit(from);
		}

		return criticalHit;
	}

	void onDeath(Vector3 knockback, RigidBody hitbox)
	{
		RigidBody getRagdollBodyForHitbox(RigidBody hitbox)
		{
			foreach (string nodeName in hitboxes.Keys)
			{
				if (hitboxes[nodeName] == hitbox)
				{
					return ragdoll.getHitboxForNode(model.skeleton.getNode(nodeName));
				}
			}
			return null;
		}

		for (int i = 0; i < itemDrops.Count; i++)
		{
			float r = Random.Shared.NextSingle();
			if (r < itemDrops[i].dropChance)
			{
				//ItemPickup drop = new ItemPickup(itemDrops[i].item, itemDrops[i].amount);
				//Quaternion rotation = Quaternion.FromAxisAngle(MathHelper.RandomVector3(-1, 1).normalized, MathHelper.RandomFloat(0, 2 * MathF.PI));
				//GameState.instance.level.addEntity(drop, position + Vector3.Up, rotation);
			}
		}

		if (hitboxes != null)
		{
			if (model != null && model.isAnimated && animator != null && spawnRagdoll && hitboxData != null)
			{
				Vector3 localVelocity = Quaternion.FromAxisAngle(Vector3.Up, yaw).conjugated * velocity;
				ragdoll = new Ragdoll(this, model.skeleton.getNode("Root"), animator, getModelMatrix(), localVelocity, hitboxData, PhysicsFiltering.RAGDOLL, PhysicsFiltering.DEFAULT | PhysicsFiltering.RAGDOLL | PhysicsFiltering.CREATURE);
				RigidBody hitBody = getRagdollBodyForHitbox(hitbox);
				if (hitBody != null)
				{
					hitBody.addForce(knockback);
				}

				ragdollAnimator = animator;
				animator = null;
			}
		}

		base.onDeath();
	}

	public override bool isAlive()
	{
		return !stats.isDead;
	}

	void updateMovement()
	{
		float movementSpeed = running ? runSpeed : walkSpeed;
		float rotationSpeed = maxRotationSpeed;
		velocity.xz = Vector2.Zero;
		rotationVelocity = 0.0f;

		if (actions.currentAction != null)
		{
			/*
			velocity += rootMotionVelocity; // currentActionAnim.layers[0].rootMotionDisplacement / Time.deltaTime;
			if (MathF.Abs(rootMotionRotationVelocity.angle) > 0.001f)
			{
				rotationVelocity += rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z);
				Console.WriteLine(rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z));
			}
			*/

			movementSpeed *= actions.currentAction.movementSpeedMultiplier;
			rotationSpeed *= actions.currentAction.rotationSpeedMultiplier;
		}

		animator.getRootMotion(out Vector3 rootMotionPosition, out Quaternion rootMotionRotation, out bool hasLooped);

		if (animator.currentAnimation != lastRootMotionAnim || hasLooped)
		{
			lastRootMotionPosition = Vector3.Zero;
			lastRootMotionRotation = Quaternion.Identity;
			lastRootMotionAnim = animator.currentAnimation;
		}

		Vector3 displacement = rootMotionPosition - lastRootMotionPosition;
		Quaternion rotationDisplacement = lastRootMotionRotation.conjugated * rootMotionRotation;
		velocity += displacement / Time.deltaTime;
		rotationVelocity += rotationDisplacement.eulers.y / Time.deltaTime;

		lastRootMotionPosition = rootMotionPosition;
		lastRootMotionRotation = rootMotionRotation;
		lastRootMotionAnim = animator.currentAnimation;

		velocity += fsu * new Vector3(1, 1, -1) * movementSpeed;
		velocity = Quaternion.FromAxisAngle(Vector3.Up, yaw) * velocity;

		bool isGrounded = Physics.Raycast(position + Vector3.Up * 0.5f, Vector3.Down, 0.6f) != null;

		if (!isGrounded)
			velocity.y += -10 * Time.deltaTime;

		if (body != null)
		{
			position = body.getPosition();
			body.setVelocity(velocity);
		}

		if (isGrounded)
		{
			if (animator.currentAnimation == runAnim)
			{
				int currentStep = (int)MathF.Floor(animator.timer / runAnim.layers[0].duration * 2);
				if (currentStep != lastStep)
				{
					if (stepSound != null)
					{
						Audio.PlayOrganic(stepSound, position, 0.05f, 1, 0.2f, 0.25f, 50);
					}
					lastStep = currentStep;
				}
			}
		}

		if (rotationTarget != Vector3.Zero)
		{
			Vector3 toTargetLocal = rotation.conjugated * rotationTarget;
			int rotationDirection = toTargetLocal.x > 0.1f ? -1 : toTargetLocal.x < -0.1f ? 1 : toTargetLocal.z > 0 ? -1 : 0;
			rotationVelocity += rotationDirection * rotationSpeed;
		}
		yaw += rotationVelocity * Time.deltaTime;
		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);
	}

	void updateAnimations()
	{
		// Update animations
		if (actions.currentAction != null)
			animator.setAnimation(currentActionAnim);
		else
		{
			float currentSpeed = velocity.length;
			if (currentSpeed > 0.4f * runSpeed)
				animator.setAnimation(runAnim);
			else
				animator.setAnimation(idleAnim);
		}

		//animator.update();
		animator.applyAnimation();
	}

	public override void update()
	{
		Matrix transform = getModelMatrix();

		if (!stats.isDead)
		{
			/*
			if ((Time.currentTime - lastAIUpdate) / 1e9f > 1.0f / AI_UPDATE_RATE)
			{
				ai?.update(1.0f / AI_UPDATE_RATE);
				lastAIUpdate = Time.currentTime;
			}
			*/

			updateMovement();

			if (rightWeaponNode != null)
				rightWeaponTransform = transform * Matrix.CreateRotation(Vector3.Up, MathF.PI) * animator.getNodeTransform(rightWeaponNode) * Matrix.CreateRotation(Vector3.UnitZ, MathF.PI); // * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);

			actions.update();

			updateAnimations();

			updateBoneHitbox(model.skeleton.rootNode, transform * animator.getNodeLocalTransform(model.skeleton.rootNode));
		}

		for (int i = 0; i < particles.Count; i++)
			particles[i].setTransform(transform);

		if (ragdoll != null)
		{
			ragdoll.update();
			ragdoll.getTransform().decompose(out position, out rotation);
		}

		if ((Time.currentTime - lastHitTime) / 1e9f > HEALTHBAR_DMG_INDICATOR_DURATION && lastHealth != stats.health)
			lastHealth += Math.Sign(stats.health - lastHealth) * Math.Min(40, Math.Abs(stats.health - lastHealth));
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();

		if (ragdoll != null)
			Renderer.DrawModel(model, transform, ragdollAnimator);
		else
		{
			base.draw(graphics);

			if (rightHandItem != null)
			{
				if (rightHandItem.entity != null)
					Renderer.DrawModel(rightHandItem.entity.Value.model, rightWeaponTransform);
			}

			if (renderHealthbar && (Time.currentTime - lastHitTime) / 1e9f < HEALTHBAR_SHOW_DURATION)
			{
				int width = 120;
				int height = 6;
				Vector2i center = MathHelper.WorldToScreenSpace(position + Vector3.Up * 2, Renderer.pv, Display.viewportSize);
				GUI.Rect(center.x - width / 2, center.y - height / 2, width, height, 0xFF333333);
				GUI.Rect(center.x - width / 2, center.y - height / 2, (int)(lastHealth / (float)stats.maxHealth * width), height, 0xFFDDA84B);
				GUI.Rect(center.x - width / 2, center.y - height / 2, (int)(stats.health / (float)stats.maxHealth * width), height, 0xFFFF3C2B);

				if ((Time.currentTime - lastHitTime) / 1e9f < HEALTHBAR_DMG_INDICATOR_DURATION)
					GUI.Text(center.x - width / 2, center.y - height / 2 - 3 - (int)healthbarFont.size, 1.0f, (lastHealth - stats.health).ToString(), healthbarFont, 0xFFBBBBBB);
			}
		}
	}

	public AnimationState getNextActionAnimationState()
	{
		currentActionAnim = currentActionAnim == actionAnim1 ? actionAnim2 : currentActionAnim == actionAnim2 ? actionAnim1 : actionAnim1;
		return currentActionAnim;
	}
}
