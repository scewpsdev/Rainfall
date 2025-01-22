using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Anvil : NPC
{
	public Anvil()
		: base("anvil")
	{
		displayName = "Anvil";

		sprite = new Sprite(tileset, 1, 2);

		canUpgrade = true;
		turnTowardsPlayer = false;
	}
}
