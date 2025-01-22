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
		anim = AttackAnim.Stab;
		attackAngle = 1.0f * MathF.PI;
		attackAngleOffset = -0.9f * MathF.PI;
		actionMovementSpeed = 0.8f;

		punchSprite = new Sprite(tileset, 0, 2);
		swingSprite = new Sprite(tileset, 1, 8);
		sprite = punchSprite;

		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/weapon/default.png", false), 0, 0, 32, 32);

		hitSound = [Resource.GetSound("sounds/punch_hit.ogg")];
		stepSound = Resource.GetSounds("sounds/step_bare", 3);
		landSound = Resource.GetSounds("sounds/land_bare", 3);
	}
}
