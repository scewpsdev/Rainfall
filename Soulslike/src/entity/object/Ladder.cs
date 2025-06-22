using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public class Ladder : Entity, Interactable
{
	public int height;
	public Vector3 halfExtents;
	public Vector3 offset;

	public Sound[] stepSound;


	public Ladder(int height)
	{
		this.height = height;

		halfExtents = new Vector3(0.5f, 0.5f * height, 0.1f);
		offset = new Vector3(0, 0.5f * height, 0.1f);

		model = Resource.GetModel("entity/object/ladder/ladder.gltf");

		stepSound = Resource.GetSounds("sound/step/climb", 5);
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static, PhysicsFilter.Default | PhysicsFilter.Interactable);
		body.addBoxCollider(halfExtents, offset, Quaternion.Identity);
	}

	public override void destroy()
	{
		body.destroy();
	}

	public bool canInteract(Player player)
	{
		return true;
		//return player.controller.currentLadder == null;
	}

	public void interact(Player player)
	{
#if false
		player.controller.initLadder(this);
		Vector2 toLadder = position.xz - player.position.xz;
		player.setRotation(MathF.Atan2(-toLadder.x, -toLadder.y));
		player.pitch = 0;
		player.actionManager.cancelAllActions();
		player.actionManager.queueAction(new LadderClimbAction(this));
#endif
	}

	public Vector3 getAttachPoint(Entity player)
	{
#if false
		Vector3 point = position + normal * (0.2f + FirstPersonController.COLLIDER_RADIUS);
		point.y = MathF.Max(player.position.y, position.y + 0.02f);
		return point;
#endif
		return position;
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (other != null && other.entity is Player)
		{
			Player player = other.entity as Player;

			if (contactType == ContactType.Found)
			{
				//player.controller.initLadder(this);
			}
			else if (contactType == ContactType.Lost)
			{
				//player.controller.endLadder();
			}
		}
		if (body != null && body.entity is Player)
		{
			Debug.Assert(false);
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		for (int i = 0; i < height; i++)
		{
			Renderer.DrawModel(model, position + Vector3.Up * i, rotation);
		}
	}

	public Vector3 normal
	{
		get { return rotation.back; }
	}
}
