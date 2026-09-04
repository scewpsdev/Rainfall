using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GlassRing : Item
{
	public GlassRing()
		: base("glass_ring", ItemType.Relic)
	{
		displayName = "Glass Ring";
		description = "Doubles attack, halves defense";
		stackable = false;

		baseValue = 45;
		maxUpgradeLevel = 1;
		spawnWithUpgrades = false;

		sprite = new Sprite(tileset, 10, 6);

		buff = new ItemBuff(this) { meleeDamageModifier = 2, magicDamageModifier = 2, defenseModifier = 0.5f };
	}

	public override void upgrade()
	{
		base.upgrade();
		if (upgradeLevel == 1)
		{
			buff.defenseModifier = 0.01f;
			description = "Doubles attack, every hit is lethal";
		}
	}
}
