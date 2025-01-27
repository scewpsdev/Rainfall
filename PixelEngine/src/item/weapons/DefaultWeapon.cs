using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DefaultWeapon : Weapon
{
	public static readonly DefaultWeapon instance = new DefaultWeapon();


	public DefaultWeapon()
		: base("default_weapon")
	{
		baseDamage = 0.8f;
		baseAttackRange = 0.7f;
		baseAttackRate = 2;
		knockback = 5;
		anim = AttackAnim.Stab;
		actionMovementSpeed = 0.8f;

		canParry = true;
		parrySound = [Resource.GetSound("sounds/punch_hit.ogg")];
		parryWindow = 0.2f;
		blockCharge = 0;

		strengthScaling = 0.5f;
		dexterityScaling = 0.5f;

		//swingSprite = new Sprite(tileset, 1, 8);
		sprite = new Sprite(tileset, 0, 2);

		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/weapon/default.png", false), 0, 0, 32, 32);

		hitSound = [Resource.GetSound("sounds/punch_hit.ogg")];
		stepSound = Resource.GetSounds("sounds/step_bare", 3);
		landSound = Resource.GetSounds("sounds/land_bare", 3);
	}
}
