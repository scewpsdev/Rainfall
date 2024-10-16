using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BerserkersChain : Item
{
	int buffLevel = 0;
	int threshhold = 8;

	long lastTick = -1;
	long lastKill = -1;


	public BerserkersChain()
		: base("berserkers_chain", ItemType.Relic)
	{
		displayName = "Berserker's Chain";
		description = "Consecutive kills temporarily increase attack";
		stackable = true;
		tumbles = false;
		canDrop = false;

		value = 25;

		sprite = new Sprite(tileset, 9, 6);

		buff = new ItemBuff();
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
			onActivate(player);

		buff.meleeDamageModifier = damageMultiplier;

		lastKill = Time.currentTime;
	}

	float damageMultiplier => 1 + MathF.Max(buffLevel - threshhold, 0) * 0.05f;

	void onActivate(Player player)
	{
		player.itemBuffs.Add(buff);
	}

	void onDeactivate(Player player)
	{
		player.itemBuffs.Remove(buff);
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			if ((Time.currentTime - lastTick) / 1e9f > 5)
			{
				lastTick = Time.currentTime;

				int lastBuffLevel = buffLevel;
				int cooldown = (int)Math.Ceiling((Time.currentTime - lastKill) / 1e9f / 5);
				buffLevel = Math.Max(buffLevel - cooldown, 0);
				if (buffLevel <= threshhold && lastBuffLevel > threshhold)
					onDeactivate(player);

				buff.meleeDamageModifier = damageMultiplier;
			}

			buff.auraStrength = damageMultiplier;
		}
	}
}
