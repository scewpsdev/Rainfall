using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GoldenApple : Item
{
	public GoldenApple()
		: base("golden_apple", ItemType.Food)
	{
		displayName = "Golden Apple";
		stackable = true;

		value = 30;

		sprite = new Sprite(tileset, 4, 2);

		useSound = [Resource.GetSound("sounds/eat.ogg")];
	}

	public override bool use(Player player)
	{
		base.use(player);
		player.addStatusEffect(new HealStatusEffect(player.maxHealth, 8 / (1 + upgradeLevel)));
		player.addStatusEffect(new ManaRechargeEffect(player.maxMana, 8 / (1 + upgradeLevel)));
		GameState.instance.level.addEntity(ParticleEffects.CreateConsumableUseEffect(player, player.direction, 0xFFe3b85c), player.position);
		return true;
	}
}
