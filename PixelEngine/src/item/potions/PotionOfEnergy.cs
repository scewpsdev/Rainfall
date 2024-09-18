using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ManaEffect : PotionEffect
{
	public float amount;
	public float duration;

	public ManaEffect(float amount = 2, float duration = 4)
		: base("Energy", 23, new Sprite(Item.tileset, 6, 2))
	{
		this.amount = amount;
		this.duration = duration;
	}

	public override void apply(Player player, Potion potion)
	{
		if (player.mana < player.maxMana)
			player.addStatusEffect(new ManaRechargeEffect(amount, 4));
		player.hud.showMessage("You feel energy flow through you.");
	}
}

public class PotionOfEnergy : Potion
{
	public PotionOfEnergy()
		: base("potion_of_energy")
	{
		addEffect(new ManaEffect());

		displayName = "Potion of Energy";
		stackable = true;

		value = 25;

		sprite = new Sprite(tileset, 6, 2);
	}
}
