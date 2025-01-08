using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Crate : Barrel
{
	public float gravity = -20;
	public float bounciness = 0.5f;
	public float rotationVelocity = 0;


	public Crate(params Item[] items)
		: base(items)
	{
		sprite = new Sprite(tileset, 8, 1);
		rect = new FloatRect(-0.5f, 0, 1, 1);
		health = 3;

		collider = new FloatRect(-0.4f, 0.1f, 0.8f, 0.8f);
		filterGroup = FILTER_DEFAULT | FILTER_ITEM;
		//platformCollider = true;

		hitSound = Item.woodHit;
		breakSound = [Resource.GetSound("sounds/break_wood.ogg")];
	}

	public Crate()
		: this(null)
	{
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if (base.hit(damage, by, item, byName, triggerInvincibility, buffedHit))
		{
			if (by != null)
			{
				Vector2 knockback = (position - by.position + (by.collider != null ? by.collider.center : Vector2.Zero)).normalized * 2;
				velocity += knockback;
				rotationVelocity = MathHelper.RandomFloat(-1, 1) * 10;
			}
			return true;
		}
		return false;
	}

	protected override void onHit(bool x, bool y)
	{
		if (velocity.length > 10)
			breakBarrel();

		if (x)
		{
			position.x -= velocity.x * Time.deltaTime;
			velocity.x = -velocity.x * bounciness;
		}
		else if (y)
		{
			position.y -= velocity.y * Time.deltaTime;
			velocity.y = -velocity.y * bounciness;
			velocity.x *= bounciness;
			velocity.x += MathHelper.RandomFloat(-0.1f, 0.1f);
		}

		if (velocity.lengthSquared > 4)
			rotationVelocity = MathHelper.RandomFloat(-1, 1) * 10;
	}

	public override void update()
	{
		base.update();

		// Tumble
		if (velocity.lengthSquared > 0.25f)
		{
			rotation += rotationVelocity * Time.deltaTime;
		}
		else
		{
			float dst = rotation * 4;
			dst = MathF.Round(dst) / 4;
			rotation = MathHelper.Lerp(rotation, dst, 5 * Time.deltaTime);
		}
	}
}
