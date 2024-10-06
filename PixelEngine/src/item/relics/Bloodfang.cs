using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bloodfang : Item
{
	float amount = 0.04f;

	public Bloodfang()
		: base("bloodfang", ItemType.Relic)
	{
		displayName = "Bloodfang";
		description = "Steals a portion of health from enemies with each attack";
		stackable = true;
		tumbles = false;

		value = 54;

		sprite = new Sprite(tileset, 15, 6);
	}

	public override void onEnemyHit(Player player, Mob mob, float damage)
	{
		player.heal(damage * amount);
	}
}
