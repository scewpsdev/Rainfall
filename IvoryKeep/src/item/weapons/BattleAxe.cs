using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BattleAxe : Weapon
{
	public BattleAxe()
		: base("battle_axe")
	{
		displayName = "Battle Axe";

		baseDamage = 1.8f;
		baseAttackRange = 1.0f;
		baseAttackRate = 1.0f;
		anim = AttackAnim.SwingOverhead;
		attackAcceleration = 1;
		baseWeight = 2.5f;
		doubleBladed = false;

		strengthScaling = 0.5f;
		dexterityScaling = 0.1f;

		value = 29;

		sprite = new Sprite(tileset, 8, 7, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;
	}
}
