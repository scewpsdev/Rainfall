using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Longsword : Weapon
{
	public Longsword()
		: base("longsword")
	{
		displayName = "Longsword";

		baseDamage = 1.8f;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.2f;

		canBlock = true;
		parryWeaponRotation = -0.3f * MathF.PI;

		strengthScaling = 0.4f;
		dexterityScaling = 0.2f;

		baseValue = 14;

		sprite = new Sprite(tileset, 1, 1);
		//icon = new Sprite(tileset.texture, 12, 10 * 16, 16, 16);
		renderOffset.x = 0.25f;
		//renderOffset.x = 0.25f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		anim = idx % 2 == 0 ? AttackAnim.SwingOverhead : AttackAnim.SwingSideways;
	}
}
