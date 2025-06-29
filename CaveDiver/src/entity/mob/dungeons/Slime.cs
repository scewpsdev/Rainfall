﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

public class Slime : Mob
{
	int size;

	long spawnTime;


	public Slime(int size = 3)
		: base("slime")
	{
		this.size = size;

		displayName = "Slime";

		if (size == 4)
		{
			sprite = new Sprite(Resource.GetTexture("sprites/mob/slime4.png", false), 0, 0, 16, 16);
			collider = new FloatRect(-0.3f, 0, 0.6f, 0.6f);
		}
		else if (size == 3)
		{
			sprite = new Sprite(Resource.GetTexture("sprites/mob/slime3.png", false), 0, 0, 16, 16);
			collider = new FloatRect(-0.25f, 0, 0.5f, 0.6f);
		}
		else if (size == 2)
		{
			sprite = new Sprite(Resource.GetTexture("sprites/mob/slime2.png", false), 0, 0, 16, 16);
			collider = new FloatRect(-0.25f, 0, 0.5f, 0.6f);
			itemDropChance = 0;
			dropCoins = false;
			displayName = "Small Slime";
		}
		else if (size == 1)
		{
			sprite = new Sprite(Resource.GetTexture("sprites/mob/slime1.png", false), 0, 0, 16, 16);
			collider = new FloatRect(-0.25f, 0, 0.5f, 0.6f);
			itemDropChance = 0;
			dropCoins = false;
			displayName = "Tiny Slime";
		}

		spriteColor = 0xFFce584e;

		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 0.333f, true);
		//animator.addAnimation("dead", 1 * 16, 0, 16, 0, 1, 1, true);
		animator.setAnimation("idle");

		health = (MathHelper.IPow(2, size) + 1) / 2;
		jumpPower = size * 4; //  MathHelper.IPow(2, size) * 3 / 2;
		speed = 0.5f;

		ai = new SlimeAI(this);
	}

	public Slime()
		: this(3)
	{
	}

	public override void init(Level level)
	{
		base.init(level);

		spawnTime = Time.currentTime;
	}

	public override void onDeath(Entity by)
	{
		if (size > 2)
		{
			int subSlimes = 4; // MathHelper.RandomInt(1, 4);
			for (int i = 0; i < subSlimes; i++)
			{
				Slime slime = new Slime(size - 2);
				slime.spriteColor = spriteColor;
				GameState.instance.level.addEntity(slime, position + new Vector2(MathHelper.RandomFloat(collider.min.x - slime.collider.min.x, collider.max.x - slime.collider.max.x), MathHelper.RandomFloat(collider.min.y - slime.collider.min.y, collider.max.y - slime.collider.max.y)));
			}
		}

		base.onDeath(by);
	}

	/*
	public override void onDeath(Entity by)
	{
		base.onDeath(by);

		if (size > 2)
		{
			int subSlimes = MathHelper.RandomInt(1, 4);
			for (int i = 0; i < subSlimes; i++)
			{
				Slime slime = new Slime(size - 2);
				slime.spriteColor = spriteColor;
				GameState.instance.level.addEntity(slime, position + new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 0.1f));
			}
		}
	}
	*/
}
