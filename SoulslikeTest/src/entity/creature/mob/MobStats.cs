using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MobStats
{
	const float STAMINA_REGEN_RATE = 5;
	const float POISE_REGEN_RATE = 0.5f;


	public float maxHealth = 8;
	public float health = 8;

	public float maxStamina = 24;
	public float stamina = 24;

	public float maxMana = 8;
	public float mana = 8;

	public float maxPoise = 10;
	public float poise = 10;

	public bool isDead { get => health == 0; }


	public void damage(float dmg)
	{
		health = MathHelper.Clamp(health - dmg, 0, maxHealth);
	}

	public void poiseDamage(float dmg)
	{
		poise = MathHelper.Clamp(poise - dmg, 0, maxPoise);
	}

	public void consumeStamina(float amount)
	{
		stamina = MathHelper.Clamp(stamina - amount, 0, maxStamina);
	}

	public void consumeMana(float amount)
	{
		mana = MathHelper.Clamp(mana - amount, 0, maxMana);
	}

	public void update(bool sprinting, bool grounded, Action currentAction)
	{
		if (stamina < maxStamina && !sprinting && grounded && (currentAction == null || currentAction.staminaCost == 0))
		{
			stamina = MathF.Min(stamina + STAMINA_REGEN_RATE * Time.deltaTime, maxStamina);
		}
		if (poise < maxPoise)
		{
			poise = MathF.Min(poise + POISE_REGEN_RATE * Time.deltaTime, maxPoise);
		}
	}
}
