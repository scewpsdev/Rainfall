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

		description = "Increases attack speed by 20%";
		value = 120;

		sprite = new Sprite(tileset, 9, 0);

		buff = new ItemBuff() { attackSpeedModifier = 1.2f };
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
		buff.attackSpeedModifier = 1.2f + 0.2f * upgradeLevel;
	}
}
