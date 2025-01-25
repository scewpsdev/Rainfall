using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlacksteelGlaive : Weapon
{
	public BlacksteelGlaive()
		: base("blacksteel_glaive")
	{
		displayName = "Blacksteel Glaive";

		baseDamage = 2.4f;
		baseAttackRange = 1.3f;
		baseAttackRate = 1.2f;
		twoHanded = true;
		baseWeight = 2.5f;
		doubleBladed = false;
		attackCooldown = 1.5f;
		attackAcceleration = 1;

		value = 35;

		sprite = new Sprite(tileset, 11, 9, 2, 1);
		icon = new Sprite(tileset.texture, 11 * 16 + 16, 9 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		if (idx % 3 == 0)
		{
			anim = AttackAnim.SwingOverhead;
			swingDir = 0;
			startAngle = MathF.PI * 1.5f;
			endAngle = MathF.PI * -0.25f;
		}
		else if (idx % 3 == 1)
		{
			anim = AttackAnim.SwingSideways;
			swingDir = 0;
			startAngle = MathF.PI * 0.75f;
			endAngle = MathF.PI * -1.0f;
		}
		else
		{
			anim = AttackAnim.Stab;
		}
	}
}
