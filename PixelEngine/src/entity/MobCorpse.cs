using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MobCorpse : Entity
{
	Sprite sprite;
	FloatRect rect;
	int direction;

	float gravity;
	Vector2 impulseVelocity;
	bool isGrounded;


	public MobCorpse(Mob mob)
	{
		sprite = mob.sprite;
		rect = mob.rect;
		direction = mob.direction;

		velocity = mob.velocity;
		impulseVelocity = mob.impulseVelocity;

		collider = mob.collider;

		velocity.y = 5;
		gravity = -30;

		mob.animator.setAnimation("dead");
		mob.animator.update(mob.sprite);
	}

	public override void update()
	{
		velocity.y += gravity * Time.deltaTime;

		if (isGrounded)
			velocity.x = MathHelper.Lerp(velocity.x, 0, 5 * Time.deltaTime);

		impulseVelocity.x = MathHelper.Lerp(impulseVelocity.x, 0, 8 * Time.deltaTime);
		if (MathF.Abs(impulseVelocity.x) < 0.01f)
			impulseVelocity.x = 0;
		if (MathF.Sign(impulseVelocity.x) == MathF.Sign(velocity.x))
			velocity.x = 0;
		velocity += impulseVelocity;

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, false);

		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (velocity.y < 0)
				isGrounded = true;
			velocity.y = 0;
			impulseVelocity.y = 0;
		}
		if ((collisionFlags & Level.COLLISION_X) != 0)
		{
			impulseVelocity.x = 0;
			impulseVelocity.y *= 0.5f;
		}

		position += displacement;
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, 0, rect.size.x, rect.size.y, 0, sprite, direction == -1, 0xFF7F7F7F);
	}
}
