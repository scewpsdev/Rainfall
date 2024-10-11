using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Projectile : Entity
{
	protected float maxSpeed = 100;
	protected float acceleration = 0;
	protected float gravity = 0;
	protected float rotationSpeed = 0;
	protected int maxRicochets = 0;
	protected float damage = 1;

	public Entity shooter;
	protected Item item;

	protected Sprite sprite;
	protected Vector4 spriteColor = Vector4.One;
	protected bool additive = false;

	int ricochets = 0;
	Vector2 offset;

	List<Entity> hitEntities = new List<Entity>();


	public Projectile(Vector2 velocity, Vector2 startVelocity, Vector2 offset, Entity shooter, Item item)
	{
		this.offset = offset;
		this.shooter = shooter;
		this.item = item;

		collider = new FloatRect(-0.1f, -0.1f, 0.2f, 0.2f);
		filterGroup = FILTER_PROJECTILE;

		this.velocity = velocity;
		if (MathF.Sign(velocity.x) == MathF.Sign(startVelocity.x) && MathF.Abs(startVelocity.x) > MathF.Abs(velocity.x))
			velocity.x = startVelocity.x;
		//velocity += (Vector2.Dot(startVelocity, velocity) + 1.0f) * 0.5f * startVelocity * 0.05f;

		if (item != null)
			damage = item.attackDamage;
		if (shooter is Player)
			damage *= (shooter as Player).getAttackDamageModifier();
	}

	public virtual void onHit(Vector2 normal)
	{
	}

	public override void update()
	{
		velocity += velocity.normalized * acceleration * Time.deltaTime;
		velocity.y += gravity * Time.deltaTime;
		if (velocity.length > maxSpeed)
			velocity = velocity.normalized * maxSpeed;

		Vector2 displacement = velocity * Time.deltaTime;
		position += displacement;

		if (rotationSpeed > 0)
			rotation += rotationSpeed * Time.deltaTime;
		else
			rotation = MathF.Atan2(velocity.y, velocity.x);

		offset = Vector2.Lerp(offset, Vector2.Zero, 3 * Time.deltaTime);

		HitData hit = GameState.instance.level.raycast(position - displacement, displacement.normalized, displacement.length, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (hit == null)
			hit = GameState.instance.level.sweep(position - displacement, collider, displacement.normalized, displacement.length, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (hit != null)
		{
			if (hit.entity != null)
			{
				if (hit.entity != shooter && !hitEntities.Contains(hit.entity) && hit.entity is Hittable)
				{
					Hittable hittable = hit.entity as Hittable;
					hitEntities.Add(hit.entity);
					if (hittable.hit(damage, this, item))
					{
						onHit(hit.normal);

						if (hit.entity is Mob)
						{
							Mob mob = hit.entity as Mob;
							Vector2 knockback = (hit.entity.position - position).normalized * (item != null ? item.knockback : 8);
							mob.addImpulse(knockback);
						}

						remove();
					}
				}
			}
			else
			{
				if (ricochets >= maxRicochets)
				{
					onHit(hit.normal);
					remove();
				}
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
		Renderer.DrawSprite(position.x - 0.5f + offset.x, position.y - 0.5f + offset.y, 0, 1, 1, rotation, sprite, false, spriteColor, additive);
	}
}
