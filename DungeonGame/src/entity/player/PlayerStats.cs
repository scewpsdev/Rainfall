﻿using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static Player;


public class StatEffect
{
	protected float duration;

	protected float elapsed = 0.0f;


	protected StatEffect(float duration)
	{
		this.duration = duration;
	}

	public virtual void update(PlayerStats stats)
	{
		elapsed += Time.deltaTime;
	}

	public bool finished
	{
		get => elapsed >= duration;
	}
}

internal class HealEffect : StatEffect
{
	int amount;

	public HealEffect(int amount, float duration)
		: base(duration)
	{
		this.amount = amount;
	}

	public override void update(PlayerStats stats)
	{
		base.update(stats);

		if (!finished)
		{
			float amountPerFrame = amount / duration * Time.deltaTime;
			stats.heal(amountPerFrame);
		}
	}
}

internal class ManaRechargeEffect : StatEffect
{
	int amount;

	public ManaRechargeEffect(int amount, float duration)
		: base(duration)
	{
		this.amount = amount;
	}

	public override void update(PlayerStats stats)
	{
		base.update(stats);

		if (!finished)
		{
			float amountPerFrame = amount / duration * Time.deltaTime;
			stats.rechargeMana(amountPerFrame);
		}
	}
}

public class PlayerStats
{
	const float STAMINA_REGEN_RATE = 8.0f;
	const float STAMINA_EXHAUST_PENALTY = 3.0f;
	const float STAMINA_DRAIN_MIN_VALUE = -5.0f;
	const float SHIELD_BLOCK_STAMINA_REGEN_PENALTY = 0.2f;

	const float SPRINT_STAMINA_COST = 2.5f;


	public int maxHealth;
	public int health;
	float bufferedHealth;

	public float maxStamina;
	public float stamina;
	float staminaPenaltyTimer;

	public int maxMana;
	public int mana;

	public int xp;

	Player player;

	List<StatEffect> effects = new List<StatEffect>();


	public PlayerStats(Player player)
	{
		this.player = player;
	}

	public void reset()
	{
		maxHealth = 80;
		health = 80;
		bufferedHealth = 0.0f;

		maxStamina = 12.0f;
		stamina = 12.0f;
		staminaPenaltyTimer = -1.0f;

		maxMana = 80;
		mana = 80;

		xp = 0;
	}

	public void addEffect(StatEffect effect)
	{
		effects.Add(effect);
	}

	public void applyDamage(float amount)
	{
		//int currentHealth = health;
		heal(-amount);
		if (health == 0)
		{
			//if (player.currentAction != null)
			//	player.cancelAction();
			player.cancelAllActions();
			player.queueAction(new DeathAction());
		}
	}

	public void heal(float amount)
	{
		int amounti = (int)(amount + 0.01f);
		bufferedHealth += amount - amounti + 0.01f;
		int bufferedHealthi = (int)(bufferedHealth);
		health = Math.Clamp(health + amounti + bufferedHealthi, 0, maxHealth);
		bufferedHealth -= bufferedHealthi;
	}

	public void consumeStamina(float amount)
	{
		stamina = Math.Max(stamina - amount, STAMINA_DRAIN_MIN_VALUE);
		//if (stamina <= 0.0f)
		//	player.audioAction.playSoundOrganic(player.sfxExhaust, 0.5f, 1.0f, 0.2f, 0.1f);
	}

	public void consumeMana(int amount)
	{
		mana = Math.Max(mana - amount, 0);
	}

	public void rechargeMana(float amount)
	{
		int amounti = (int)(amount + 0.01f);
		mana = Math.Clamp(mana + amounti, 0, maxMana);
	}

	public void awardXP(int amount)
	{
		xp += amount;
	}

	public void update()
	{
		if (player.walkMode == WalkMode.Sprint && player.isGrounded && staminaPenaltyTimer == -1.0f)
		{
			stamina -= SPRINT_STAMINA_COST * Time.deltaTime;
			if (stamina <= 0.0f)
			{
				stamina = 0.0f;
				staminaPenaltyTimer = 0.0f;
			}
		}
		else //if (player.isGrounded)
		{
			if (player.currentAction == null || player.currentAction.staminaCost == 0.0f)
			{
				// Regen stamina
				if (stamina < maxStamina)
				{
					if (player.isBlocking)
						stamina = Math.Min(stamina + STAMINA_REGEN_RATE * SHIELD_BLOCK_STAMINA_REGEN_PENALTY * Time.deltaTime, maxStamina);
					else
						stamina = Math.Min(stamina + STAMINA_REGEN_RATE * Time.deltaTime, maxStamina);
				}
			}
		}

		if (staminaPenaltyTimer != -1.0f)
		{
			staminaPenaltyTimer += Time.deltaTime;
			if (staminaPenaltyTimer >= STAMINA_EXHAUST_PENALTY)
				staminaPenaltyTimer = -1.0f;
		}

		for (int i = 0; i < effects.Count; i++)
		{
			effects[i].update(this);
			if (effects[i].finished)
			{
				effects.RemoveAt(i);
				i--;
			}
		}
	}

	public bool canSprint
	{
		get => stamina > 0.0f && staminaPenaltyTimer == -1.0f;
	}

	public bool canJump
	{
		get => stamina >= 0.0f;
	}

	public bool canDoAction
	{
		get => stamina >= 0.0f;
	}
}
