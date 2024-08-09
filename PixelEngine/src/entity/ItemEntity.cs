using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public class ItemEntity : Entity, Interactable, Destructible
{
	public float gravity = -30;
	public float bounciness = 0.5f;

	public int ricochets = 0;
	int pierces = 0;
	int damage;

	float rotationVelocity = 0;

	public Entity thrower = null;
	long throwTime;

	public Item item;


	public ItemEntity(Item item, Entity thrower = null, Vector2 velocity = default)
	{
		this.item = item;

		collider = new FloatRect(-0.25f, -0.25f, 0.5f, 0.5f);
		filterGroup = FILTER_ITEM;

		damage = item.attackDamage;

		this.thrower = thrower;
		this.velocity = velocity;
		throwTime = Time.currentTime;
	}

	public void onDestroyed(Entity entity, Item item)
	{
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

	void onHit(bool x, bool y)
	{
		if (!x && !y)
			return;

		if (x)
		{
			velocity.x = -velocity.x * bounciness;
		}
		else if (y)
		{
			velocity.y = -velocity.y * bounciness;
			velocity.x *= bounciness;
		}

		ricochets++;
		if (damage > 0)
			damage--;
		if (damage == 0)
			rotationVelocity = MathHelper.RandomFloat(-1, 1) * 20;
	}

	public override void update()
	{
		velocity.y += gravity * Time.deltaTime;

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement);
		position += displacement;

		onHit((collisionFlags & Level.COLLISION_X) != 0, (collisionFlags & Level.COLLISION_Y) != 0);

		if (damage > 0 && item.projectileItem && thrower != null)
		{
			HitData hit = GameState.instance.level.raycast(position, velocity.normalized, 0.5f, FILTER_DEFAULT | FILTER_MOB | FILTER_PLAYER);
			if (hit != null)
			{
				bool skipHit = hit.entity == thrower && (Time.currentTime - throwTime) / 1e9f < 1.0f;

				if (!skipHit)
				{
					if (hit.entity != null && hit.entity != this)
					{
						if (hit.entity is Hittable)
						{
							Hittable hittable = hit.entity as Hittable;
							hittable.hit(damage, this);

							if (pierces < item.maxPierces || item.maxPierces == -1)
								pierces++;
							else
							{
								damage = 0;
								onHit(MathF.Abs(hit.normal.x) > 0.5f, MathF.Abs(hit.normal.y) > 0.5f);
							}
						}
					}

					if (item.breakOnHit && velocity.lengthSquared > 1)
						remove();
				}
			}
		}

		item.update(this);
	}

	public override void render()
	{
		bool flipped = false;
		if (item.projectileItem && damage > 0)
		{
			if (velocity.lengthSquared > 0.1f)
			{
				rotation = MathF.Atan2(velocity.y, velocity.x);
				//flipped = velocity.x < 0;
			}
		}
		else if (item.projectileItem && damage == 0)
		{
			// Tumble
			if (velocity.lengthSquared > 0.1f)
			{
				rotation += rotationVelocity * Time.deltaTime;
				flipped = false;
			}
			else
			{
				if (MathF.Abs(rotation) < 0.5f * MathF.PI)
					rotation = MathHelper.Lerp(rotation, 0, 5 * Time.deltaTime);
				else
					rotation = MathHelper.LerpAngle(rotation, MathF.PI, 5 * Time.deltaTime);
			}
		}
		else
		{
			rotation = MathHelper.Lerp(rotation, 0, 5 * Time.deltaTime);
			flipped = false;
		}
		Renderer.DrawSprite(position.x - 0.5f * item.size.x, position.y - 0.5f * item.size.y, LAYER_PLAYER_ITEM, item.size.x, item.size.y, rotation, item.sprite, flipped, 0xFFFFFFFF);
	}
}
