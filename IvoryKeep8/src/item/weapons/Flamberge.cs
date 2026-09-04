using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Flamberge : Weapon
{
	public Flamberge()
		: base("flamberge")
	{
		displayName = "Flamberge";

		//baseDamage = 1.5f;
		baseDamage = 0.5f;
		baseAttackRange = 1.7f;
		baseAttackRate = 0.9f;
		//criticalChanceModifier = 2.0f;
		bleed = 1;
		twoHanded = true;
		baseWeight = 2;
		canBlock = true;

		attackStartAngle = 0.75f * MathF.PI;
		attackEndAngle = -0.25f * MathF.PI;
		anim = AttackAnim.SwingOverhead;

		strengthScaling = 0.5f;

		baseValue = 29;

		sprite = new Sprite(tileset, 12, 6, 2, 1);
		icon = new Sprite(tileset.texture, 12 * 16, 6 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;
		//backRotation = -0.25f * MathF.PI;
		weaponTipMargin = 3 / 16.0f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		anim = idx % 2 == 0 ? AttackAnim.SwingOverhead : AttackAnim.SwingSideways;
	}
}
