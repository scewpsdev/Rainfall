using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


public class ExplosiveCrate : ExplosiveObject
{
	public ExplosiveCrate()
	{
		displayName = "Explosive Crate";

		sprite = new Sprite(tileset, 9, 1);
		platformCollider = true;

		health = 3;

		hitSound = Item.woodHit;
		breakSound = [Resource.GetSound("sounds/break_wood.ogg")];
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
