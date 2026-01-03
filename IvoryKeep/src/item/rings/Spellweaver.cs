using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spellweaver : Item
{
	public Spellweaver()
		: base("spellweaver", ItemType.Relic)
	{
		displayName = "Spellweaver";
		description = "Reduces spell mana cost";
		tumbles = false;

		baseValue = 27;
		maxUpgradeLevel = 3;

		sprite = new Sprite(tileset, 15, 7);

		buff = new ItemBuff(this) { manaCostModifier = 0.8f };
	}

	public override void upgrade()
	{
		base.upgrade();
		buff.manaCostModifier = 0.8f - upgradeLevel * 0.1f;
	}
}
