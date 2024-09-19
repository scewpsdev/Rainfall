using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public class WaterEffect : PotionEffect
{
	public bool boiling;

	public WaterEffect(bool boiling = false)
		: base("Water", 1, new Sprite(Item.tileset, 4, 5))
	{
		this.boiling = boiling;
	}

	public override void apply(Entity entity, Potion potion)
	{
		if (boiling)
		{
			if (entity is Hittable)
			{
				Hittable hittable = entity as Hittable;
				hittable.hit(Random.Shared.NextSingle() * 2, null, potion, "Boiling Water");
			}
			if (entity is Player)
			{
				Player player = entity as Player;
				player.hud.showMessage("The water is scalding hot.");
			}
		}
		else
		{
			if (entity is Player)
			{
				Player player = entity as Player;
				player.hud.showMessage("It tastes bland.");
			}
		}
	}
}

public class BottleOfWater : Potion
{
	const float COOLDOWN_TIME = 20;

	bool boiling;
	long startTime = -1;

	public BottleOfWater(bool boiling)
		: base("bottle_of_water")
	{
		this.boiling = boiling;

		addEffect(new WaterEffect(boiling));

		displayName = "Bottle of Water";
		stackable = true;
		value = 3;
		canDrop = false;

		sprite = new Sprite(tileset, 4, 5);
	}

	public BottleOfWater()
		: this(false)
	{
	}

	public override void update(Entity entity)
	{
		if (startTime == -1)
			startTime = Time.currentTime;

		if (boiling && (Time.currentTime - startTime) / 1e9f > COOLDOWN_TIME)
		{
			boiling = false;
			displayName = "Distilled water";
		}
	}
}
