using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Skull : Item
{
	public Skull()
		: base("skull")
	{
		sprite = new Sprite(tileset, 0, 0);
	}

	public override void use(Player player)
	{
		player.throwItem(this);
	}
}
