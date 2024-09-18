using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HealEffect : PotionEffect
{
	float amount;
	float duration;

	public HealEffect(float amount = 1.5f, float duration = 2)
		: base("Healing", 23, new Sprite(Item.tileset, 7, 0))
	{
		this.amount = amount;
		this.duration = duration;
	}

	public override void apply(Player player, Potion potion)
	{
		if (player.health < player.maxHealth - 0.1f)
			player.addStatusEffect(new HealStatusEffect(amount, duration));
		else
			player.health = player.maxHealth = player.maxHealth + MathF.Round(0.5f * amount);
		if (Random.Shared.NextSingle() < 0.5f)
			player.hud.showMessage("You feel refreshed.");
		else
			player.hud.showMessage("You feel your strength returning.");
	}
}

public class PotionOfHealing : Potion
{
	public PotionOfHealing(float amount)
		: base("potion_of_healing")
	{
		addEffect(new HealEffect(amount));

		displayName = "Potion of Healing";
		//stackable = true;

		value = 25;

		sprite = new Sprite(tileset, 7, 0);
	}

	public PotionOfHealing()
		: this(1.5f)
	{
	}
}
