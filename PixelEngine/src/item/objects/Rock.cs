using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rock : Item
{
	public Rock()
		: base("rock", ItemType.Weapon)
	{
		displayName = "Rock";

		projectileItem = true;
		maxPierces = -1;

		attackDamage = 1.5f; //4;

		value = 1;
		upgradable = false;

		sprite = new Sprite(tileset, 4, 0);

		hitSound = Resource.GetSounds("res/sounds/hit_rock", 5);
	}

	public override bool use(Player player)
	{
		player.throwItem(this, player.lookDirection.normalized);
		return true;
	}

	public override void upgrade()
	{
		base.upgrade();
		attackDamage++;
	}
}
