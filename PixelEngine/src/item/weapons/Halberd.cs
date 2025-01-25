using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Halberd : Weapon
{
	public Halberd()
		: base("halberd")
	{
		displayName = "Halberd";

		baseDamage = 1.6f;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.6f;
		twoHanded = true;
		baseWeight = 2.5f;
		doubleBladed = false;

		value = 16;

		sprite = new Sprite(tileset, 7, 4, 2, 1);
		icon = new Sprite(tileset.texture, 7 * 16 + 12, 4 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		anim = idx % 2 == 0 ? AttackAnim.Stab : AttackAnim.SwingSideways;
	}
}
