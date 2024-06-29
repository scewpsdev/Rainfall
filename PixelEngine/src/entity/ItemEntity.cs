using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemEntity : Entity, Interactable
{
	float gravity = -30;
	float bounciness = 0.5f;

	public Vector2 velocity = Vector2.Zero;
	int ricochets = 0;

	public Item item;


	public ItemEntity(Item item)
	{
		this.item = item;

		collider = new FloatRect(-0.25f, -0.25f, 0.5f, 0.5f);
	}

	public bool canInteract(Player player)
	{
		return true;
	}

	public void interact(Player player)
	{
		if (player.pickupObject(this))
			remove();
	}

	public float getRange()
	{
		return 1.0f;
	}

	public KeyCode getInput()
	{
		return KeyCode.X;
	}

	public override void update()
	{
		velocity.y += gravity * Time.deltaTime;

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement);
		position += displacement;

		if ((collisionFlags & Level.COLLISION_X) != 0)
		{
			velocity.x = -velocity.x * bounciness;
			ricochets++;
		}
		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			velocity.y = -velocity.y * bounciness;
			velocity.x *= bounciness;
			ricochets++;
		}

		if (ricochets == 0)
		{
			HitData overlap = GameState.instance.level.overlap(position + collider.min, position + collider.max, FILTER_MOB);
			if (overlap != null)
			{
				if (overlap.entity != null && overlap.entity != this)
				{
					if (overlap.entity is Mob)
					{
						Mob mob = overlap.entity as Mob;
						mob.hit(1, this);
					}
					remove();
				}
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, LAYER_DEFAULT_OVERLAY, 1, 1, 0, item.sprite, false, 0xFFFFFFFF);
	}
}
