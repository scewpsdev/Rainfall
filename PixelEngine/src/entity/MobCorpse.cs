using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MobCorpse : Entity
{
	Sprite sprite;
	SpriteAnimator animator;
	FloatRect rect;
	int direction;
	uint color;

	Item[] passiveItems;

	float gravity;
	bool isGrounded = false;

	bool particlesEmitted = false;


	public MobCorpse(Sprite sprite, SpriteAnimator animator, FloatRect rect, int direction, Vector2 velocity, Vector2 impulseVelocity, FloatRect collider, uint color, Item[] passiveItems = null)
	{
		this.sprite = sprite;
		this.animator = animator;
		this.rect = rect;
		this.direction = direction;
		this.color = color;
		this.passiveItems = passiveItems;

		this.velocity = velocity * 0.0f + impulseVelocity;

		this.collider = collider;
		filterGroup = FILTER_DECORATION;

		this.velocity.y = MathF.Max(this.velocity.y, 5);
		gravity = -30;
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
			velocity.y = 0.0f;
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


		if (animator.getAnimation("dead_falling") != null && !isGrounded)
			animator.setAnimation("dead_falling");
		else
			animator.setAnimation("dead");
		animator.update(sprite);
		if (passiveItems != null)
		{
			for (int i = 0; i < passiveItems.Length; i++)
			{
				if (passiveItems[i] != null && passiveItems[i].ingameSprite != null)
				{
					animator.update(passiveItems[i].ingameSprite);
					passiveItems[i].ingameSprite.position *= passiveItems[i].ingameSpriteSize;
				}
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, 0, rect.size.x, rect.size.y, 0, sprite, direction == -1, color);

		if (passiveItems != null)
		{
			for (int i = 0; i < passiveItems.Length; i++)
			{
				if (passiveItems[i] != null && passiveItems[i].ingameSprite != null)
				{
					Renderer.DrawSprite(position.x - 0.5f * passiveItems[i].ingameSpriteSize, position.y, LAYER_PLAYER_ARMOR, passiveItems[i].ingameSpriteSize, passiveItems[i].ingameSpriteSize, 0, passiveItems[i].ingameSprite, direction == -1, passiveItems[i].ingameSpriteColor);
				}
			}
		}
	}
}
