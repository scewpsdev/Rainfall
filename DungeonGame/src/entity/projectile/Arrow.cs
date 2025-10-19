using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Arrow : Projectile, Interactable
{
	const float ARROW_SPEED = 30.0f;


	Item item;


	public Arrow(Item item, Item bow, Entity shooter, Vector3 direction, Vector3 offset)
		: base(direction * ARROW_SPEED, offset, shooter)
	{
		this.item = item;

		model = item.model;
		gravity = -5;
		damage = bow.baseDamage;
		stickToWalls = true;
		itemDrop = Item.Get("arrow");
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Weapon | (uint)PhysicsFilterGroup.Interactable, (uint)PhysicsFilterMask.Weapon);
		body.addBoxTrigger(new Vector3(0.02f, 0.02f, 0.8f), new Vector3(0.0f, 0.0f, -0.6f), Quaternion.Identity);
	}

	public bool canInteract(Entity by)
	{
		return true;
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = by as Player;
			player.inventory.addItem(item, 1);
			remove();
		}
	}
}
