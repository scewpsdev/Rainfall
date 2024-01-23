using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class XPOrb : Entity, Toucheable
{
	const float MAX_SPEED = 20;
	const float SIZE = 0.125f;


	Entity target;
	public Vector2 velocity;
	float accelMultiplier;
	int amount;


	public XPOrb(Vector2 position, Entity target, int amount)
	{
		this.position = position;
		this.target = target;
		this.amount = amount;

		if (!(target is Player))
			Debug.Assert(false);

		collider = new FloatRect(-0.5f * SIZE, -0.5f * SIZE, SIZE, SIZE);
		hitbox = new FloatRect(-0.5f * SIZE, -0.5f * SIZE, SIZE, SIZE);

		accelMultiplier = MathHelper.RandomFloat(4, 6);
	}

	public override void reset()
	{
		removed = true;
	}

	public void touch(Entity entity)
	{
		if (target == entity)
		{
			if (entity is Player)
			{
				Player player = entity as Player;
				player.points += amount;
				Roguelike.instance.manager.pointsEarned += amount;

				player.pointsCollected++;
			}
			removed = true;
		}
	}

	public override void update()
	{
		Vector2 targetPosition = target.position;
		float distance = (targetPosition - position).length;
		Vector2 direction = (targetPosition - position) / distance;
		float acceleration = Math.Max(accelMultiplier / distance, accelMultiplier);
		velocity += direction * acceleration * Time.deltaTime;
		float speed = velocity.length;
		if (speed > MAX_SPEED)
			velocity = velocity.normalized * MAX_SPEED;
		position += velocity * Time.deltaTime;
		position += direction * accelMultiplier * MathF.Max(3.0f / distance, 1) * Time.deltaTime;
	}

	public override void draw()
	{
		uint color = amount < 100 ? 0xFFFFFF77 : amount < 1000 ? 0xFFFF7777 : amount < 10000 ? 0xFF77FF77 : 0xFF7777FF;
		Renderer.DrawSprite(position.x - 0.5f * SIZE, position.y - 0.5f * SIZE, 1, SIZE, SIZE, 0, null, false, color);
	}
}
