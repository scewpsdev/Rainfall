﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightObject : Entity
{
	Vector3 color;
	PointLight light;


	public LightObject(Vector3 color)
	{
		this.color = color;
	}

	public LightObject(float r, float g, float b)
	{
		this.color = new Vector3(r, g, b);
	}

	public override void init()
	{
		light = new PointLight(position, color, Renderer.graphics);
	}

	public override void destroy()
	{
		light.destroy(Renderer.graphics);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawPointLight(light);
		//Renderer.DrawLight(position, color);
	}
}
