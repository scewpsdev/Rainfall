using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemEntity : Entity, Interactable
{
	Item item;


	public ItemEntity(Item item)
	{
		this.item = item;

		model = item.model;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Dynamic, PhysicsFilter.Pickup | PhysicsFilter.Interactable);
		for (int i = 0; i < item.colliders.Count; i++)
		{
			SceneFormat.ColliderData collider = item.colliders[i];
			if (!collider.trigger)
			{
				if (collider.type == SceneFormat.ColliderType.Box)
					body.addBoxCollider(collider.size * 0.5f, collider.offset, Quaternion.Identity);
				else if (collider.type == SceneFormat.ColliderType.Sphere)
					body.addSphereCollider(collider.radius, collider.offset);
				else if (collider.type == SceneFormat.ColliderType.Capsule)
					body.addCapsuleCollider(collider.radius, collider.size.y, collider.offset, Quaternion.Identity);
				else if (collider.type == SceneFormat.ColliderType.Mesh)
					body.addMeshColliders(collider.meshCollider, Matrix.Identity);
				else if (collider.type == SceneFormat.ColliderType.ConvexMesh)
					body.addConvexMeshColliders(collider.meshCollider, Matrix.Identity);
				else
					Debug.Assert(false);
			}
		}
	}

	public override void destroy()
	{
		body.destroy();
	}

	public void interact(Player player)
	{
		player.giveItem(item);
		remove();
	}

	public override void update()
	{
		body.getTransform(out position, out rotation);
	}
}
