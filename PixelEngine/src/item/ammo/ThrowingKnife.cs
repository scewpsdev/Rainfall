using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ThrowingKnife : Item
{
	float speed = 32;

	public ThrowingKnife()
		: base("throwing_knife", ItemType.Ammo)
	{
		displayName = "Throwing Knife";

		baseDamage = 2;
		projectileItem = true;
		projectileRotationOffset = -0.25f * MathF.PI;
		projectileSticks = true;
		projectileAims = true;
		breakOnEnemyHit = true;
		stackable = true;
		isHandItem = false;
		isActiveItem = true;

		value = 3;

		sprite = new Sprite(tileset, 4, 4);
		collider = new FloatRect(-1.0f / 16, -1.0f / 16, 2.0f / 16, 2.0f / 16);
	}

	public override bool use(Player player)
	{
		Item knife = player.removeItemSingle(this);
		ItemEntity entity = player.throwItem(knife, player.lookDirection.normalized, speed);
		entity.gravity = -1;
		return true;
	}
}
