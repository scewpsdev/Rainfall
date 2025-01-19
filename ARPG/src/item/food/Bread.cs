using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bread : Item
{
	public Bread()
		: base("Bread", ItemType.Food)
	{
		displayName = "Bread";
		stackable = true;

		value = 8;

		sprite = new Sprite(tileset, 7, 2);

		useSound = [Resource.GetSound("sounds/eat.ogg")];
	}

	public override bool use(Player player)
	{
		base.use(player);
		player.addStatusEffect(new HealStatusEffect(0.75f, 10));
		player.addStatusEffect(new ManaRechargeEffect(2, 10));
		GameState.instance.level.addEntity(ParticleEffects.CreateConsumableUseEffect(player, player.direction, 0xFF967b4b), player.position);
		return true;
	}
}
