using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Crate : Entity, Interactable, ItemContainerEntity, Hittable
{
	Model model, fracturedModel;
	RigidBody body;

	Sound[] sfxBreak;

	public ItemContainer container;


	public Crate()
	{
		model = Resource.GetModel("res/entity/object/crate/crate.gltf");
		model.configureLODs(LOD.DISTANCE_MEDIUM);

		fracturedModel = Resource.GetModel("res/entity/object/crate/crate_fractured.gltf");
		fracturedModel.configureLODs(LOD.DISTANCE_SMALL);

		container = new ItemContainer(5, 5);
	}

	public Crate(Item[] items, int[] amounts)
		: base()
	{
		for (int i = 0; i < items.Length; i++)
			container.addItem(items[i], amounts[i]);
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Dynamic, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		body.addBoxCollider(new Vector3(0.5f), Vector3.Zero, Quaternion.Identity);

		sfxBreak = new Sound[]
		{
			Resource.GetSound("res/entity/object/crate/sfx/break1.ogg"),
		};
	}

	public override void destroy()
	{
		body.destroy();
	}

	public bool canInteract(Entity by)
	{
		return true;
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = (Player)by;
			if (player.currentAction == null)
				player.inventoryUI.openChestUI(this);
		}
	}

	public void onClose()
	{
	}

	public ItemContainer getContainer()
	{
		return container;
	}

	public void hit(int damage, Entity from, Vector3 hitPosition, Vector3 force, int linkID)
	{
		remove();
		if (fracturedModel != null)
			DungeonGame.instance.level.addEntity(new FracturedObject(fracturedModel, sfxBreak, 0.3f), position, rotation);

		foreach (ItemSlot slot in container.items)
		{
			if (slot.item != null)
			{
				ItemPickup pickup = new ItemPickup(slot.item, slot.stackSize);
				Vector3 itemPosition = position + MathHelper.RandomVector3(-1.0f, 1.0f);
				Quaternion itemRotation = Quaternion.FromAxisAngle(new Vector3(1.0f).normalized, MathHelper.RandomFloat(0.0f, MathF.PI * 2.0f));
				DungeonGame.instance.level.addEntity(pickup, itemPosition, itemRotation);
			}
		}
	}

	public override void update()
	{
		body.getTransform(out position, out rotation);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix());
	}
}
