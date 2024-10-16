using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DefaultWeapon : Item
{
	public static readonly DefaultWeapon instance = new DefaultWeapon();


	public DefaultWeapon()
		: base("default_weapon", ItemType.Weapon)
	{
		baseDamage = 1;
		baseAttackRange = 1.0f;
		baseAttackRate = 4;
		knockback = 5;

		sprite = new Sprite(tileset, 0, 2);

		hitSound = [Resource.GetSound("res/sounds/punch_hit.ogg")];
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, true));
		return false;
	}
}
