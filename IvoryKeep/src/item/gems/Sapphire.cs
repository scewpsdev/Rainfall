using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Sapphire : Gem
{
	public Sapphire()
		: base("sapphire")
	{
		displayName = "Sapphire";

		sprite = new Sprite(tileset, 0, 3);
	}
}
