using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Apple : Item
{
	public Apple()
		: base("apple", ItemType.Food)
	{
		displayName = "Apple";
		stackable = true;

		value = 4;

		sprite = new Sprite(tileset, 5, 2);

		useSound = [Resource.GetSound("sounds/eat.ogg")];
	}

	public override bool use(Player player)
	{
		base.use(player);
		player.addStatusEffect(new HealStatusEffect(0.5f, 10));
		player.addStatusEffect(new ManaRechargeEffect(2, 10));
		GameState.instance.level.addEntity(ParticleEffects.CreateConsumableUseEffect(player, player.direction, 0xFFb15848), player.position);
		return true;
	}
}
