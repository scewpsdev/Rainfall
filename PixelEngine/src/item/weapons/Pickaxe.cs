using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Pickaxe : Weapon
{
	public Pickaxe()
		: base("pickaxe")
	{
		displayName = "Pickaxe";

		baseDamage = 2;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.5f;
		anim = AttackAnim.SwingOverhead;

		value = 13;

		sprite = new Sprite(tileset, 0, 1);
		renderOffset.x = 0.2f;
	}
}
