using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CrossbowShootAction : EntityAction
{
	Item weapon;
	Item arrow;

	public List<Entity> hitEntities = new List<Entity>();


	public CrossbowShootAction(Item weapon, Item arrow, bool mainHand)
		: base("gun_shoot", mainHand)
	{
		duration = 1000;

		this.weapon = weapon;
		this.arrow = arrow;

		setRenderWeapon(mainHand, weapon);
	}

	public override void onQueued(Player player)
	{
		duration = 1.0f / weapon.attackRate / player.getAttackSpeedModifier();

		/*
		Vector2 direction = player.lookDirection.normalized;
		Vector2 position = player.position + player.collider.center + direction * 0.25f;
		Vector2 velocity = direction * weapon.attackRange;

		Arrow arrow = new Arrow();
		//arrow.breakOnHit = Random.Shared.Next() % 5 > 0;
		arrow.breakOnWallHit = true;
		//arrow.maxRicochets = 1;
		arrow.attackDamage = weapon.attackDamage;
		arrow.knockback = weapon.knockback;
		ItemEntity entity = new ItemEntity(arrow, player, velocity);
		entity.collider = new FloatRect(-1.0f / 16, -1.0f / 16, 2.0f / 16, 2.0f / 16);
		entity.bounciness = 0.3f;
		GameState.instance.level.addEntity(entity, position);
		*/
	}

	public override void onStarted(Player player)
	{
		Vector2 direction = player.lookDirection.normalized;
		Vector2 inaccuracy = MathHelper.RandomPointOnCircle(Random.Shared) * 0.02f;
		direction = (direction + inaccuracy / (weapon.accuracy * player.getAccuracyModifier())).normalized;

		Vector2 position = player.position + player.collider.center;
		Vector2 offset = new Vector2(player.direction * 0.25f, 0.1f);

		ArrowProjectile projectile = new ArrowProjectile(direction, offset, player, weapon, arrow);
		GameState.instance.level.addEntity(projectile, position);
	}

	public override Matrix getItemTransform(Player player, bool mainHand)
	{
		Vector2 direction = player.lookDirection.normalized;
		float rotation = direction.angle;
		//bool flip = direction.x < 0;
		Matrix weaponTransform = Matrix.CreateTranslation(0, player.getWeaponOrigin(mainHand).y, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, rotation)
			* Matrix.CreateTranslation(0.25f, 0, 0);
		//if (flip)
		//	weaponTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * weaponTransform;
		return weaponTransform;
	}
}
