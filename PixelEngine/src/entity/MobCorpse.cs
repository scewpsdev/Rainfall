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
	bool isGrounded = false;

	bool particlesEmitted = false;


	public MobCorpse(Mob mob)
	{
		sprite = mob.sprite;
		rect = mob.rect;
		direction = mob.direction;

		velocity = mob.velocity * 0.0f + mob.impulseVelocity;

		collider = mob.collider;
		filterGroup = FILTER_DECORATION;

		velocity.y = MathF.Max(velocity.y, 5);
		gravity = -30;

		mob.animator.setAnimation("dead");
		mob.animator.update(mob.sprite);
	}

	public override void update()
	{
		velocity.y += gravity * Time.deltaTime;

		if (isGrounded)
			velocity.x = MathHelper.Lerp(velocity.x, 0, 4 * Time.deltaTime);

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, false);

		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (velocity.y < 0)
				isGrounded = true;
			velocity.y = -0.5f * velocity.y;
		}
		if ((collisionFlags & Level.COLLISION_X) != 0)
		{
			velocity.x = -0.5f * velocity.x;
		}

		if (isGrounded && !particlesEmitted)
		{
			GameState.instance.level.addEntity(Effects.CreateDeathEffect(this, MathF.Sign(velocity.x)), position);
			particlesEmitted = true;
		}

		position += displacement;
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, 0, rect.size.x, rect.size.y, 0, sprite, direction == -1, 0xFF7F7F7F);
	}
}
