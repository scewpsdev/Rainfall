using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Entity
{
	public static SpriteSheet sprites = new SpriteSheet(Resource.GetTexture("sprites/entities.png", false), 8, 8);


	public int x, y;
	public int width = 1, height = 1;
	public bool remove = false;

	public float displayX, displayY;

	public int direction = 1;

	public Sprite sprite;
	public FloatRect rect = new FloatRect(0, 0.2f, 1, 1);
	public Vector4 color = Vector4.One;
	public bool additive = false;

	public float turn = 0;
	public float speed = 1;

	public int health = 20;
	public int maxHealth = 20;
	public int damage = 10;


	public virtual void init()
	{
		displayX = x;
		displayY = y;
	}

	public virtual void destroy()
	{
	}

	public virtual void interact(Player player)
	{
	}

	public virtual void update()
	{
	}

	public virtual void render()
	{
		displayX = MathHelper.Linear(displayX, x, 15 * Time.deltaTime);
		displayY = MathHelper.Linear(displayY, y, 15 * Time.deltaTime);
	}
}
