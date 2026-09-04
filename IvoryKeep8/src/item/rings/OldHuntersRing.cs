using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class OldHuntersRing : Item
{
	public OldHuntersRing()
		: base("old_hunters_ring", ItemType.Relic)
	{
		displayName = "Old Hunter's Ring";

		description = "Increases projectile range";

		baseValue = 40;
		maxUpgradeLevel = 3;

		sprite = new Sprite(tileset, 0, 4);

		buff = new ItemBuff(this) { projectileRangeModifier = 1.5f };
	}

	public override void upgrade()
	{
		base.upgrade();
		buff.projectileRangeModifier = 1.5f + upgradeLevel * 0.5f;
	}
}
