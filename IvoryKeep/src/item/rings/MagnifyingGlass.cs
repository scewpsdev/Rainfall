using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagnifyingGlass : Item
{
	public MagnifyingGlass()
		: base("magnifying_glass", ItemType.Relic)
	{
		displayName = "Magnifying Glass";
		description = "Increases projectile accuracy";
		stackable = true;

		baseValue = 28;
		maxUpgradeLevel = 3;

		sprite = new Sprite(tileset, 0, 7);

		buff = new ItemBuff(this) { accuracyModifier = 2 };
	}

	public override void upgrade()
	{
		base.upgrade();
		buff.accuracyModifier = 2 + upgradeLevel * 0.5f;
	}
}
