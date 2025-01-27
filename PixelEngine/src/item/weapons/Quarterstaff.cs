using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Quarterstaff : Weapon
{
	public Quarterstaff()
		: base("quarterstaff")
	{
		displayName = "Quarterstaff";

		baseDamage = 1.0f;
		baseAttackRange = 1.2f;
		baseAttackRate = 2.5f;
		attackStartAngle = 1.25f * MathF.PI;
		attackEndAngle = -0.75f * MathF.PI;
		attackCooldown = 0.5f;
		twoHanded = true;
		secondaryChargeTime = 0.3f;
		baseWeight = 1;
		//stab = false;
		//attackAngle = MathF.PI * 0.7f;

		dexterityScaling = 0.5f;
		canBlock = true;

		value = 9;

		sprite = new Sprite(tileset, 4, 1, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;

		hitSound = woodHit;
		blockSound = woodHit;
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		anim = idx % 2 == 0 ? AttackAnim.SwingSideways : idx % 2 == 1 ? AttackAnim.Stab : AttackAnim.SwingOverhead;
	}
}
