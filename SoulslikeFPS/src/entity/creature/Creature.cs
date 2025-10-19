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

	public Vector3 fsu = Vector3.Zero;
	public float yaw = 0;
	public Vector3 rotationTarget = Vector3.Forward;
	float maxRotationSpeed = 2.0f;
	public Vector3 velocity = Vector3.Zero;
	float runSpeed = 2.0f;
	Vector3 rootMotionDelta = Vector3.Zero;

	bool renderHealthbar = true;
	Vector3 healthbarHeight = new Vector3(0, 1.6f, 0);
	Font healthbarFont;
	long lastHitTime = -1;
	float lastHealth;

	public Node rightWeaponNode;

	public Node rootMotionNode;
	Vector3 lastRootMotionDisplacement;
	float lastRootMotionAngle;
	CreatureAction lastRootMotionAction;
	AnimationState lastRootMotionAnim;

	public Matrix rightWeaponTransform;

	AnimationState idleAnim, runAnim;

	AnimationState actionAnim1, actionAnim2;
	AnimationState currentActionAnim;

	public CreatureActionManager actionManager;

	public List<CreatureAttack> attacks = new List<CreatureAttack>();
	public float weaponReach = 0;
	public float weaponRadius = 0;

	public AI ai;
	long lastAITick1 = -1, lastAITick10 = -1;

	public Sound[] slashSound, stabSound;


	public Creature(string name)
	{
		this.name = name;

		hitboxFilterGroup = PhysicsFilter.CreatureHitbox;
		hitboxFilterMask = 0;
		load($"entity/creature/{name}/{name}.rfs", PhysicsFilter.Creature, PhysicsFilter.Default);
		body.lockRotationAxis(true, true, true);

		rootMotionNode = model.skeleton.getNode("Root");
		rightWeaponNode = model.skeleton.getNode("Weapon.R");

		animator = Animator.Create(model);

		modelTransform = Matrix.CreateRotation(Vector3.Up, MathF.PI);

		idleAnim = Animator.CreateAnimation(model, "idle", true, 0.4f);
		idleAnim.animationSpeed = 0.005f;
		runAnim = Animator.CreateAnimation(model, "run", true, 0.4f);

		actionAnim1 = Animator.CreateAnimation(model, "default", false, 0.1f);
		actionAnim2 = Animator.CreateAnimation(model, "default", false, 0.1f);

		actionManager = new CreatureActionManager(this);

		slashSound = Resource.GetSounds("sound/hit/hit_slash", 2);
		stabSound = Resource.GetSounds("sound/hit/hit_stab", 2);

		healthbarFont = FontManager.GetFont("default", 18, true);
	}

	public override void init()
	{
		base.init();

		float scaleFactor = scale.max;
		body.scale(scaleFactor);
		if (hitboxes != null)
		{
			foreach (var pair in hitboxes)
			{
				pair.Value.scale(scaleFactor);
			}
		}
	}

	public void setHealth(int health)
	{
		this.health = health;
		this.maxHealth = health;
	}

	public void addAttack(CreatureAttack attack)
	{
		attacks.Add(attack);
	}

	public bool getAttack(string name, out int idx)
	{
		for (int i = 0; i < attacks.Count; i++)
		{
			if (attacks[i].name == name)
			{
				idx = i;
				return true;
			}
		}
		idx = -1;
		return false;
	}

	public void hit(HitData hit)
	{
		lastHitTime = Time.currentTime;

		Node hitboxNode = getHitboxNode(hit.hitbox);
		if (hitboxNode.name == "Head")
		{
			hit.damage *= 2;
			hit.critical = true;
		}
		if (actionManager.currentAction is CreatureStaggerAction)
		{
			hit.damage *= 2;
			hit.critical = true;
		}

		health -= hit.damage;

		ai?.onHit(hit.by);

		if (health <= 0)
		{
			death();

			float knockbackForce = /*poiseDamage / stats.maxPoise **/ 1.5f;
			if (hit.critical)
				knockbackForce *= 2;
			Vector3 knockback = hit.hitDirection * knockbackForce;

			GameState.instance.scene.addEntity(new CreatureRagdoll(this, "Hips", knockback, hit.hitbox), getModelMatrix());

			remove();
		}
		else
		{
			//stagger();
		}
	}

	public void stagger()
	{
		actionManager.cancelAllActions();
		actionManager.queueAction(new CreatureStaggerAction());
	}

	public virtual void death()
	{
	}

	void updateMovement()
	{
		float movementSpeed = 1.0f;
		float rotationSpeed = 1.0f;

		if (actionManager.currentAction != null)
		{
			/*
			velocity += rootMotionVelocity; // currentActionAnim.layers[0].rootMotionDisplacement / Time.deltaTime;
			if (MathF.Abs(rootMotionRotationVelocity.angle) > 0.001f)
			{
				rotationVelocity += rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z);
				Console.WriteLine(rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z));
			}
			*/

			movementSpeed *= actionManager.currentAction.movementSpeedMultiplier;
			rotationSpeed *= actionManager.currentAction.rotationSpeedMultiplier;
		}

		Vector3 moveVelocity = rotation * (fsu * new Vector3(1, 1, -1)) * runSpeed * movementSpeed;
		Vector3 velocity = moveVelocity + rootMotionDelta / Time.deltaTime;
		body.setVelocityX(velocity.x);
		body.setVelocityZ(velocity.z);

		float rotationVelocity = 0.0f;
		if (rotationTarget != Vector3.Zero)
		{
			Vector3 toTargetLocal = rotation.conjugated * rotationTarget;
			int rotationDirection = toTargetLocal.x > 0.1f ? -1 : toTargetLocal.x < -0.1f ? 1 : toTargetLocal.z > 0 ? -1 : 0;
			rotationVelocity = rotationDirection * maxRotationSpeed * rotationSpeed;
		}
		yaw += rotationVelocity * Time.deltaTime;
		body.setRotation(Quaternion.AxisAngle(Vector3.Up, yaw));
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
			//Vector3 displacement = currentActionAnim.layers[0].rootMotionDisplacement.translation;
			animator.getRootMotion(currentActionAnim, out Vector3 rootMotionDisplacement, out Quaternion rootMotionRotation, out bool hasLooped);
			float rootMotionAngle = rootMotionRotation.angle * MathF.Sign(rootMotionRotation.axis.y);
			if (lastRootMotionAction == actionManager.currentAction && lastRootMotionAnim == currentActionAnim && !hasLooped)
			{
				rootMotionDelta = rotation * Quaternion.AxisAngle(Vector3.Up, MathF.PI) * (rootMotionDisplacement - lastRootMotionDisplacement) /** actionManager.currentAction.rootMotionMultiplier*/;
				yaw += (rootMotionAngle - lastRootMotionAngle);
			}
			/*
			if (lastRootMotionAction == actionManager.currentAction && !currentActionAnim.layers[0].hasLooped)
			{
				rootMotionVelocity = (displacement - lastRootMotionDisplacement) / Time.deltaTime;
				rootMotionVelocity = rotation * Quaternion.AxisAngle(Vector3.Up, MathF.PI) * rootMotionVelocity;
				if (rootMotionVelocity.length > 20)
					Debug.Assert(false);
			}
			*/
			lastRootMotionDisplacement = rootMotionDisplacement;
			lastRootMotionAngle = rootMotionAngle;
			lastRootMotionAction = actionManager.currentAction;
			lastRootMotionAnim = currentActionAnim;
			//velocity += displacement.translation / Time.deltaTime;
			//if (MathF.Abs(rootMotionRotationVelocity.angle) > 0.001f)
			//{
			//	rotationVelocity += rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z);
			//	Console.WriteLine(rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z));
			//}
		}
		else
		{
			rootMotionDelta = Vector3.Zero;
			lastRootMotionDisplacement = Vector3.Zero;
			lastRootMotionAngle = 0;
			lastRootMotionAction = null;
			lastRootMotionAnim = null;
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
		else if (fsu.lengthSquared > 0)
		{
			animator.setAnimation(runAnim);
		}
		else
		{
			animator.setAnimation(idleAnim);
		}

		animator.applyAnimation();

		rightWeaponTransform = transform * modelTransform * animator.getNodeTransform(rightWeaponNode);

		if (hitboxes != null && model != null && animator != null)
			updateBoneHitbox(model.skeleton.rootNode, transform * Matrix.CreateRotation(Vector3.Up, MathF.PI) * animator.getNodeLocalTransform(model.skeleton.rootNode));

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
			ai.fixedUpdate(delta);

		if (actionManager.currentAction != null)
			actionManager.currentAction.fixedUpdate(this, delta);
	}

	public AnimationState getNextActionAnimationState()
	{
		currentActionAnim = currentActionAnim == actionAnim1 ? actionAnim2 : currentActionAnim == actionAnim2 ? actionAnim1 : actionAnim1;
		return currentActionAnim;
	}

	public override unsafe void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		for (int i = 0; i < model.meshCount; i++)
		{
			MeshData* mesh = model.getMeshData(i);
			string meshName = model.getMeshName(i);
			if (meshName == "Weapon")
				Renderer.DrawMesh(mesh, model.getMaterialData(i), rightWeaponTransform * Matrix.CreateRotation(Vector3.Right, MathF.PI * 0.5f), animator);
			else
				Renderer.DrawMesh(model, i, transform * modelTransform, animator);
		}

		if (renderHealthbar && lastHitTime != -1 && (Time.currentTime - lastHitTime) / 1e9f < HEALTHBAR_SHOW_DURATION)
		{
			int width = 120;
			int height = 6;
			Vector2i center = Mathf.WorldToScreenSpace(position + healthbarHeight, Renderer.pv, Display.viewportSize);
			GUI.Rect(center.x - width / 2, center.y - height / 2, width, height, 0xFF333333);
			GUI.Rect(center.x - width / 2, center.y - height / 2, (int)(lastHealth / (float)maxHealth * width), height, 0xFFDDA84B);
			GUI.Rect(center.x - width / 2, center.y - height / 2, (int)(health / (float)maxHealth * width), height, 0xFFFF3C2B);

			if ((Time.currentTime - lastHitTime) / 1e9f < HEALTHBAR_DMG_INDICATOR_DURATION)
				GUI.Text(center.x - width / 2, center.y - height / 2 - 3 - (int)healthbarFont.size, 1.0f, (lastHealth - health).ToString(), healthbarFont, 0xFFBBBBBB);
		}

		if (actionManager.currentAction != null)
			actionManager.currentAction.draw(this);
	}
}
