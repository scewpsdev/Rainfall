using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionOfEnergy : Item
{
	public float amount = 2;

	public PotionOfEnergy()
		: base("potion_of_energy", ItemType.Potion)
	{
		displayName = "Potion of Energy";
		stackable = true;

		value = 25;

		sprite = new Sprite(tileset, 6, 2);
	}

	public override bool use(Player player)
	{
		if (player.mana < player.maxMana)
			player.addStatusEffect(new ManaRechargeEffect(amount, 4));
		player.hud.showMessage("You feel energy flow through you.");
		return true;
	}
}
