using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Entity
{
	public Vector2 position;
	public Vector2 size;
	public Level level;
	public bool removed = false;

	public FloatRect collider = null;
	public FloatRect hitbox = null;
	public bool colliderEnabled = true;
	public bool staticCollider = false;


	public virtual void reset()
	{
	}

	public virtual void destroy()
	{
	}

	public virtual void update()
	{
	}

	public virtual void draw()
	{
	}
}
