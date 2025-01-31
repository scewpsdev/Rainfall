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

		baseDamage = 1;
		projectileItem = true;
		projectileAims = true;
		projectileSticks = true;
		//maxPierces = 1;
		breakOnEnemyHit = true;
		stackable = true;

		value = 1;

		sprite = new Sprite(tileset, 2, 0);
		renderOffset.x = 0.2f;
		collider = new FloatRect(-1.0f / 16, -1.0f / 16, 2.0f / 16, 2.0f / 16);

		hitSound = [Resource.GetSound("sounds/arrow_hit.ogg")];
	}

	public override bool use(Player player)
	{
		Item arrow = player.removeItemSingle(this);
		player.throwItem(arrow, player.lookDirection.normalized);
		arrow.baseDamage = 0.5f;
		arrow.knockback = 2.0f;
		return false;
	}
}
