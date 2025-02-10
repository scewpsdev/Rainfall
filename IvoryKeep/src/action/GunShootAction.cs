using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GunShootAction : EntityAction
{
	Item weapon;

	public List<Entity> hitEntities = new List<Entity>();


	public GunShootAction(Item weapon, bool mainHand)
		: base("revolver_shoot", mainHand)
	{
		duration = 1000;

		this.weapon = weapon;

		renderWeaponMain = weapon;
	}

	public override void onQueued(Player player)
	{
		duration = 1.0f / weapon.attackRate / player.getAttackSpeedModifier();

		Vector2 direction = Vector2.Zero;
		if (InputManager.IsDown("Up"))
			direction.y++;
		if (InputManager.IsDown("Down"))
			direction.y--;
		//if (InputManager.IsDown("Left"))
		//	direction.x--;
		//if (InputManager.IsDown("Right"))
		//	direction.x++;
		if (direction == Vector2.Zero)
			direction.x += player.direction;
		direction = direction.normalized;

		direction = player.lookDirection.normalized;

		float rotation = new Vector2(MathF.Abs(direction.x), direction.y).angle;
		Vector2 position = new Vector2(0.25f + 0.5f * weapon.size.x, 0);
		position = Vector2.Rotate(position, rotation);
		position += new Vector2(0, player.getWeaponOrigin(mainHand).y);
		if (direction.x < 0)
			position.x *= -1;
		position += player.position;

		Vector2 offset = Vector2.Zero; // new Vector2(player.direction * 0.6f, 0);

		GameState.instance.level.addEntity(new Bullet(direction, player.velocity, offset, player, weapon), position);

		GameState.instance.level.addEntity(new BulletFireEffect(player), position + offset);
	}

	public override Matrix getItemTransform(Player player, bool mainHand)
	{
		Vector2 direction = player.lookDirection.normalized;
		float rotation = new Vector2(MathF.Abs(direction.x), direction.y).angle;
		bool flip = direction.x < 0;
		Matrix weaponTransform = Matrix.CreateTranslation(0, player.getWeaponOrigin(mainHand).y, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, rotation)
			* Matrix.CreateTranslation(0.25f, 0, 0);
		if (flip)
			weaponTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * weaponTransform;
		return weaponTransform;
	}
}
