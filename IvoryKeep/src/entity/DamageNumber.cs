﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DamageNumber : Entity
{
	string numberStr;
	bool critical;

	float lifetime = 1.2f;
	long startTime;


	public DamageNumber(int number, Vector2 velocity, bool critical)
	{
		numberStr = number.ToString();
		this.velocity = velocity;
		this.critical = critical;
	}

	public override void init(Level level)
	{
		startTime = Time.currentTime;
	}

	public override void update()
	{
		velocity.y += -10 * Time.deltaTime;
		Vector2 displacement = velocity * Time.deltaTime;
		position += displacement;

		if ((Time.currentTime - startTime) / 1e9f >= lifetime)
			remove();
	}

	public override void render()
	{
		float progress = (Time.currentTime - startTime) / 1e9f / lifetime;
		uint color = MathHelper.ColorAlpha(critical ? 0xFFFF0000 : 0xFFAAAAAA, 1 - progress);
		Renderer.DrawWorldTextBMP(position.x - Renderer.MeasureWorldTextBMP(numberStr).x / 2 / 16, position.y - Renderer.MeasureWorldTextBMP(numberStr).y / 2 / 16, 0, numberStr, 1.0f / 16, color);
	}
}
