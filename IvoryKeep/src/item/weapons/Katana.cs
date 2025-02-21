using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Katana : Weapon
{
	public Katana()
		: base("katana")
	{
		displayName = "Katana";

		baseDamage = 1.2f;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.5f;
		attackCooldown = 2.5f;
		baseWeight = 2.5f;

		canParry = true;
		parryWeaponRotation = -0.3f * MathF.PI;
		blockCharge = 0;

		dexterityScaling = 0.9f;

		value = 14;

		sprite = new Sprite(tileset, 8, 10, 2, 1);
		icon = new Sprite(tileset, 8.25f, 10);
		size = new Vector2(2, 1);
		renderOffset.x = 0.3f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		anim = idx % 2 == 0 ? AttackAnim.SwingSideways : AttackAnim.Stab;
	}
}
