using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Light : Entity
{
	Vector3 color;
	float radius;


	public Light(int x, int y, float brightness)
	{
		position = new Vector2(x, y);
		color = new Vector3(1.0f, 0.9f, 0.7f) * brightness;
		radius = brightness;
	}

	public override void draw()
	{
		Renderer.DrawLight(position, color, radius);
	}
}
