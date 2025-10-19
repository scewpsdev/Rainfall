using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public class PalePotion : Potion
{
	const float COOLDOWN_TIME = 20;

	bool boiling;
	long startTime = -1;

	public PalePotion(bool boiling = false)
		: base("pale_potion", "Pale Potion", PotionEffectType.Water)
	{
		this.boiling = boiling;

		/*
		addEffect(new WaterEffect(boiling));

		displayName = "Bottle of Water";
		stackable = true;
		value = 3;
		canDrop = false;

		sprite = new Sprite(tileset, 4, 5);
		*/
	}

	public PalePotion()
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
