using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Projectile : Entity, Hittable
{
	protected float maxSpeed = 100;
	protected float acceleration = 0;
	public float gravity = 0;
	protected float rotationSpeed = 0;
	protected int maxRicochets = 0;
	public float damage = 1;
	public float maxRange = 1000;
	protected float dropRange = 0;

	public Entity shooter;
	protected Item item;

	protected Sprite sprite;
	public Vector4 spriteColor = Vector4.One;
	protected bool additive = false;

	int ricochets = 0;
	protected Vector2 offset;
	public float currentDistance = 0;

	List<Entity> hitEntities = new List<Entity>();

	//Trail trail;
	protected Vector4 trailColor = Vector4.One;


	public Projectile(Vector2 velocity, Vector2 startVelocity, Vector2 offset, Entity shooter, Item item, float damage)
	{
		this.offset = offset;
		this.shooter = shooter;
		this.item = item;
		this.damage = damage;

		collider = new Hitbox(-0.1f, -0.1f, 0.2f, 0.2f);
		filterGroup = FILTER_PROJECTILE;

		this.velocity = velocity;
		if (MathF.Sign(velocity.x) == MathF.Sign(startVelocity.x) && MathF.Abs(startVelocity.x) > MathF.Abs(velocity.x))
			velocity.x = startVelocity.x;
		//velocity += (Vector2.Dot(startVelocity, velocity) + 1.0f) * 0.5f * startVelocity * 0.05f;
	}

	public override void init(Level level)
	{
		//trail = new Trail(20, trailColor, position);
		dropRange *= shooter is Player ? ((Player)shooter).getProjectileRangeModifier() : 1;
		maxRange *= shooter is Player ? ((Player)shooter).getProjectileRangeModifier() : 1;
	}

	public virtual void onHit(Vector2 normal)
	{
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if (by != null)
		{
			onHit((position - by.position).normalized);
			return true;
		}
		return false;
	}

	public override void update()
	{
		velocity += velocity.normalized * acceleration * Time.deltaTime;
		if (currentDistance >= dropRange || ricochets > 0)
			velocity.y += gravity * Time.deltaTime;
		if (velocity.length > maxSpeed)
			velocity = velocity.normalized * maxSpeed;

		Vector2 displacement = velocity * Time.deltaTime;
		position += displacement;
		currentDistance += displacement.length;

		if (currentDistance >= maxRange)
		{
			GameState.instance.level.addEntity(new BulletDisappearEffect(), position);
			remove();
		}

		if (rotationSpeed > 0)
			rotation += rotationSpeed * Time.deltaTime;
		else
			rotation = MathF.Atan2(velocity.y, velocity.x);

		offset = Vector2.Lerp(offset, Vector2.Zero, 5 * Time.deltaTime);

		//trail.update();
		//trail.setPosition(position + offset);

		bool hasHit = GameState.instance.level.raycast(position - displacement, displacement.normalized, displacement.length, out HitData hit, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (!hasHit)
			hasHit = GameState.instance.level.sweep(position - displacement, collider, displacement.normalized, displacement.length, out hit, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (hasHit && (hit.entity == null || hit.entity != shooter && !hitEntities.Contains(hit.entity) && hit.entity is Hittable))
		{
			if (hit.entity != null)
			{
				Hittable hittable = hit.entity as Hittable;
				hitEntities.Add(hit.entity);

				Player player = GameState.instance.player;

				bool critical = false;
				if (hit.entity is Mob)
				{
					Mob mob = hit.entity as Mob;
					critical = mob.isStunned && mob.criticalStun
					|| Random.Shared.NextSingle() < player.criticalChance * player.getCriticalChanceModifier()
						|| mob.ai.target != player && player.getStealthAttackModifier() > 1;
				}
				if (critical)
					damage *= player.getCriticalAttackModifier();

				if (hittable.hit(damage, this, item, null, true, critical))
				{
					//onHit(hit.normal);

					if (hit.entity is Mob)
					{
						Mob mob = hit.entity as Mob;
						Vector2 knockback = (hit.entity.position - position).normalized * (item != null ? item.knockback : 8);
						mob.addImpulse(knockback);

						if (shooter is Player)
							player.onEnemyProjectileHit(mob, damage, critical);
					}

					//remove();
				}
			}

			if (ricochets >= maxRicochets || hit.entity != null)
			{
				onHit(hit.normal);
				remove();
			}
			else
			{
				position = position - displacement + displacement.normalized * hit.distance;
				velocity = Vector2.Reflect(velocity, hit.normal);
				velocity = velocity.normalized * MathF.Min(velocity.length, 10);
				position += velocity.normalized * hit.distance;
				damage *= 0.5f;
				ricochets++;
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f + offset.x, position.y - 0.5f + offset.y, 0.001f, 1, 1, rotation, sprite, false, spriteColor, additive);

		//trail.render();
	}
}
