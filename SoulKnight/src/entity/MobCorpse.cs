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
	Vector4 color;
	bool renderLight;

	List<Item> passiveItems;

	float gravity;
	bool isGrounded = false;

	bool particlesEmitted = false;


	public MobCorpse(Sprite sprite, SpriteAnimator animator, FloatRect rect, int direction, Vector2 velocity, Vector2 impulseVelocity, FloatRect collider, uint color, bool renderLight = false, List<Item> passiveItems = null)
	{
		this.sprite = sprite;
		this.animator = animator;
		this.rect = rect;
		this.direction = direction;
		this.color = color;
		this.renderLight = renderLight;
		this.passiveItems = passiveItems;

		this.velocity = velocity * 0.5f + impulseVelocity;

		this.collider = collider;
		filterGroup = FILTER_DECORATION;

		//this.velocity.y = MathF.Max(this.velocity.y, 5);
		gravity = -30;
	}

	public override void update()
	{
		zVelocity += gravity * Time.deltaTime;

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, false);

		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
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
		z += zVelocity * Time.deltaTime;
		if (z < 0)
		{
			z = 0;
			zVelocity *= -0.5f;
			isGrounded = true;
		}
		else
		{
			isGrounded = false;
		}

		if (isGrounded)
			velocity = Vector2.Lerp(velocity, Vector2.Zero, 4 * Time.deltaTime);


		if (animator.getAnimation("dead_falling") != null && !isGrounded)
			animator.setAnimation("dead_falling");
		else
			animator.setAnimation("dead");
		animator.update(sprite);
		if (passiveItems != null)
		{
			for (int i = 0; i < passiveItems.Count; i++)
			{
				if (passiveItems[i].ingameSprite != null)
				{
					animator.update(passiveItems[i].ingameSprite);
					passiveItems[i].ingameSprite.position *= passiveItems[i].ingameSpriteSize;
				}
			}
		}
	}

	public override void render()
	{
		Renderer.DrawVerticalSprite(position.x + rect.position.x, position.y + z, z + rect.position.y, rect.size.x, rect.size.y, sprite, direction == -1, color);

		if (passiveItems != null)
		{
			for (int i = 0; i < passiveItems.Count; i++)
			{
				if (passiveItems[i].ingameSprite != null)
				{
					Renderer.DrawVerticalSprite(position.x - 0.5f * passiveItems[i].ingameSpriteSize, position.y + z, z + 0.001f, passiveItems[i].ingameSpriteSize, passiveItems[i].ingameSpriteSize, passiveItems[i].ingameSprite, direction == -1, (Vector4)passiveItems[i].ingameSpriteColor);
				}
			}
		}

		if (renderLight)
		{
			Renderer.DrawLight(position + new Vector2(0, 0.5f), new Vector3(1.0f) * 1.5f, 7);
		}
	}
}
