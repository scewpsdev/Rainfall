using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfThorns : Item
{
	public RingOfThorns()
		: base("ring_of_thorns", ItemType.Relic)
	{
		displayName = "Ring of Thorns";
		description = "Damages attacker upon taking a hit";

		stackable = false;

		baseValue = 33;
		maxUpgradeLevel = 3;

		sprite = new Sprite(tileset, 11, 6);
	}

	public override void onPlayerHit(Player player, Entity by, float damage)
	{
		if (by is Hittable)
		{
			Hittable hittable = by as Hittable;
			float amount = 0.5f + upgradeLevel * 0.2f;
			hittable.hit(damage * amount, player, this);
		}
	}
}
