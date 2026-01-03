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
		baseAttackRange = 1.1f;
		baseAttackRate = 1.0f;
		anim = AttackAnim.SwingOverhead;
		attackAcceleration = 1;
		baseWeight = 2.5f;
		doubleBladed = false;

		strengthScaling = 0.5f;
		dexterityScaling = 0.1f;

		baseValue = 29;

		sprite = new Sprite(tileset, 8, 7, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;
		weaponTipMargin = 5 / 16.0f;
		backRotation = -0.5f * MathF.PI;
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		anim = idx % 2 == 0 ? AttackAnim.SwingOverhead : AttackAnim.SwingSideways;
	}
}
