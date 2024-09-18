using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionOfGreaterHealing : Item
{
	public float healAmount = 2.0f;

	public PotionOfGreaterHealing()
		: base("potion_of_greater_healing", ItemType.Potion)
	{
		displayName = "Potion of Greater Healing";
		//stackable = true;

		value = 80;

		sprite = new Sprite(tileset, 7, 0);
	}

	public override bool use(Player player)
	{
		if (player.health < player.maxHealth - 0.1f)
			player.health = MathF.Min(player.health + healAmount, player.maxHealth);
		else
			player.health = ++player.maxHealth;
		player.removeItemSingle(this);
		player.giveItem(new GlassBottle());
		return false;
	}
}
