using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MoonFruit : Item
{
	public MoonFruit()
		: base("moon_fruit", ItemType.Food)
	{
		displayName = "Moon Fruit";

		value = 72;

		sprite = new Sprite(tileset, 13, 4);

		useSound = [Resource.GetSound("res/sounds/eat.ogg")];
	}

	public override bool use(Player player)
	{
		base.use(player);
		player.maxMana += 1 + upgradeLevel;
		player.addStatusEffect(new ManaRechargeEffect(player.maxMana, 3.0f));
		GameState.instance.level.addEntity(Effects.CreateConsumableUseEffect(player, player.direction, 0xFFa6f1cc), player.position);
		return true;
	}
}
