using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Entity
{
	public Vector3 position;
	public float rotation;
	public Level level;

	public bool removed { get; private set; } = false;
	public List<Action> removeCallbacks = new List<Action>();

	public Sprite sprite;
	public FloatRect rect;


	public virtual void init(Level level)
	{
	}

	public virtual void destroy()
	{
	}

	public virtual void update()
	{
	}

	public virtual void render()
	{
		if (sprite != null && rect != null)
		{
			Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, position.z, rect.size.x, rect.size.y, 0, sprite);
		}
	}

	public virtual void onLevelSwitch(Level newLevel)
	{
	}

	public void remove()
	{
		removed = true;
	}

	public Matrix getTransform()
	{
		return Matrix.CreateTransform(position, Quaternion.FromAxisAngle(Vector3.UnitZ, rotation));
	}
}
