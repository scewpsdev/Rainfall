using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BowShootAction : EntityAction
{
	Item weapon;
	Item arrow;

	Sound shootSound;

	public List<Entity> hitEntities = new List<Entity>();


	public BowShootAction(Item weapon, Item arrow, bool mainHand)
		: base("bow_shoot", mainHand)
	{
		duration = 1000;

		this.weapon = weapon;
		this.arrow = arrow;

		setRenderWeapon(mainHand, weapon);

		shootSound = Resource.GetSound("sounds/bow_shoot.ogg");
	}

	public override void onFinished(Player player)
	{
		Vector2 direction = (player.lookDirection.normalized * 1.1f + new Vector2(MathF.Sign(player.velocity.x), 0)).normalized;
		Vector2 inaccuracy = MathHelper.RandomPointOnCircle(Random.Shared) * 0.03f;
		direction = (direction + inaccuracy / (weapon.accuracy * player.getAccuracyModifier())).normalized;

		Vector2 position = player.position + Vector2.Up * 0.75f;
		Vector2 offset = new Vector2(player.direction * 0.25f, 0);

		ArrowProjectile projectile = new ArrowProjectile(direction, offset, player, weapon, arrow);
		GameState.instance.level.addEntity(projectile, position);

		Audio.PlayOrganic(shootSound, new Vector3(player.position, 0), 1, 1, 0.0f, 0.15f);
	}

	public override void update(Player player)
	{
		base.update(player);

		InputBinding binding = player.currentAttackInput;
		if (binding == null)
			duration = 1.0f / weapon.attackRate / player.getAttackSpeedModifier();
	}

	public override Matrix getItemTransform(Player player, bool mainHand)
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
