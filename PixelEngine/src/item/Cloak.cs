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

		armor = 1;

		sprite = new Sprite(tileset, 5, 0);
	}

	public override Item createNew()
	{
		return new Cloak();
	}
}
