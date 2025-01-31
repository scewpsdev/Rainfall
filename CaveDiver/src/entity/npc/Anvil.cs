using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Anvil : NPC
{
	public Anvil()
		: base("anvil")
	{
		displayName = "Anvil";

		sprite = new Sprite(tileset, 1, 2);
		collider = new FloatRect(-0.5f, 0, 1, 10.0f / 16);
		platformCollider = true;
		filterGroup = FILTER_OBJECT;

		canUpgrade = true;
		turnTowardsPlayer = false;
	}

	public override void init(Level level)
	{
		level.addEntityCollider(this);
	}

	public override void destroy()
	{
		level.removeEntityCollider(this);
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		return false;
	}
}
