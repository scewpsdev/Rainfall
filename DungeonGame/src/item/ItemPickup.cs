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

	public RigidBody body;
	ParticleSystem particles;
	AudioSource audio;

	bool looted = false;


	public ItemPickup(Item item, int amount, Item[] equippedSpells, Chest chest, bool simulate)
	{
		this.item = item;
		this.amount = amount;
		this.equippedSpells = equippedSpells;
		this.chest = chest;
		this.simulate = simulate;
	}

	public ItemPickup(Item item, int amount = 1, Chest chest = null, Item[] equippedSpells = null)
		: this(item, amount, equippedSpells, chest, chest == null)
	{
	}

	public override void init()
	{
		rotation = rotation * item.pickupTransform;

		body = new RigidBody(this, simulate ? RigidBodyType.Dynamic : RigidBodyType.Kinematic, 1.0f, item.colliderCenterOfMass, (uint)PhysicsFilterGroup.ItemPickup | (uint)PhysicsFilterGroup.Interactable, (uint)PhysicsFilterMask.ItemPickup);

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

		if (item.particleSystems.Count > 0)
		{
			particles = new ParticleSystem(64);
			particles.copyData(item.particleSystems[0]);
		}

		audio = new AudioSource(position);
	}

	public override void destroy()
	{
		body.destroy();
		audio.destroy();
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (other != null && other.entity == null && contactType == ContactType.Found && simulate) // if level geometry
		{
			if (item.sfxDrop != null && !audio.isPlaying)
				audio.playSound(item.sfxDrop);
		}
	}

	public override void update()
	{
		if (looted)
		{
			if (!audio.isPlaying)
				remove();
			return;
		}

		if (simulate)
			body.getTransform(out position, out rotation);

		if (particles != null)
		{
			particles.transform = getModelMatrix();
			particles.update();
		}

		audio.updateTransform(position);
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (looted)
			return;

		Matrix modelMatrix = getModelMatrix();

		Renderer.DrawModel(item.model, modelMatrix);

		foreach (ItemLight light in item.lights)
		{
			Matrix lightTransform = modelMatrix * Matrix.CreateTranslation(light.position);
			Renderer.DrawLight(lightTransform.translation, light.color);
		}

		if (particles != null)
			particles.draw(graphics);
	}

	public bool canInteract(Entity by)
	{
		return !looted;
	}

	public void interact(Entity by)
	{
		Debug.Assert(!looted);
		if (by is Player)
		{
			Player player = (Player)by;
			player.giveItem(item, amount, equippedSpells);
		}
		if (chest != null)
			chest.removePickup(this);
		if (item.sfxTake != null)
			audio.playSoundOrganic(item.sfxTake);
		looted = true;
	}
}
