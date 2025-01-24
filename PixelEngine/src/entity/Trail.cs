using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Trail
{
	public Vector2[] points;
	Vector4 color;


	public Trail(int numPoints, Vector4 color, Vector2 position)
	{
		points = new Vector2[numPoints];
		this.color = color;

		Array.Fill(points, position);
	}

	public void update()
	{
		for (int i = points.Length - 1; i >= 1; i--)
			points[i] = points[i - 1];
	}

	public void setPosition(Vector2 position)
	{
		points[0] = position;
	}

	public void render()
	{
		for (int i = 0; i < points.Length - 1; i++)
		{
			float alpha = 1 - i / (float)(points.Length - 1);
			alpha = alpha * alpha;
			Renderer.DrawLine(new Vector3(points[i], 0), new Vector3(points[i + 1], 0), color * new Vector4(1, 1, 1, alpha));
		}
	}
}
