using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Barrel : Container
{
	public Barrel(params Item[] items)
		: base(items)
	{
		displayName = "Barrel";

		sprite = new Sprite(tileset, 0, 1);
		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 0.75f);
		platformCollider = true;

		hitSound = Item.woodHit;
		breakSound = [Resource.GetSound("sounds/break_wood.ogg")];
	}

	public Barrel()
		: this(null)
	{
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		base.hit(damage, by, item, byName, triggerInvincibility);
		if (health > 0)
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF675051), position);
		else
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF675051, 20, velocity * 0.25f), position);
		return true;
	}
}
