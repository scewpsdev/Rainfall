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

		description = "Increases attack speed by 25%";
		value = 60;

		sprite = new Sprite(tileset, 9, 0);

		buff = new ItemBuff(this) { attackSpeedModifier = 1.25f };
	}

	public override void onEquip(Player player)
	{
		player.itemBuffs.Add(buff);
	}

	public override void onUnequip(Player player)
	{
		player.itemBuffs.Remove(buff);
	}

	public override void upgrade()
	{
		base.upgrade();
		buff.attackSpeedModifier = 1.1f + 0.1f * upgradeLevel;
	}
}
