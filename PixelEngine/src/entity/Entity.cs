using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Entity
{
	public const uint FILTER_DEFAULT = 1 << 0;
	public const uint FILTER_PLAYER = 1 << 1;
	public const uint FILTER_MOB = 1 << 2;
	public const uint FILTER_ITEM = 1 << 3;

	public const float LAYER_DEFAULT = 0.0f;
	public const float LAYER_DEFAULT_OVERLAY = -0.01f;
	public const float LAYER_BG = 0.1f;
	public const float LAYER_FG = -0.1f;


	public Vector2 position;
	public float rotation;
	public Vector2 velocity;

	public bool removed { get; private set; } = false;
	public List<Action> removeCallbacks = new List<Action>();

	public FloatRect collider;
	public uint filterGroup = FILTER_DEFAULT;


	public virtual void init()
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
	}

	public void remove()
	{
		removed = true;
	}

	public Matrix getTransform()
	{
		return Matrix.CreateTransform(new Vector3(position, 0), Quaternion.FromAxisAngle(Vector3.UnitZ, rotation));
	}
}
