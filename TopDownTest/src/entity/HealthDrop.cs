using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


public class HealthDrop : Pickup
{
	public HealthDrop(Vector2 position)
		: base(position)
	{
		sprite = new Sprite(itemSprites, 0, 0);
		autoPickup = true;
	}

	protected override bool onPickup(Player player)
	{
		if (player.health < player.maxHealth)
		{
			player.health++;
			return true;
		}
		return false;
	}
}
