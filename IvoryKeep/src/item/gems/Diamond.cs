using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Diamond : Gem
{
	public Diamond()
		: base("diamond")
	{
		displayName = "Diamond";

		sprite = new Sprite(tileset, 3, 0);
	}
}
