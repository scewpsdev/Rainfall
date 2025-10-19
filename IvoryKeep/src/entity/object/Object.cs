using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

	protected bool hitsEnemies = true;
	protected bool hitsPlayer = true;

	public bool isGrounded;

	public long throwTime = -1;
	public Entity thrower;

	protected Sprite sprite;
	protected Vector4 spriteColor = Vector4.One;
	protected FloatRect rect;
	public bool flipped;

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
		level.addEntityCollider(this);
	}

	public override void destroy()
	{
		level.removeEntityCollider(this);
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

	public void addImpulse(Vector2 impulse)
	{
		velocity += impulse;
	}

	public virtual bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		health -= damage;

		if (by != null)
		{
			Vector2 knockback = (position - by.position + (by.collider != null ? by.collider.center : Vector2.Zero)).normalized * 2;
			velocity += knockback;
			rotationVelocity = Mathf.RandomFloat(-1, 1) * 10;
		}

		if (hitSound != null)
			Audio.PlayOrganic(hitSound, new Vector3(position, 0));

		return true;
	}

	protected virtual void onCollision(bool x, bool y, bool isEntity)
	{
		if (!isEntity)
		{
			if (x)
			{
				position.x -= velocity.x * Time.deltaTime;
				velocity.x = -velocity.x * bounciness;
			}
			if (y)
			{
				if (velocity.y < -4)
					velocity.x += Mathf.RandomFloat(-0.1f, 0.1f) * bounciness;
				if (velocity.y < 0)
					isGrounded = true;

				//position.y -= velocity.y * Time.deltaTime;
				velocity.y = -velocity.y * bounciness;
				//velocity.x *= bounciness;
			}

			if (velocity.lengthSquared > 4 && tumbles)
				rotationVelocity = Mathf.RandomFloat(-1, 1) * 10;
		}
		else
		{
			if (x)
			{
				//velocity.x *= (1 - bounciness);
				//position.x -= velocity.x * Time.deltaTime;
				//velocity.x = -velocity.x * bounciness;
			}
			if (y)
			{
				if (velocity.y < -4)
					velocity.x += Mathf.RandomFloat(-0.1f, 0.1f) * bounciness;
				if (velocity.y < 0)
					isGrounded = true;

				//position.y -= velocity.y * Time.deltaTime;
				//velocity.y = velocity.y * (1 - bounciness);
				//velocity.x *= bounciness;
			}

			if (velocity.lengthSquared > 4 && tumbles)
				rotationVelocity = Mathf.RandomFloat(-1, 1) * 10;
		}
	}

	public override void update()
	{
		velocity.y += gravity * Time.deltaTime;

		isGrounded = false;

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, false);
		position += displacement;

		bool collidesX = (collisionFlags & Level.COLLISION_X) != 0;
		bool collidesY = (collisionFlags & Level.COLLISION_Y) != 0;
		if (collidesX || collidesY)
		{
			onCollision(collidesX, collidesY, false);
		}

		if (isGrounded)
			velocity.x = Mathf.Lerp(velocity.x, 0, 8 * Time.deltaTime);

		if (velocity.length > 8)
		{
			HitData[] hits = new HitData[32];
			int numHits = GameState.instance.level.overlap(position + collider.min, position + collider.max, hits, FILTER_PROJECTILE | FILTER_OBJECT | (hitsEnemies ? FILTER_MOB : 0) | (hitsPlayer ? FILTER_PLAYER : 0));
			bool hitEntity = false;
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].entity != null && hits[i].entity != this && (hits[i].entity != thrower || (Time.currentTime - throwTime) / 1e9f > 0.2f))
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
		if (tumbles)
		{
			if (velocity.lengthSquared > 0.25f)
			{
				rotation += rotationVelocity * Time.deltaTime;
			}
			else
			{
				Debug.Assert(numRestRotations > 0);
				float dst = rotation / (2 * MathF.PI) * numRestRotations;
				dst = MathF.Round(dst) / numRestRotations * 2 * MathF.PI;
				rotation = Mathf.LerpAngle(rotation, dst, 5 * Time.deltaTime);
			}
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
				Renderer.DrawOutline(rect.size.x, rect.size.y, transform, sprite, flipped, outline);
			Renderer.DrawSprite(rect.size.x, rect.size.y, transform, sprite, flipped, spriteColor);
		}
	}
}
