using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Creature : Entity, Hittable
{
	public class CreatureStats
	{
		public int maxHealth = 100;
		public int damage = 20;
		public float headshotDmgMultiplier = 2.0f;

		public int health = 100;


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

	protected struct ItemDrop
	{
		public int itemID;
		public int amount;
		public float dropChance;

		public ItemDrop(int itemID, int amount, float dropChance)
		{
			this.itemID = itemID;
			this.amount = amount;
			this.dropChance = dropChance;
		}
	}


	const float GRAVITY = -10.0f;

	const float HEALTHBAR_SHOW_DURATION = 5.0f;

	const int MAX_ACTION_QUEUE_SIZE = 2;

	const int AI_TICKS = 10;

	public Model model { get; protected set; }
	protected Node rootNode, rightWeaponNode, leftWeaponNode;
	public Animator animator { get; protected set; }
	protected AnimationState idleState, runState, deadState;
	protected AnimationState actionState1, actionState2;
	protected AnimationState currentActionState;

	public RigidBody movementBody { get; protected set; }
	public List<RigidBody> hitboxes { get; private set; } = new List<RigidBody>();
	public Dictionary<Node, int> hitboxesNodeMap { get; private set; } = new Dictionary<Node, int>();
	protected Dictionary<string, BoneHitbox> hitboxData = new Dictionary<string, BoneHitbox>();
	int headColliderID = -1;

	public Ragdoll ragdoll { get; private set; }
	Sound[] ragdollImpactSounds;

	protected ParticleSystem hitParticles;

	protected string nameTag = null;

	protected AI ai = null;
	long lastAIUpdate = 0;

	protected List<ItemDrop> itemDrops = new List<ItemDrop>();

	public CreatureState state = CreatureState.Idle;
	public CreatureStats stats;

	protected float walkSpeed = 2.0f;
	protected float rotationSpeed = 3.0f;

	public Vector2 fsu = Vector2.Zero;
	public float rotationDirection = 0.0f;

	public float yaw = 0.0f;

	float verticalVelocity = 0.0f;
	bool isGrounded = false;

	AudioSource audio;
	protected Sound[] hitSound;

	Item[] handItem = new Item[2];
	MobItemEntity[] weaponEntities = new MobItemEntity[2];

	long lastDamagedTime = 0;
	Font healthbarTextFont;
	byte[] healthbarText = new byte[32];

	List<MobAction> actionQueue = new List<MobAction>();


	protected Creature()
	{
		stats = new CreatureStats();

		weaponEntities[0] = new MobItemEntity(this, 0);
		weaponEntities[1] = new MobItemEntity(this, 1);

		healthbarTextFont = Resource.GetFontData("res/fonts/libre-baskerville.regular.ttf").createFont(20.0f, true);

		ragdollImpactSounds = new Sound[] {
			Resource.GetSound("res/entity/creature/sfx/impact1.ogg"),
			Resource.GetSound("res/entity/creature/sfx/impact2.ogg"),
			Resource.GetSound("res/entity/creature/sfx/impact3.ogg"),
			Resource.GetSound("res/entity/creature/sfx/impact4.ogg"),
		};
	}

	protected void setItem(int handID, Item item)
	{
		handItem[0] = item;
		weaponEntities[0].setItem(item);
	}

	public override void init()
	{
		audio = Audio.CreateSource(position);

		movementBody = new RigidBody(this, RigidBodyType.Dynamic, (uint)PhysicsFilterGroup.CreatureMovementBody, (uint)PhysicsFilterMask.CreatureMovementBody);
		movementBody.lockRotationAxis(true, true, true);

		Node rootNode = model.skeleton.getNode("Hip");
		if (rootNode != null)
			Hitboxes.GenerateHitboxBodies(hitboxData, rootNode, this, hitboxes, hitboxesNodeMap);
		Node headNode = model.skeleton.getNode("Head");
		if (headNode == null || !hitboxesNodeMap.ContainsKey(headNode))
			headNode = model.skeleton.getNode("Neck");
		if (headNode != null && hitboxesNodeMap.ContainsKey(headNode))
			headColliderID = hitboxesNodeMap[headNode];

		yaw = rotation.eulers.y;
	}

	public override void destroy()
	{
		Audio.DestroySource(audio);

		movementBody?.destroy();

		foreach (RigidBody body in hitboxes)
			body.destroy();

		ragdoll?.destroy();

		ai?.destroy();
	}

	protected virtual void onHit(int damage, Entity from, Vector3 force)
	{
		if (isAlive)
		{
			if (ai != null)
				ai.onHit(damage, from);
		}
	}

	protected void onDeath(int damage, Entity from, Vector3 force, int linkID)
	{
		if (hitboxData.Count > 0)
		{
			// TODO hitboxData needs to be passed in correct order
			//ragdoll = new RagdollEntity(model, animator, hitboxData, (uint)PhysicsFilterGroup.Ragdoll, (uint)PhysicsFilterMask.Ragdoll);
			ragdoll = new Ragdoll(this, model, model.skeleton.getNode("Hip"), animator, getModelMatrix(), hitboxData, (uint)PhysicsFilterGroup.Ragdoll, (uint)PhysicsFilterMask.Ragdoll);
			//Maze.instance.level.addEntity(ragdoll, position, rotation);
			if (force != Vector3.Zero && linkID != -1)
				ragdoll.hitboxes[linkID].addForce(force);

			foreach (RigidBody body in hitboxes)
				body.clearColliders();
		}

		for (int i = 0; i < itemDrops.Count; i++)
		{
			if (Random.Shared.NextSingle() < itemDrops[i].dropChance)
			{
				ItemPickup item = new ItemPickup(Item.Get(itemDrops[i].itemID), itemDrops[i].amount);
				DungeonGame.instance.level.addEntity(item, position + new Vector3(0.0f, 1.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.One.normalized, Random.Shared.NextSingle() * MathF.PI * 2.0f));
			}
		}

		//remove();
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (ragdoll != null)
		{
			if (Random.Shared.NextSingle() < 0.2f)
				audio.playSoundOrganic(ragdollImpactSounds);
		}
	}

	public void hit(int damage, Entity from, Vector3 force, int linkID)
	{
		if (from is Creature)
			return;
		if (from is Arrow)
			from = ((Arrow)from).shooter;

		if (headColliderID == linkID)
			damage = (int)(damage * stats.headshotDmgMultiplier);

		stats.applyDamage(damage);
		lastDamagedTime = Time.currentTime;

		onHit(damage, from, force);

		if (stats.health == 0)
		{
			state = CreatureState.Dead;
			movementBody.clearColliders();
			weaponEntities[0].hitbox.clearColliders();
			weaponEntities[1].hitbox.clearColliders();
			if (from is Player)
			{
				Player otherPlayer = (Player)from;
				otherPlayer.stats.awardXP(200);
			}
			actionQueue.Clear();

			onDeath(damage, from, force, linkID);
		}
		else
		{
			if (currentAction == null || currentAction.type != MobActionType.StaggerShort)
			{
				actionQueue.Clear();
				queueAction(new MobStaggerAction(MobActionType.StaggerShort));
			}
		}

		if (hitSound != null)
			audio.playSoundOrganic(hitSound);

		if (hitParticles != null && from != null)
		{
			Vector3 hitDirection = (position - from.position).normalized;
			int numBloodParticles = MathHelper.RandomInt(1, 3);
			for (int i = 0; i < numBloodParticles; i++)
			{
				Vector3 particleDirection = -hitDirection.normalized * 0.5f;
				Vector3 randomVector = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()) * 2.0f - 1.0f;
				particleDirection += randomVector * Vector3.Dot(randomVector.normalized, particleDirection);
				hitParticles.emitParticle(particleDirection);
			}
		}
	}

	public void hit(int damage, Entity from)
	{
		hit(damage, from, Vector3.Zero, 0);
	}

	void updateMovement()
	{
		if (state != CreatureState.Dead)
		{
			movementBody.getTransform(out position, out Quaternion _);

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
				movementBody.setVelocity(velocity);
				//position += displacement;
			}

			verticalVelocity = verticalVelocity + 0.5f * GRAVITY * Time.deltaTime;


			isGrounded = false;
			if (verticalVelocity < 0.01f)
			{
				Span<HitData> hits = stackalloc HitData[16];
				int numHits = Physics.OverlapSphere(0.3f, position + new Vector3(0.0f, 0.3f - 0.1f, 0.0f), hits, QueryFilterFlags.Static | QueryFilterFlags.Dynamic);
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
			movementBody.setVelocity(Vector3.Zero);
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

		Hitboxes.UpdateHitboxBodyTransforms(this);
	}

	public override void update()
	{
		if (isAlive)
		{
			if (state != CreatureState.Dead)
			{
				if (ai != null)
				{
					if ((Time.currentTime - lastAIUpdate) / 1e9f >= 1.0f / AI_TICKS)
					{
						ai.update();
						lastAIUpdate = Time.currentTime;
					}
				}
			}

			updateMovement();
			updateActions();
			updateAnimations();

			audio.updateTransform(position);

			if (position.y < -100)
			{
				hit(10000, null);
				remove();
			}
		}
		else
		{
			if (ragdoll != null)
			{
				ragdoll.update();
				ragdoll.getTransform(out position, out rotation);
			}
		}

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

	public void cancelAction()
	{
		if (actionQueue.Count > 0)
		{
			actionQueue[0].onFinished(this);
			actionQueue.RemoveAt(0);
		}
	}

	public void cancelAllActions()
	{
		while (actionQueue.Count > 0)
		{
			actionQueue[0].onFinished(this);
			actionQueue.RemoveAt(0);
		}
	}

	public void clearActions()
	{
		actionQueue.Clear();
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI), animator);

		if (isAlive)
		{
			weaponEntities[0].draw(graphics);
			weaponEntities[1].draw(graphics);

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

					StringUtils.WriteInteger(healthbarText, stats.health);
					Renderer.DrawText(screenPosition.x - width / 2, screenPosition.y - 20, 1.0f, healthbarText, healthbarTextFont, 0xffffffff);
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


		/*
		foreach (var pair in hitboxes)
		{
			Node node = pair.Key;
			Matrix globalTransform = getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI) * animator.getNodeTransform(node, 0);
			Renderer.DrawModel(Resource.GetModel("res/models/cube.gltf"), globalTransform * Matrix.CreateScale(0.05f));
		}
		*/
	}

	public bool isAlive
	{
		get => stats.health > 0;
	}

	public bool isDead
	{
		get => stats.health <= 0;
	}

	public MobAction currentAction
	{
		get => actionQueue.Count > 0 && actionQueue[0].startTime != 0 ? actionQueue[0] : null;
	}
}
