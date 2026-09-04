using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public class Barrel : Container
{
	public Barrel(params Item[] items)
		: base(items)
	{
		displayName = "Barrel";

		health = 1;

		sprite = new Sprite(tileset, 0, 1);
		collider = new Hitbox(-0.4f, 0.0f, 0.8f, 0.75f);
		platformCollider = true;

		hitSound = Item.woodHit;
		breakSound = Item.woodBreak;
	}

	public Barrel()
		: this(null)
	{
	}

	protected override void breakContainer()
	{
		base.breakContainer();

		GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF675051, 20, velocity * 0.25f), position);
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		base.hit(damage, by, item, byName, triggerInvincibility);
		if (health > 0)
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF675051), position);
		return true;
	}
}
