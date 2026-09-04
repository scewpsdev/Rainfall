using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfDexterity : Item
{
	public RingOfDexterity()
		: base("ring_of_dexterity", ItemType.Relic)
	{
		displayName = "Ring of Dexterity";

		description = "Increases attack speed by 10%";
		baseValue = 60;
		maxUpgradeLevel = 3;

		sprite = new Sprite(tileset, 14, 6);

		buff = new ItemBuff(this) { attackSpeedModifier = 1.1f };
	}

	public override void upgrade()
	{
		base.upgrade();
		buff.attackSpeedModifier = 1.1f + upgradeLevel * 0.1f;
	}
}
