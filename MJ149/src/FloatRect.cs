using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FloatRect
{
	public Vector2 position;
	public Vector2 size;


	public FloatRect(Vector2 position, Vector2 size)
	{
		this.position = position;
		this.size = size;
	}

	public FloatRect(float x, float y, float width, float height)
	{
		position = new Vector2(x, y);
		size = new Vector2(width, height);
	}

	public Vector2 min
	{
		get => position;
	}

	public Vector2 max
	{
		get => position + size;
	}
}
