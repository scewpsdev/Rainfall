using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Item
{
	public static SpriteSheet tileset = new SpriteSheet(Resource.GetTexture("res/sprites/items.png", false), 16, 16);


	public int id;

	public string name;
	public string displayName = "???";

	public int attackDamage = 0;
	public float attackRange = 1;

	public Sprite sprite = null;
	public Sprite ingameSprite = null;

	// modifiers


	public Item(string name)
	{
		id = (int)Hash.hash(name);
		this.name = name;
	}

	public virtual void use(Player player)
	{
	}
}
