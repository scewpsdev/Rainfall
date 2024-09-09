using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Arrow : Item
{
	public Arrow()
		: base("arrow", ItemType.Ammo)
	{
		displayName = "Arrow";

		attackDamage = 1;
		projectileItem = true;
		//maxPierces = 1;
		breakOnEnemyHit = true;
		stackable = true;

		value = 1;

		sprite = new Sprite(tileset, 2, 0);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		Item arrow = player.removeItemSingle(this);
		player.throwItem(arrow, player.lookDirection.normalized);
		return false;
	}
}
