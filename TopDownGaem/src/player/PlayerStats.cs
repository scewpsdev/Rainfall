using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class StatEffect
{
	public abstract bool update(PlayerStats stats);
}

public class RegenerationEffect : StatEffect
{
	ConsumableEffectStat stat;
	float amount;
	float duration;

	long startTime;
	float amountRegenerated;

	public RegenerationEffect(ConsumableEffectStat stat, float amount, float duration)
	{
		this.stat = stat;
		this.amount = amount;
		this.duration = duration;

		startTime = Time.currentTime;
	}

	public override bool update(PlayerStats stats)
	{
		float elapsedTime = (Time.currentTime - startTime) / 1e9f;
		float progress = duration > 0 ? MathF.Min(elapsedTime / duration, 1.0f) : 1;
		float shouldAmount = progress * amount;
		float delta = shouldAmount - amountRegenerated;

		if (stat == ConsumableEffectStat.Health)
		{
			stats.damage(-delta);
		}
		else if (stat == ConsumableEffectStat.Mana)
		{
			stats.consumeMana(-delta);
		}

		amountRegenerated += delta;

		return elapsedTime < duration;
	}
}

public struct PlayerLevels
{
	public int vitality = 1;
	public int endurance = 1;
	public int agility = 50;
	public int strength = 1;
	public int finesse = 1;
	public int intelligence = 1;

	public PlayerLevels(int _)
	{
	}
}

public class PlayerStats
{
	const float STAMINA_REGEN_RATE = 6;
	const int STAMINA_DRAIN_MIN_VALUE = -5;

	const float POISE_REGEN_RATE = 1;
	const float MANA_REGEN_RATE = 0.5f;


	Player player;

	public float health;
	public float stamina;
	public float mana;

	public float maxPoise = 10;
	public float poise = 10;

	public int xp { get; private set; }
	public int level = 1;
	public int availablePoints = 0;

	public int kills = 0;

	public PlayerLevels levels = new PlayerLevels(0);

	public bool isDead { get => health == 0; }

	public List<StatEffect> effects = new List<StatEffect>();

	int lastLevelXP = 0;


	public PlayerStats(Player player)
	{
		this.player = player;

		health = getMaxHealth();
		stamina = getMaxStamina();
		mana = getMaxMana();

		//for (int i = 0; i < 10; i++)
		//{
		//	Console.WriteLine("Level " + i + ": " + getLevelRequiredXP(i));
		//}
	}

	public void damage(float dmg)
	{
		health = MathHelper.Clamp(health - dmg, 0, getMaxHealth());
	}

	public void poiseDamage(float dmg)
	{
		poise = MathHelper.Clamp(poise - dmg, 0, maxPoise);
	}

	public void consumeStamina(float amount)
	{
		stamina = MathHelper.Clamp(stamina - amount, STAMINA_DRAIN_MIN_VALUE, getMaxStamina());
	}

	public void consumeMana(float amount)
	{
		mana = MathHelper.Clamp(mana - amount, 0, getMaxMana());
	}

	public void addEffect(StatEffect effect)
	{
		effects.Add(effect);
	}

	public void awardXP(int count)
	{
		xp += count;
		int nextLevelRequired = getLevelRequiredXP(level + 1);
		if (xp - lastLevelXP > nextLevelRequired)
		{
			level++;
			availablePoints += 3;
			//player.onLevelUp();
			lastLevelXP = nextLevelRequired;
		}
	}

	int getLevelRequiredXP(int level)
	{
		float f = (MathF.Exp((level - 1) * 0.2f) - 1) * 80;
		return (int)f;
	}

	public void update(bool sprinting, bool grounded, Action currentAction)
	{
		if (!sprinting && grounded && (currentAction == null || currentAction.staminaCost == 0))
			stamina = Math.Min(stamina + STAMINA_REGEN_RATE * Time.deltaTime, getMaxStamina());
		if (poise < maxPoise)
			poise = MathF.Min(poise + POISE_REGEN_RATE * Time.deltaTime, maxPoise);
		if (mana < getMaxMana())
			mana = Math.Min(mana + MANA_REGEN_RATE * Time.deltaTime, getMaxMana());

		for (int i = 0; i < effects.Count; i++)
		{
			if (!effects[i].update(this))
				effects.RemoveAt(i--);
		}
	}

	public float getMaxHealth()
	{
		return 12 + levels.vitality * 1;
	}

	public float getMaxStamina()
	{
		return 15.0f + levels.endurance * 0.5f;
	}

	public float getMaxMana()
	{
		return 5 + (levels.intelligence - 1) * 1.3f;
	}

	public float getMovementSpeed()
	{
		return 2.2f + levels.agility * 0.025f;
	}
}
