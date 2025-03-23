using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Creature : Entity, Hittable
{
	const float HEALTHBAR_SHOW_DURATION = 5.0f;
	const float HEALTHBAR_DMG_INDICATOR_DURATION = 3.0f;


	public int health = 100;
	public int maxHealth = 100;

	public float yaw = 0;
	public Vector3 velocity = Vector3.Zero;
	Vector3 rootMotionVelocity = Vector3.Zero;

	bool renderHealthbar = true;
	Font healthbarFont;
	long lastHitTime = -1;
	float lastHealth;

	public Node rightWeaponNode;

	public Node rootMotionNode;
	Vector3 lastRootMotionDisplacement;
	CreatureAction lastRootMotionAction;

	AnimationState idleAnim;

	AnimationState actionAnim1, actionAnim2;
	AnimationState currentActionAnim;

	public CreatureActionManager actionManager;

	public List<CreatureAttack> attacks = new List<CreatureAttack>();

	public AI ai;
	long lastAITick1 = -1, lastAITick10 = -1;

	public Sound[] slashSound, stabSound;


	public Creature(string name)
	{
		this.name = name;

		hitboxFilterGroup = PhysicsFilter.CreatureHitbox;
		hitboxFilterMask = 0;
		load($"entity/creature/{name}/{name}.rfs", PhysicsFilter.Creature, PhysicsFilter.Default | PhysicsFilter.PlayerHitbox);
		body.lockRotationAxis(true, true, true);

		rootMotionNode = model.skeleton.getNode("root");
		rightWeaponNode = model.skeleton.getNode("weapon.R");

		animator = Animator.Create(model);

		idleAnim = Animator.CreateAnimation(model, "idle", true, 0.4f);
		idleAnim.animationSpeed = 0.005f;

		actionAnim1 = Animator.CreateAnimation(model, "default", false, 0.1f);
		actionAnim2 = Animator.CreateAnimation(model, "default", false, 0.1f);

		actionManager = new CreatureActionManager(this);

		slashSound = Resource.GetSounds("audio/hit_slash", 2);
		stabSound = Resource.GetSounds("audio/hit_stab", 2);

		healthbarFont = FontManager.GetFont("default", 18, true);
	}

	public void addAttack(CreatureAttack attack)
	{
		attacks.Add(attack);
	}

	public void hit(int damage, bool criticalHit, Vector3 hitDirection, Entity by, Item item, RigidBody hitbox)
	{
		lastHitTime = Time.currentTime;

		health -= damage;

		if (health <= 0)
		{
			death();

			float knockbackForce = /*poiseDamage / stats.maxPoise **/ 0.7f;
			if (criticalHit)
				knockbackForce *= 2;
			Vector3 knockback = hitDirection * knockbackForce;

			GameState.instance.scene.addEntity(new CreatureRagdoll(this, knockback, hitbox), getModelMatrix());

			remove();
		}
		else
		{
			actionManager.cancelAllActions();
			actionManager.queueAction(new CreatureStaggerAction());
		}
	}

	public virtual void death()
	{
	}

	void updateMovement()
	{
		Vector3 velocity = rootMotionVelocity;
		body.setVelocity(velocity);

		body.setRotation(Quaternion.FromAxisAngle(Vector3.Up, yaw));
	}

	void updateActions()
	{
		if (ai != null)
		{
			ai.update();
			if ((Time.currentTime - lastAITick10) / 1e9f >= 0.1f || lastAITick10 == -1)
			{
				ai.tick10();
				lastAITick10 = Time.currentTime;
			}
			if ((Time.currentTime - lastAITick1) / 1e9f >= 1 || lastAITick1 == -1)
			{
				ai.tick1();
				lastAITick1 = Time.currentTime;
			}
		}

		actionManager.update();

		if (actionManager.currentAction != null && currentActionAnim.layers[0].rootMotion)
		{
			Vector3 displacement = currentActionAnim.layers[0].rootMotionDisplacement.translation;
			if (lastRootMotionAction == actionManager.currentAction && !currentActionAnim.layers[0].hasLooped)
			{
				rootMotionVelocity = (displacement - lastRootMotionDisplacement) / Time.deltaTime;
				rootMotionVelocity = rotation * rootMotionVelocity;
				if (rootMotionVelocity.length > 20)
					Debug.Assert(false);
			}
			lastRootMotionDisplacement = displacement;
			lastRootMotionAction = actionManager.currentAction;
			//velocity += displacement.translation / Time.deltaTime;
			//if (MathF.Abs(rootMotionRotationVelocity.angle) > 0.001f)
			//{
			//	rotationVelocity += rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z);
			//	Console.WriteLine(rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z));
			//}
		}
		else
		{
			rootMotionVelocity = Vector3.Zero;
			lastRootMotionDisplacement = Vector3.Zero;
			lastRootMotionAction = null;
		}

		if (lastHitTime == -1 || (Time.currentTime - lastHitTime) / 1e9f > HEALTHBAR_DMG_INDICATOR_DURATION)
			lastHealth = health;
	}

	void updateAnimations()
	{
		Matrix transform = getModelMatrix();

		if (body != null)
		{
			if (body.type == RigidBodyType.Dynamic)
				body.getTransform(out position, out rotation);
			else if (body.type == RigidBodyType.Kinematic)
				body.setTransform(position, rotation);
		}

		if (actionManager.currentAction != null)
		{
			animator.setAnimation(currentActionAnim);
		}
		else
		{
			animator.setAnimation(idleAnim);
		}

		animator.applyAnimation();

		if (hitboxes != null && model != null && animator != null)
			updateBoneHitbox(model.skeleton.rootNode, transform * animator.getNodeLocalTransform(model.skeleton.rootNode));

		for (int i = 0; i < particles.Count; i++)
		{
			//if (Renderer.IsInFrustum(particles[i].boundingSphere.center, particles[i].boundingSphere.radius, transform, Renderer.pv))
			particles[i].setTransform(transform);
		}
	}

	public override void update()
	{
		updateMovement();
		updateActions();
		updateAnimations();
	}

	public override void fixedUpdate(float delta)
	{
		if (ai != null)
		{
			ai.fixedUpdate(delta);
		}
	}

	public AnimationState getNextActionAnimationState()
	{
		currentActionAnim = currentActionAnim == actionAnim1 ? actionAnim2 : currentActionAnim == actionAnim2 ? actionAnim1 : actionAnim1;
		return currentActionAnim;
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		Renderer.DrawModel(model, transform * modelTransform, animator, isStatic);

		if (renderHealthbar && lastHitTime != -1 && (Time.currentTime - lastHitTime) / 1e9f < HEALTHBAR_SHOW_DURATION)
		{
			int width = 120;
			int height = 6;
			Vector2i center = MathHelper.WorldToScreenSpace(position + Vector3.Up * 1.7f, Renderer.pv, Display.viewportSize);
			GUI.Rect(center.x - width / 2, center.y - height / 2, width, height, 0xFF333333);
			GUI.Rect(center.x - width / 2, center.y - height / 2, (int)(lastHealth / (float)maxHealth * width), height, 0xFFDDA84B);
			GUI.Rect(center.x - width / 2, center.y - height / 2, (int)(health / (float)maxHealth * width), height, 0xFFFF3C2B);

			if ((Time.currentTime - lastHitTime) / 1e9f < HEALTHBAR_DMG_INDICATOR_DURATION)
				GUI.Text(center.x - width / 2, center.y - height / 2 - 3 - (int)healthbarFont.size, 1.0f, (lastHealth - health).ToString(), healthbarFont, 0xFFBBBBBB);
		}
	}
}
