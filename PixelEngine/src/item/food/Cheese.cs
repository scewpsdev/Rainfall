using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Cheese : Item
{
	public Cheese()
		: base("cheese", ItemType.Food)
	{
		displayName = "Cheese";
		stackable = true;

		value = 12;

		sprite = new Sprite(tileset, 13, 0);

		useSound = [Resource.GetSound("res/sounds/eat.ogg")];
	}

	public override bool use(Player player)
	{
		base.use(player);
		player.addStatusEffect(new HealStatusEffect(1.5f + upgradeLevel * 0.5f, 5));
		GameState.instance.level.addEntity(Effects.CreateConsumableUseEffect(player, player.direction, 0xFFe9dd78), player.position);
		return true;
	}
}
