using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RevolverShootAction : EntityAction
{
	Item weapon;

	public List<Entity> hitEntities = new List<Entity>();


	public RevolverShootAction(Item weapon, bool mainHand)
		: base("revolver_shoot", mainHand)
	{
		duration = 1000;

		this.weapon = weapon;
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

		direction = (player.lookDirection.normalized * 1.1f + new Vector2(MathF.Sign(player.velocity.x), 0)).normalized;

		Vector2 position = player.position + new Vector2(0.0f, 0.3f);
		Vector2 offset = new Vector2(player.direction * 0.6f, 0);

		GameState.instance.level.addEntity(new Bullet(direction, player.velocity, offset, player, weapon), position);

		GameState.instance.level.addEntity(new BulletFireEffect(player), position + offset);
	}
}
