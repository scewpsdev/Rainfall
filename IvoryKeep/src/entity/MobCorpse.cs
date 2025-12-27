using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MobCorpse : Object, Interactable
{
	SpriteAnimator animator;
	int direction;
	bool renderLight;

	List<Item> passiveItems;

	bool particlesEmitted = false;


	public MobCorpse(Sprite sprite, Vector4 spriteColor, SpriteAnimator animator, FloatRect rect, int direction, Vector2 velocity, float impulseVelocity, FloatRect collider, bool renderLight = false, List<Item> passiveItems = null)
		: base()
	{
		this.sprite = sprite;
		this.spriteColor = spriteColor;
		this.animator = animator;
		this.rect = rect;
		this.direction = direction;
		this.renderLight = renderLight;
		this.passiveItems = passiveItems;

		this.velocity = velocity * 0.5f + new Vector2(impulseVelocity, 0);

		this.collider = collider;
		filterGroup = FILTER_DECORATION;
		canPickUp = false;

		this.velocity.y = MathF.Max(this.velocity.y, 5);
		gravity = -30;

		numRestRotations = 1;
		tumbles = false;
		hitsEnemies = false;
		hitsPlayer = false;
		damage = 1;
		bounciness = 0.3f;

		displayName = "Corpse";
	}

	public override void update()
	{
		base.update();

		/*
		velocity.y += gravity * Time.deltaTime;

		if (isGrounded)
			velocity.x = Mathf.Lerp(velocity.x, 0, 4 * Time.deltaTime);

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, false);

		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (velocity.y < 0)
				isGrounded = true;
			velocity.y = -0.3f * velocity.y;
		}
		if ((collisionFlags & Level.COLLISION_X) != 0)
		{
			velocity.x = -0.5f * velocity.x;
		}

		position += displacement;
		*/


		if (isGrounded && !particlesEmitted)
		{
			GameState.instance.level.addEntity(ParticleEffects.CreateDeathEffect(this, MathF.Sign(velocity.x)), position);
			particlesEmitted = true;
		}

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
		/*
			Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, LAYER_BG, rect.size.x, rect.size.y, 0, sprite, direction == -1, spriteColor);

			if (passiveItems != null)
			{
				for (int i = 0; i < passiveItems.Count; i++)
				{
					if (passiveItems[i].ingameSprite != null)
					{
						Renderer.DrawSprite(position.x + rect.min.x * passiveItems[i].ingameSpriteSize, position.y + rect.min.y * passiveItems[i].ingameSpriteSize - 0.5f, LAYER_PLAYER_ARMOR, rect.size.x * passiveItems[i].ingameSpriteSize, rect.size.y * passiveItems[i].ingameSpriteSize, 0, passiveItems[i].ingameSprite, direction == -1, passiveItems[i].ingameSpriteColor);
					}
				}
			}
		*/

		base.render();

		if (renderLight)
		{
			Renderer.DrawLight(position + new Vector2(0, 0.5f), new Vector3(1.0f) * 1.5f, 2);
		}
	}
}
