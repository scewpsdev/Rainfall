using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Ruby : Gem
{
	public Ruby()
		: base("ruby")
	{
		displayName = "Ruby";

		baseValue = 70;

		sprite = new Sprite(tileset, 2, 3);
	}
}
