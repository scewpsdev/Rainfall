using Rainfall;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


public class PlayerHand
{
	public readonly int handID;
	public readonly Player player;
	public Item item { get; private set; }

	public bool hitboxEnabled = false;

	public Matrix transform;
	Matrix lastTransform;

	Model model = null;
	Animator animator = null;
	//Matrix modelTransform = Matrix.Identity;

	List<PointLight> lights = new List<PointLight>();
	List<ParticleSystem> particles = new List<ParticleSystem>();

	string currentItemOverride = "kekw";


	public PlayerHand(int handID, Player player)
	{
		this.handID = handID;
		this.player = player;
	}

	public void destroy()
	{
		model?.destroy();
		model = null;
		if (animator != null)
		{
			Animator.Destroy(animator);
			animator = null;
		}
		for (int i = 0; i < lights.Count; i++)
			lights[i].destroy(Renderer.graphics);
		lights.Clear();
		for (int i = 0; i < particles.Count; i++)
			ParticleSystem.Destroy(particles[i]);
		particles.Clear();
	}

	void setItemEntity(SceneFormat.EntityData? entityData, string entityPath)
	{
		model?.destroy();
		model = null;
		for (int i = 0; i < lights.Count; i++)
			lights[i].destroy(Renderer.graphics);
		lights.Clear();
		for (int i = 0; i < particles.Count; i++)
			ParticleSystem.Destroy(particles[i]);
		particles.Clear();

		if (entityData != null)
		{
			if (entityData.Value.modelPath != null)
				model = Resource.GetModel(Path.GetDirectoryName(entityPath) + "/" + entityData.Value.modelPath);
			for (int i = 0; i < entityData.Value.lights.Count; i++)
			{
				SceneFormat.LightData lightData = entityData.Value.lights[i];
				PointLight light = new PointLight(lightData.offset, lightData.color * lightData.intensity, Renderer.graphics);
				lights.Add(light);
			}
			for (int i = 0; i < entityData.Value.particles.Count; i++)
			{
				ParticleSystem particle = ParticleSystem.Create(transform);
				unsafe
				{
					*particle.handle = entityData.Value.particles[0];
					if (particle.handle->textureAtlasPath[0] != 0)
						particle.handle->textureAtlas = Resource.GetTexture(Path.GetDirectoryName(entityPath) + "/" + new string((sbyte*)particle.handle->textureAtlasPath)).handle;
				}
				particles.Add(particle);
			}
		}
	}

	public void setItem(Item item)
	{
		this.item = item;

		if (item != null)
			setItemEntity(item.entity, item.entityPath);
		else
			setItemEntity(null, null);

		if (animator != null)
		{
			Animator.Destroy(animator);
			animator = null;
		}
		if (item != null && model != null && model.isAnimated)
		{
			animator = Animator.Create(model);
			animator.setAnimation(Animator.CreateAnimation(model, "default"));
		}
	}

	public void update(Matrix transform)
	{
		lastTransform = this.transform;
		this.transform = transform;

		if (player.actions.currentAction != null && player.actions.currentAction.overrideHandModels[handID])
		{
			string itemName = player.actions.currentAction.handItemModels[handID];
			if (itemName != currentItemOverride)
			{
				if (itemName != null)
				{
					Item item = Item.Get(itemName);
					if (item != null)
						setItemEntity(item.entity, item.entityPath);
					else
					{
						/*
						EntityType entityType = EntityType.Get(itemName);
						if (entityType != null)
							setItemEntity(entityType.entityData, entityType.entityDataPath);
						*/
					}
				}
				else
				{
					setItemEntity(null, null);
				}
				currentItemOverride = player.actions.currentAction.handItemModels[handID];
			}
		}
		else if (currentItemOverride != "kekw")
		{
			if (item != null)
				setItemEntity(item.entity, item.entityPath);
			else
				setItemEntity(null, null);
			currentItemOverride = "kekw";
		}

		if (animator != null)
		{
			if (player.actions.currentAction != null && player.actions.currentAction.handItemAnimations[handID] != null)
			{
				animator.currentAnimation.layers[0].animationName = player.actions.currentAction.handItemAnimations[handID];
				animator.timer = player.actions.currentAction.elapsedTime;
			}
			else
			{
				animator.currentAnimation.layers[0].animationName = "default";
			}

			//animator.update();
			animator.applyAnimation();
		}
		for (int i = 0; i < particles.Count; i++)
			particles[i].setTransform(transform);

		Span<HitData> hits = stackalloc HitData[256];
		int numHits = 0;
		if (hitboxEnabled)
		{
			foreach (SceneFormat.ColliderData collider in item.entity.Value.colliders)
			{
				Matrix colliderTransform = transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers));

				Matrix lastColliderTransform = lastTransform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers));
				Vector3 delta = colliderTransform.translation - lastColliderTransform.translation;
				float distance = delta.length;
				Vector3 direction = delta / distance;

				if (collider.type == SceneFormat.ColliderType.Box)
					numHits = Physics.SweepBox(collider.size * 0.5f, lastColliderTransform.translation, colliderTransform.rotation, direction, distance, hits, QueryFilterFlags.Default, PhysicsFiltering.CREATURE_HITBOX | PhysicsFiltering.RAGDOLL);
				else if (collider.type == SceneFormat.ColliderType.Sphere)
					numHits = Physics.SweepSphere(collider.radius, lastColliderTransform.translation, direction, distance, hits, QueryFilterFlags.Default, PhysicsFiltering.CREATURE_HITBOX | PhysicsFiltering.RAGDOLL);
				else if (collider.type == SceneFormat.ColliderType.Capsule)
					numHits = Physics.SweepCapsule(collider.radius, collider.height, lastColliderTransform.translation, colliderTransform.rotation, direction, distance, hits, QueryFilterFlags.Default, PhysicsFiltering.CREATURE_HITBOX | PhysicsFiltering.RAGDOLL);

				for (int i = 0; i < numHits; i++)
				{
					HitData hit = hits[i];
					if (hit.body != null)
					{
						// Pseudo friction
						// If the friction is high, the ragdoll gets pushed in the direction of the strike.
						// If the friction is low, the ragdoll slides along the weapon and moves in the direction of the inverse normal.
						float friction = 0.9f;
						Vector3 hitDirection = Vector3.Lerp(-hit.normal, direction, friction);
						onWeaponContact(hit.body, hit.position, hitDirection);
					}
				}
			}
		}
	}

	public void onWeaponContact(RigidBody other, Vector3 hitPosition, Vector3 hitDirection)
	{
		/*
		if (player.actions.currentAction != null && player.actions.currentAction is AttackAction)
		{
			((AttackAction)player.actions.currentAction).onContact(other, player, hitPosition, hitDirection);
		}
		*/
	}

	public void draw()
	{
		if (model != null)
			Renderer.DrawModel(model, transform, animator);
		for (int i = 0; i < lights.Count; i++)
			Renderer.DrawLight(transform.translation + lights[i].offset, lights[i].color);
		for (int i = 0; i < particles.Count; i++)
			Renderer.DrawParticleSystem(particles[i]);
	}
}
