using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PoisonVial : Item
{
	public PoisonVial()
		: base("poison_vial", ItemType.Potion)
	{
		displayName = "Poison Vial";
		stackable = true;
		value = 11;
		canDrop = false;

		sprite = new Sprite(tileset, 5, 5);
	}

	public override bool use(Player player)
	{
		player.addStatusEffect(new PoisonEffect(1, 16));
		player.hud.showMessage("The water burns on your tongue.");
		player.removeItemSingle(this);
		player.giveItem(new GlassBottle());
		return false;
	}
}
