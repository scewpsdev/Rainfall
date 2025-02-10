using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Parsley : Item
{
	public Parsley()
		: base("parsley", ItemType.Food)
	{
		displayName = "Parsley";
		stackable = true;

		description = "Cures poison";

		value = 12;
		rarity = 0.1f;

		sprite = new Sprite(tileset, 12, 4);

		useSound = [Resource.GetSound("sounds/eat.ogg")];
	}

	public override bool use(Player player)
	{
		base.use(player);
		if (player.hasStatusEffect("poison", out StatusEffect effect))
			player.removeStatusEffect(effect);
		return true;
	}
}
