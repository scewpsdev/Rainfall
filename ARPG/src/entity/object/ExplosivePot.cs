using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ExplosivePot : ExplosiveObject
{
	public ExplosivePot()
	{
		displayName = "Explosive Pot";

		sprite = new Sprite(tileset, 13, 0, 1, 2);
		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 1.0f);
		platformCollider = true;
		tumbles = false;
		health = 1;

		breakSound = Resource.GetSounds("sounds/break_pot", 4);
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		base.hit(damage, by, item, byName, triggerInvincibility);
		if (health > 0)
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyPotEffect(0xFF6e484d), position);
		else
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyPotEffect(0xFF6e484d, 20, velocity * 0.5f), position);
		return true;
	}
}
