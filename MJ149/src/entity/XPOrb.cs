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
	Vector2 velocity;


	public XPOrb(Vector2 position, Entity target)
	{
		this.position = position;
		this.target = target;

		collider = new FloatRect(-0.5f * SIZE, -0.5f * SIZE, SIZE, SIZE);
		hitbox = new FloatRect(-0.5f * SIZE, -0.5f * SIZE, SIZE, SIZE);
	}

	public void touch(Entity entity)
	{
		if (target == entity)
		{
			if (entity is Player)
			{
				Player player = entity as Player;
				player.points += 10;
			}
			removed = true;
		}
	}

	public override void update()
	{
		Vector2 targetPosition = target.position;
		float distance = (targetPosition - position).length;
		Vector2 direction = (targetPosition - position) / distance;
		float acceleration = Math.Max(10.0f / distance, 10.0f);
		velocity += direction * acceleration * Time.deltaTime;
		float speed = velocity.length;
		if (speed > MAX_SPEED)
			velocity = velocity.normalized * MAX_SPEED;
		position += velocity * Time.deltaTime;
		position += direction * 5 * Time.deltaTime;
	}

	public override void draw()
	{
		Renderer.DrawSprite(position.x - 0.5f * SIZE, position.y - 0.5f * SIZE, 1, SIZE, SIZE, 0, null, false, 0xFFFFFF77);
	}
}
