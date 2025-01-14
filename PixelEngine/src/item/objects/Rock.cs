using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rock : Object
{
	Sound[] hitSound;


	public Rock()
	{
		displayName = "Rock";

		damage = 4;
		health = 20;

		sprite = new Sprite(Item.tileset, 4, 0);
		collider = new FloatRect(-5 / 16.0f, 0, 10 / 16.0f, 7 / 16.0f);
		platformCollider = true;

		hitSound = Resource.GetSounds("sounds/hit_rock", 5);
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		base.hit(damage, by, item, byName, triggerInvincibility, buffedHit);

		if (hitSound != null)
			Audio.PlayOrganic(hitSound, new Vector3(position, 0));

		return true;
	}

	protected override void onCollision(bool x, bool y, bool isEntity)
	{
		if (isEntity)
			hit(velocity.length / 8);
		else if (velocity.length > 8)
			hit((velocity.length - 8) / 8);

		base.onCollision(x, y, isEntity);
	}
}
