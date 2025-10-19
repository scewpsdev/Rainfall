using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Sapphire : Item
{
	public Sapphire()
		: base("sapphire", ItemType.Gem)
	{
		displayName = "Sapphire";
		stackable = true;

		baseValue = 40;

		sprite = new Sprite(tileset, 0, 3);
	}
}
