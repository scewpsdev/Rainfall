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
		float restitution = 0.1f;

		body = new RigidBody(this, RigidBodyType.Dynamic, PhysicsFilter.Pickup | PhysicsFilter.Interactable, PhysicsFilter.Default | PhysicsFilter.Creature | PhysicsFilter.Pickup | PhysicsFilter.Ragdoll);
		for (int i = 0; i < item.colliders.Count; i++)
		{
			SceneFormat.ColliderData collider = item.colliders[i];
			if (!collider.trigger)
			{
				if (collider.type == SceneFormat.ColliderType.Box)
					body.addBoxCollider(collider.size * 0.5f, collider.offset, Quaternion.Identity, 0.5f, restitution);
				else if (collider.type == SceneFormat.ColliderType.Sphere)
					body.addSphereCollider(collider.radius, collider.offset, 0.5f, restitution);
				else if (collider.type == SceneFormat.ColliderType.Capsule)
					body.addCapsuleCollider(collider.radius, collider.size.y, collider.offset, Quaternion.Identity, 0.5f, restitution);
				else if (collider.type == SceneFormat.ColliderType.Mesh)
					body.addMeshColliders(collider.meshCollider, Matrix.Identity, 0.5f, restitution);
				else if (collider.type == SceneFormat.ColliderType.ConvexMesh)
					body.addConvexMeshColliders(collider.meshCollider, Matrix.Identity, 0.5f, restitution);
				else
					Debug.Assert(false);
			}
		}
		body.addBoxCollider(model.boundingBox.size * 0.5f, model.boundingBox.center, Quaternion.Identity, PhysicsFilter.Pickup | PhysicsFilter.Interactable, 0);
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
