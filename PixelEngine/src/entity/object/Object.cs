using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Object : Entity, Hittable, Interactable
{
	public float gravity = -20;
	public float bounciness = 0.5f;

	public float health = 1.5f;
	public float damage = 4;

	public bool tumbles = true;
	public float rotationVelocity = 0;
	public int numRestRotations = 4;

	public long throwTime = -1;

	protected Sprite sprite;
	protected FloatRect rect;

	protected Sound[] hitSound;

	uint outline = 0;


	public Object()
	{
		rect = new FloatRect(-0.5f, 0, 1, 1);
		collider = new FloatRect(-0.5f, 0, 1, 1);
		filterGroup = FILTER_DEFAULT | FILTER_OBJECT;
	}

	public override void init(Level level)
	{
		level.addCollider(this);
	}

	public override void destroy()
	{
		level.removeCollider(this);
	}

	public virtual bool canInteract(Player player)
	{
		return player.isDucked && player.carriedObject == null;
	}

	public virtual void interact(Player player)
	{
		player.carryObject(this);
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	public virtual bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		health -= damage;

		if (by != null)
		{
			Vector2 knockback = (position - by.position + (by.collider != null ? by.collider.center : Vector2.Zero)).normalized * 2;
			velocity += knockback;
			rotationVelocity = MathHelper.RandomFloat(-1, 1) * 10;
		}

		if (hitSound != null)
			Audio.PlayOrganic(hitSound, new Vector3(position, 0));

		return true;
	}

	protected virtual void onCollision(bool x, bool y, bool isEntity)
	{
		if (x)
		{
			position.x -= velocity.x * Time.deltaTime;
			velocity.x = -velocity.x * bounciness;
		}
		if (y)
		{
			if (velocity.y < -4)
				velocity.x += MathHelper.RandomFloat(-0.1f, 0.1f) * bounciness;

			//position.y -= velocity.y * Time.deltaTime;
			velocity.y = -velocity.y * bounciness;
			velocity.x *= bounciness;
		}

		if (velocity.lengthSquared > 4 && tumbles)
			rotationVelocity = MathHelper.RandomFloat(-1, 1) * 10;
	}

	public override void update()
	{
		velocity.y += gravity * Time.deltaTime;

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, false);
		position += displacement;

		bool collidesX = (collisionFlags & Level.COLLISION_X) != 0;
		bool collidesY = (collisionFlags & Level.COLLISION_Y) != 0;
		if (collidesX || collidesY)
		{
			onCollision(collidesX, collidesY, false);
		}

		if ((throwTime == -1 || (Time.currentTime - throwTime) / 1e9f > 0.1f) && velocity.length > 8)
		{
			HitData[] hits = new HitData[32];
			int numHits = GameState.instance.level.overlap(position + collider.min, position + collider.max, hits, FILTER_MOB | FILTER_PLAYER | FILTER_PROJECTILE | FILTER_OBJECT);
			bool hitEntity = false;
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].entity != null && hits[i].entity != this)
				{
					if (hits[i].entity is Hittable)
					{
						Hittable hittable = hits[i].entity as Hittable;
						if (hittable.hit(damage, this, null, displayName))
							hitEntity = true;
					}
				}
			}
			if (hitEntity)
				onCollision(true, true, true);
		}

		// Tumble
		if (velocity.lengthSquared > 0.25f)
		{
			rotation += rotationVelocity * Time.deltaTime;
		}
		else
		{
			float dst = rotation / (2 * MathF.PI) * numRestRotations;
			dst = MathF.Round(dst) / numRestRotations * 2 * MathF.PI;
			rotation = MathHelper.Lerp(rotation, dst, 5 * Time.deltaTime);
		}
	}

	public override void render()
	{
		if (sprite != null)
		{
			Matrix transform = Matrix.CreateTranslation(position.x + collider.center.x, position.y + collider.center.y, LAYER_BG) *
				Matrix.CreateRotation(Vector3.UnitZ, rotation) *
				Matrix.CreateTranslation(rect.position.x - collider.center.x + 0.5f * rect.size.x, rect.position.y - collider.center.y + 0.5f * rect.size.y, 0);
			if (outline != 0)
				Renderer.DrawOutline(rect.size.x, rect.size.y, transform, sprite, false, outline);
			Renderer.DrawSprite(rect.size.x, rect.size.y, transform, sprite);
		}
	}
}
