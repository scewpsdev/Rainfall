using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SapphireRing : Item
{
	public SapphireRing()
		: base("sapphire_ring", ItemType.Relic)
	{
		displayName = "Sapphire Ring";

		description = "Increases mana recovery rate";
		baseValue = 25;
		maxUpgradeLevel = 3;

		sprite = new Sprite(tileset, 13, 5);

		buff = new ItemBuff(this) { manaRecoveryModifier = 1.5f };
	}

	public override void upgrade()
	{
		base.upgrade();
		buff.manaRecoveryModifier = 1.5f + upgradeLevel * 0.5f;
	}
}
