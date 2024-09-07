using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BowShootAction : EntityAction
{
	Item weapon;

	public List<Entity> hitEntities = new List<Entity>();


	public BowShootAction(Item weapon, bool mainHand)
		: base("bow_shoot", mainHand)
	{
		duration = 1.0f / weapon.attackRate;

		this.weapon = weapon;
	}

	public override void onStarted(Player player)
	{
		Vector2 direction = player.lookDirection.normalized;
		Vector2 position = player.position + player.collider.center + direction * 0.25f;
		Vector2 velocity = direction * weapon.attackRange;

		Arrow arrow = new Arrow();
		//arrow.breakOnHit = Random.Shared.Next() % 5 > 0;
		arrow.breakOnHit = true;
		//arrow.maxRicochets = 1;
		arrow.attackDamage = weapon.attackDamage;
		arrow.knockback = weapon.knockback;
		ItemEntity entity = new ItemEntity(arrow, player, velocity);
		entity.collider = new FloatRect(-1.0f / 16, -1.0f / 16, 2.0f / 16, 2.0f / 16);
		entity.bounciness = 0.3f;
		GameState.instance.level.addEntity(entity, position);
	}

	public override Matrix getItemTransform(Player player)
	{
		Vector2 direction = player.lookDirection.normalized;
		float rotation = direction.angle;
		//bool flip = direction.x < 0;
		Matrix weaponTransform = Matrix.CreateTranslation(0, player.getWeaponOrigin(mainHand).y, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, rotation)
			* Matrix.CreateTranslation(0.25f, 0, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, MathF.PI * 0.5f);
		//if (flip)
		//	weaponTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * weaponTransform;
		return weaponTransform;
	}
}
