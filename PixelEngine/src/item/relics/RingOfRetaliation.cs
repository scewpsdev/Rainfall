using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfRetaliation : Item
{
	const float timeThreshhold = 5;

	long lastHit = -1;
	float damageTaken = 0;
	float recoveryAmount;


	public RingOfRetaliation()
		: base("ring_of_retaliation", ItemType.Relic)
	{
		displayName = "Ring of Retaliation";
		description = "Upon taking damage, restores health from counterattacks";
		sprite = new Sprite(tileset, 11, 6);
		stackable = false;
		value = 30;
	}

	public override void onHit(Player player, Entity by, float damage)
	{
		if ((Time.currentTime - lastHit) > timeThreshhold)
		{
			damageTaken = 0;
			recoveryAmount = 0;
		}

		lastHit = Time.currentTime;
		damageTaken += damage;
		recoveryAmount += 0.15f * damage;
	}

	public override void onEnemyHit(Player player, Mob mob, float damage)
	{
		if ((Time.currentTime - lastHit) / 1e9f < timeThreshhold)
		{
			if (damageTaken >= recoveryAmount)
			{
				player.heal(recoveryAmount);
				damageTaken -= recoveryAmount;
			}
		}
	}
}
