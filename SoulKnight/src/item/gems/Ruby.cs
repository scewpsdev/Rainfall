using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Ruby : Item
{
	public Ruby()
		: base("ruby", ItemType.Gem)
	{
		displayName = "Ruby";
		stackable = true;

		value = 30;

		sprite = new Sprite(tileset, 2, 3);
	}
}
