using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DynamiteObject : ExplosiveObject
{
	public DynamiteObject()
	{
		displayName = "Dynamite";

		sprite = new Sprite(tileset, 10, 1);
		collider = new Hitbox(-0.25f, 0.0f, 0.5f, 0.6f);
		//platformCollider = true;

		hitSound = Item.woodHit;
		breakSound = [Resource.GetSound("sounds/fuse.ogg")];
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		base.hit(damage, by, item, byName, triggerInvincibility, buffedHit);

		if (health > 0)
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF4c3f46), position);
		else
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF4c3f46, 20, velocity * 0.5f), position);

		return true;
	}
}
