using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ItemPickup : Entity, Interactable
{
	Item item;
	int amount;
	Item[] equippedSpells;

	Chest chest;
	bool simulate;

	RigidBody body;
	ParticleSystem particles;


	public ItemPickup(Item item, int amount = 1, Chest chest = null, Item[] equippedSpells = null)
	{
		this.item = item;
		this.amount = amount;
		this.equippedSpells = equippedSpells;
		this.chest = chest;
		this.simulate = chest == null;
	}

	public override void init()
	{
		body = new RigidBody(this, simulate ? RigidBodyType.Dynamic : RigidBodyType.Kinematic, 1.0f, item.colliderCenterOfMass);

		foreach (Collider collider in item.colliders)
		{
			if (collider.type == ColliderType.Box)
				body.addBoxCollider(0.5f * collider.size, collider.offset, Quaternion.Identity);
			else if (collider.type == ColliderType.Sphere)
				body.addSphereCollider(collider.radius, collider.offset);
			else if (collider.type == ColliderType.Capsule)
				body.addCapsuleCollider(collider.radius, collider.size.y, collider.offset, Quaternion.Identity);
			else
			{
				Debug.Assert(false);
			}
		}

		if (item.particleTexture != null)
		{
			particles = new ParticleSystem(64);
			particles.textureAtlas = item.particleTexture;
			particles.atlasColumns = item.particleAtlasColumns;
			particles.frameWidth = item.particleFrameSize;
			particles.frameHeight = item.particleFrameSize;
			particles.numFrames = item.particleFrameCount;
			particles.emissionRate = item.particleEmissionRate;
			particles.lifetime = item.particleLifetime;
			particles.spawnOffset = item.particleSpawnOffset;
			particles.spawnRadius = item.particleSpawnRadius;
			particles.spawnShape = item.particleSpawnShape;
			particles.particleSize = item.particleSize;
			particles.initialVelocity = item.particleInitialVelocity;
			particles.gravity = item.particleGravity;
			particles.followMode = ParticleFollowMode.Trail;
			particles.additive = item.particleAdditive;
			//particles.spriteTint = item.particleTint;
		}
	}

	public override void destroy()
	{
		body.destroy();
	}

	public override void update()
	{
		if (simulate)
			body.getTransform(out position, out rotation);

		if (particles != null)
		{
			particles.transform = getModelMatrix();
			particles.update();
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix modelMatrix = getModelMatrix();

		Renderer.DrawModel(item.model, modelMatrix);

		foreach (Light light in item.lights)
		{
			Matrix lightTransform = modelMatrix * Matrix.CreateTranslation(light.position);
			Renderer.DrawLight(lightTransform.translation, light.color);
		}

		if (particles != null)
			particles.draw(graphics);
	}

	public bool canInteract()
	{
		return true;
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = (Player)by;
			player.onItemPickup(item, amount, equippedSpells);
		}
		if (chest != null)
			chest.removePickup(this);
		remove();
	}
}
