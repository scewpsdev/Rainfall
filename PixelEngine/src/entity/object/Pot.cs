using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Pot : Container
{
	public Pot(params Item[] items)
		: base(items)
	{
		displayName = "Pot";

		sprite = Random.Shared.NextSingle() < 0.5f ? new Sprite(tileset, 11, 0, 1, 2) : new Sprite(tileset, 12, 0, 1, 2);
		rect = new(-0.5f, 0, 1, 2);
		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 1.0f);
		platformCollider = true;
		tumbles = false;
		health = 1;

		breakSound = Resource.GetSounds("sounds/break_pot", 4);
	}

	public Pot()
		: this(null)
	{
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		base.hit(damage, by, item, byName, triggerInvincibility);
		if (health > 0)
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyPotEffect(0xFF8e5252), position);
		else
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyPotEffect(0xFF8e5252, 20, velocity), position);
		return true;
	}
}
