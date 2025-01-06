using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DefaultWeapon : Weapon
{
	public static readonly DefaultWeapon instance = new DefaultWeapon();


	Sprite punchSprite, swingSprite;

	public DefaultWeapon()
		: base("default_weapon")
	{
		baseDamage = 0.8f;
		baseAttackRange = 0.8f;
		baseAttackRate = 3;
		knockback = 5;
		attackAngle = 1.0f * MathF.PI;
		attackAngleOffset = -0.9f * MathF.PI;

		punchSprite = new Sprite(tileset, 0, 2);
		swingSprite = new Sprite(tileset, 1, 8);

		//ingameSprite = new Sprite(Resource.GetTexture("sprites/items/weapon/default.png", false), 0, 0, 16, 16);

		hitSound = [Resource.GetSound("sounds/punch_hit.ogg")];
	}

	protected override void getAttackAnim(int idx, out bool stab, out int swingDir, out float startAngle, out float endAngle)
	{
		base.getAttackAnim(idx, out stab, out swingDir, out startAngle, out endAngle);
		stab = idx % 2 == 0;
		swingDir = 1;
		sprite = stab ? punchSprite : swingSprite;
	}
}
