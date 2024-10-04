using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BerserkersChain : Item
{
	int buffLevel = 0;
	int threshhold = 6;

	long lastTick = -1;
	long lastKill = -1;

	AttackModifier modifier;


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
	}

	public override void onEquip(Player player)
	{
		lastKill = Time.currentTime;
	}

	public override void onKill(Player player, Mob mob)
	{
		float preDmg = damageMultiplier;

		int lastBuffLevel = buffLevel;
		buffLevel += 3;
		if (buffLevel > threshhold && lastBuffLevel <= threshhold)
			onActivate(player);

		float postDmg = damageMultiplier;
		player.attackDamageModifier *= postDmg / preDmg;

		lastKill = Time.currentTime;
	}

	float damageMultiplier => 1 + MathF.Max(buffLevel - threshhold, 0) * 0.05f;

	void onActivate(Player player)
	{
		player.addStatusEffect(modifier = new AttackModifier(1));
	}

	void onDeactivate(Player player)
	{
		player.removeStatusEffect(modifier);
		modifier = null;
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			if ((Time.currentTime - lastTick) / 1e9f > 6)
			{
				lastTick = Time.currentTime;

				float preDmg = damageMultiplier;

				int lastBuffLevel = buffLevel;
				int cooldown = (int)Math.Ceiling((Time.currentTime - lastKill) / 1e9f / 10);
				buffLevel = Math.Max(buffLevel - cooldown, 0);
				if (buffLevel <= threshhold && lastBuffLevel > threshhold)
					onDeactivate(player);

				float postDmg = damageMultiplier;
				player.attackDamageModifier *= postDmg / preDmg;

				Console.WriteLine(buffLevel);
			}

			if (modifier != null)
				modifier.strength = damageMultiplier;
		}
	}
}
