using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicProjectile : Entity
{
	float speed = 2;
	float maxSpeed = 20;
	float acceleration = 30;
	int maxRicochets = 0;
	float damage = 1;

	Entity shooter;
	Item item;

	Sprite sprite;
	Vector2 direction;

	int ricochets = 0;
	Vector2 offset;

	List<Entity> hitEntities = new List<Entity>();


	public MagicProjectile(Vector2 direction, Vector2 startVelocity, Vector2 offset, Entity shooter, Item item)
	{
		this.direction = direction;
		this.offset = offset;
		this.shooter = shooter;
		this.item = item;

		collider = new FloatRect(-0.1f, -0.1f, 0.2f, 0.2f);
		filterGroup = FILTER_PROJECTILE;

		velocity = direction * speed;
		if (MathF.Sign(velocity.x) == MathF.Sign(startVelocity.x) && MathF.Abs(startVelocity.x) > MathF.Abs(velocity.x))
			velocity.x = startVelocity.x;
		//velocity += (Vector2.Dot(startVelocity, velocity) + 1.0f) * 0.5f * startVelocity * 0.05f;

		if (item != null)
		{
			damage = item.attackDamage;
			if (shooter is Player)
				damage *= (shooter as Player).attack;
		}

		sprite = new Sprite(Item.tileset, 9, 1);
	}

	public override void update()
	{
		velocity += velocity.normalized * acceleration * Time.deltaTime;
		if (velocity.length > maxSpeed)
			velocity = velocity.normalized * maxSpeed;

		Vector2 displacement = velocity * Time.deltaTime;
		position += displacement;

		rotation = MathF.Atan2(velocity.y, velocity.x);

		offset = Vector2.Lerp(offset, Vector2.Zero, 3 * Time.deltaTime);

		HitData hit = GameState.instance.level.raycast(position - displacement, displacement.normalized, displacement.length, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (hit != null)
		{
			if (hit.entity != null)
			{
				if (hit.entity != shooter && hit.entity is Hittable && !hitEntities.Contains(hit.entity))
				{
					Hittable hittable = hit.entity as Hittable;
					hittable.hit(damage, shooter, item);
					hitEntities.Add(hit.entity);
					remove();
				}
			}
			else
			{
				if (ricochets >= maxRicochets)
					remove();
				else
				{
					velocity = Vector2.Reflect(velocity, hit.normal);
					position += velocity * 0.01f;
					ricochets++;
				}
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f + offset.x, position.y - 0.5f + offset.y, 0, 1, 1, rotation, sprite, false, new Vector4(3.0f));
		Renderer.DrawLight(position, new Vector3(0.5f, 1.0f, 1.0f) * 3, 3.0f);
	}
}
