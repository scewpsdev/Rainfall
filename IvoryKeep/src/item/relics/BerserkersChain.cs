using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BerserkersChain : Item
{
	int buffLevel = 0;
	int threshhold = 10;

	long lastTick = -1;
	long lastKill = -1;


	public BerserkersChain()
		: base("berserkers_chain", ItemType.Relic)
	{
		displayName = "Berserker's Chain";
		description = "Consecutive kills temporarily increase attack speed";
		stackable = true;
		tumbles = false;
		canDrop = false;

		value = 25;

		sprite = new Sprite(tileset, 9, 6);

		buff = new ItemBuff(this);
		buff.auraColor = 0xFFd82b2b;
	}

	public override void onEquip(Player player)
	{
		lastKill = Time.currentTime;
	}

	public override void onKill(Player player, Mob mob)
	{
		int lastBuffLevel = buffLevel;
		buffLevel = Math.Min(buffLevel + 3, (2 - 1) * 20 + threshhold);
		if (buffLevel > threshhold && lastBuffLevel <= threshhold)
			buff.active = true;

		buff.attackSpeedModifier = damageMultiplier;

		lastKill = Time.currentTime;
	}

	float damageMultiplier => 1 + MathF.Max(buffLevel - threshhold, 0) * 0.05f;

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			if ((Time.currentTime - lastTick) / 1e9f > 5)
			{
				lastTick = Time.currentTime;

				if (buff.active)
				{
					int lastBuffLevel = buffLevel;
					int cooldown = (int)Math.Ceiling((Time.currentTime - lastKill) / 1e9f / 4);
					buffLevel = Math.Max(buffLevel - cooldown, 0);
					if (buffLevel <= threshhold && lastBuffLevel > threshhold)
						buff.active = false;

					buff.attackSpeedModifier = damageMultiplier;
				}
			}

			buff.auraStrength = damageMultiplier - 1;
		}
	}
}
