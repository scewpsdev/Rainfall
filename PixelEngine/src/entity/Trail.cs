using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Trail
{
	Vector2[] trail;
	Vector4 color;


	public Trail(int numPoints, Vector4 color, Vector2 position)
	{
		trail = new Vector2[numPoints];
		this.color = color;

		Array.Fill(trail, position);
	}

	public void update()
	{
		for (int i = trail.Length - 1; i >= 1; i--)
			trail[i] = trail[i - 1];
	}

	public void setPosition(Vector2 position)
	{
		trail[0] = position;
	}

	public void render()
	{
		for (int i = 0; i < trail.Length - 1; i++)
		{
			float alpha = 1 - i / (float)(trail.Length - 1);
			alpha = alpha * alpha;
			Renderer.DrawLine(new Vector3(trail[i], 0), new Vector3(trail[i + 1], 0), color * new Vector4(1, 1, 1, alpha));
		}
	}
}
