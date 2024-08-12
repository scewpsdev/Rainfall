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
		: base("default_weapon")
	{
		attackDamage = 1;
		attackRange = 0.8f;
		attackRate = 5;

		sprite = new Sprite(tileset, 4, 1);
	}

	public override Item createNew()
	{
		return new DefaultWeapon();
	}
}
