using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Creature : Entity
{
	internal class CreatureStats
	{
		public int maxHealth = 100;
		public int health = 100;

		public int damage = 20;

		Creature creature;


		public CreatureStats(Creature creature)
		{
			this.creature = creature;
		}

		public void applyDamage(int damage)
		{
			health = Math.Max(health - damage, 0);
		}
	}

	public enum CreatureState
	{
		Idle,
		Run,
		Dead,
	}


	const float GRAVITY = -10.0f;

	const float HEALTHBAR_SHOW_DURATION = 5.0f;

	const int MAX_ACTION_QUEUE_SIZE = 2;

	protected Model model;
	protected Node rootNode, rightWeaponNode, leftWeaponNode;
	protected Animator animator;
	protected AnimationState idleState, runState, deadState;
	protected AnimationState actionState1, actionState2;
	protected AnimationState currentActionState;

	public RigidBody body { get; protected set; }

	protected ParticleSystem hitParticles;

	protected string nameTag = null;

	protected AIBehavior behavior = null;

	public CreatureState state = CreatureState.Idle;
	public CreatureStats stats;

	float walkSpeed = 2.0f;
	float rotationSpeed = 3.0f;

	public Vector2 fsu = Vector2.Zero;
	public float rotationDirection = 0.0f;

	public float yaw = 0.0f;

	float verticalVelocity = 0.0f;
	bool isGrounded = false;

	AudioSource audio;
	protected Sound hitSound;

	Item[] handItem = new Item[2];
	MobItemEntity[] weaponEntities = new MobItemEntity[2];

	long lastDamagedTime = 0;
	Font healthbarTextFont;

	List<MobAction> actionQueue = new List<MobAction>();


	protected Creature()
	{
		stats = new CreatureStats(this);

		weaponEntities[0] = new MobItemEntity(this, 0);
		weaponEntities[1] = new MobItemEntity(this, 1);

		healthbarTextFont = Resource.GetFontData("res/fonts/libre-baskerville.regular.ttf").createFont(20.0f);
	}

	protected void setItem(int handID, Item item)
	{
		handItem[0] = item;
		weaponEntities[0].setItem(item);
	}

	public override void init()
	{
		yaw = rotation.angle;

		audio = Audio.CreateSource(position);
	}

	protected virtual void onHit(int damage, Entity from)
	{
		if (behavior != null)
			behavior.onHit(damage, from);
	}

	protected virtual void onDeath()
	{
	}

	public void hit(int damage, Entity from)
	{
		if (from is Creature)
			return;

		stats.applyDamage(damage);
		lastDamagedTime = Time.currentTime;

		if (stats.health == 0)
		{
			state = CreatureState.Dead;
			body.clearColliders();
			weaponEntities[0].hitbox.clearColliders();
			weaponEntities[1].hitbox.clearColliders();
			if (from is Player)
			{
				Player otherPlayer = (Player)from;
				otherPlayer.stats.awardXP(200);
			}
			onDeath();

			actionQueue.Clear();
		}
		else
		{
			actionQueue.Clear();
			queueAction(new MobStaggerAction(MobActionType.StaggerShort));
		}

		audio.playSoundOrganic(hitSound);

		if (hitParticles != null)
		{
			Vector3 hitDirection = (position - from.position).normalized;
			int numBloodParticles = MathHelper.RandomInt(10, 24);
			for (int i = 0; i < numBloodParticles; i++)
			{
				Vector3 particleDirection = -hitDirection.normalized * 2.0f;
				Vector3 randomVector = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()) * 2.0f - 1.0f;
				particleDirection += randomVector * Vector3.Dot(randomVector.normalized, particleDirection);
				hitParticles.emitParticle(particleDirection);
			}
		}

		onHit(damage, from);
	}

	void updateMovement()
	{
		if (state != CreatureState.Dead)
		{
			body.getTransform(out position, out Quaternion _);

			Vector3 displacement = Vector3.Zero;

			if (fsu.lengthSquared > 0.0001f)
			{
				Quaternion rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);
				Vector3 forward = rotation.forward;
				Vector3 right = rotation.right;
				Vector3 walkDirection = (fsu.x * right + fsu.y * forward).normalized;
				Vector3 velocity = walkDirection * walkSpeed;
				displacement += velocity * Time.deltaTime;
				state = CreatureState.Run;
			}
			else
			{
				state = CreatureState.Idle;
			}

			if (MathF.Abs(rotationDirection) > 0.0001f)
			{
				yaw += rotationDirection * rotationSpeed * Time.deltaTime;
			}


			// Root Motion
			if (/*isGrounded && */currentAction != null && currentAction.rootMotion)
			{
				Vector3 rootMotionDisplacement = currentActionState.layers[0].rootMotionDisplacement;
				rootMotionDisplacement = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI + yaw) * rootMotionDisplacement;
				displacement += rootMotionDisplacement;
			}


			verticalVelocity = verticalVelocity + 0.5f * GRAVITY * Time.deltaTime;

			{
				Vector3 velocity = displacement / Time.deltaTime + new Vector3(0.0f, verticalVelocity, 0.0f);
				body.setVelocity(velocity);
				//position += displacement;
			}

			verticalVelocity = verticalVelocity + 0.5f * GRAVITY * Time.deltaTime;


			isGrounded = false;
			if (verticalVelocity < 0.01f)
			{
				SweepHit[] hits = new SweepHit[16];
				int numHits = Physics.SweepSphere(0.3f, position + new Vector3(0.0f, 0.1f, 0.0f), Vector3.Down, 0.2f, hits, 16, QueryFilterFlags.Static | QueryFilterFlags.Dynamic);
				for (int i = 0; i < numHits; i++)
				{
					RigidBody body = hits[i].body;
					if (!hits[i].isTrigger && body != null && body.entity != this)
					{
						isGrounded = true;
						break;
					}
				}
			}

			if (isGrounded)
				verticalVelocity = 0.0f;
		}
		else
		{
			body.setVelocity(Vector3.Zero);
		}
	}

	void updateActions()
	{
		// Actions
		if (actionQueue.Count > 0)
		{
			MobAction currentAction = actionQueue[0];
			if (currentAction.hasStarted)
			{
				bool actionShouldFinish = currentAction.hasFinished ||
					(currentAction.elapsedTime >= currentAction.followUpCancelTime && actionQueue.Count > 1 && currentAction.type == actionQueue[1].type);
				if (actionShouldFinish)
				{
					currentAction.onFinished(this);
					actionQueue.RemoveAt(0);
					currentAction = actionQueue.Count > 0 ? actionQueue[0] : null;
				}
			}

			if (currentAction != null)
			{
				if (!currentAction.hasStarted)
				{
					// Initialize action
					currentAction.startTime = Time.currentTime;

					currentActionState = currentActionState == actionState1 ? actionState2 : currentActionState == actionState2 ? actionState1 : actionState1;

					AnimationData? animationData = null;
					if (currentAction.animationName != null)
					{
						animationData = model.getAnimationData(currentAction.animationName);
						currentActionState.layers[0].animationData = model;
					}
					if (animationData != null)
					{
						currentActionState.layers[0].animationName = currentAction.animationName;
						currentActionState.layers[0].looping = false;
						currentActionState.layers[0].rootMotion = currentAction.rootMotion;
						currentActionState.layers[0].rootMotionNode = rootNode;
						currentActionState.animationSpeed = currentAction.animationSpeed;

						if (currentAction.duration == 0.0f)
							currentAction.duration = animationData.Value.duration / currentAction.animationSpeed;
					}

					currentAction.onStarted(this);
				}

				currentAction.update(this);
			}
		}
	}

	Matrix getWeaponTransform(int handID)
	{
		Node itemNode = handID == 0 ? rightWeaponNode : leftWeaponNode;
		if (itemNode != null)
			return getModelMatrix() * Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.Up, MathF.PI)) * animator.getNodeTransform(itemNode, 0) * Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.UnitX, MathF.PI));
		return Matrix.Identity;
	}

	void updateAnimations()
	{
		if (animator != null)
		{
			if (currentAction != null)
			{
				animator.setStateIfNot(currentActionState);
			}
			else
			{
				if (state == CreatureState.Idle)
					animator.setStateIfNot(idleState);
				else if (state == CreatureState.Run)
					animator.setStateIfNot(runState);
				else if (state == CreatureState.Dead)
					animator.setStateIfNot(deadState);
				else
				{
					Debug.Assert(false);
				}
			}

			animator.update();
			animator.applyAnimation();
			//model.applyAnimation(animator.nodeLocalTransforms);


			weaponEntities[0].setTransform(getWeaponTransform(0));
			weaponEntities[1].setTransform(getWeaponTransform(1));

			weaponEntities[0].update();
			weaponEntities[1].update();
		}

		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);
	}

	public override void update()
	{
		if (state != CreatureState.Dead)
		{
			if (behavior != null)
				behavior.update();
		}

		updateMovement();
		updateActions();
		updateAnimations();

		audio.updateTransform(position);

		if (hitParticles != null)
		{
			hitParticles.transform = getModelMatrix();
			hitParticles.update();
		}
	}

	public void queueAction(MobAction action)
	{
		if (actionQueue.Count < MAX_ACTION_QUEUE_SIZE)
		{
			actionQueue.Add(action);
		}
	}

	public void cancelAction(int num = 1)
	{
		if (actionQueue.Count > 0)
			actionQueue.RemoveAt(0);
	}

	public void clearActions()
	{
		actionQueue.Clear();
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (model != null)
		{
			Renderer.DrawModel(model, getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI), animator);

			weaponEntities[0].draw(graphics);
			weaponEntities[1].draw(graphics);
		}

		Matrix viewProjection = Renderer.projection * Renderer.view; // Maze.instance.camera.getProjectionMatrix() * Maze.instance.camera.getViewMatrix();

		bool renderHealthbar = (Time.currentTime - lastDamagedTime) / 1e9f < HEALTHBAR_SHOW_DURATION;
		if (renderHealthbar)
		{
			Vector3 healthbarPosition = position + new Vector3(0.0f, 2.0f, 0.0f);
			Vector2i screenPosition = MathHelper.WorldToScreenSpace(healthbarPosition, viewProjection, Display.viewportSize);
			if (screenPosition.x != -1 && screenPosition.y != -1)
			{
				int width = 160;
				int height = 5;

				Renderer.DrawUIRect(screenPosition.x - width / 2, screenPosition.y, width, height, 0xff231507);
				Renderer.DrawUIRect(screenPosition.x - width / 2, screenPosition.y, width * stats.health / stats.maxHealth, height, 0xffcc3726);
				Renderer.DrawText(screenPosition.x - width / 2, screenPosition.y - 20, 1.0f, stats.health.ToString(), healthbarTextFont, 0xffffffff);
			}
		}

		if (nameTag != null)
		{
			Vector3 nameTagPosition = position + new Vector3(0.0f, 1.8f, 0.0f);
			Vector2i screenPosition = MathHelper.WorldToScreenSpace(nameTagPosition, viewProjection, Display.viewportSize);

			int width = healthbarTextFont.measureText(nameTag);

			Renderer.DrawText(screenPosition.x - width / 2, screenPosition.y - 40, 1.0f, nameTag, healthbarTextFont, 0xff999999);
		}

		if (hitParticles != null)
			hitParticles.draw(graphics);
	}

	public MobAction currentAction
	{
		get => actionQueue.Count > 0 && actionQueue[0].startTime != 0 ? actionQueue[0] : null;
	}
}
