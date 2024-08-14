using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Cloak : Item
{
	public Cloak()
		: base("cloak")
	{
		type = ItemType.Passive;

		armor = 3;

		sprite = new Sprite(tileset, 5, 0);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/cloak.png", false), 0, 0, 16, 16);
	}

	public override Item createNew()
	{
		return new Cloak();
	}
}
