using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Emerald : Gem
{
	public Emerald()
		: base("emerald")
	{
		displayName = "Emerald";

		sprite = new Sprite(tileset, 1, 3);
	}
}
