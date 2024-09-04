using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpellCastAction : EntityAction
{
	Item weapon;

	public List<Entity> hitEntities = new List<Entity>();


	public SpellCastAction(Item weapon, bool mainHand)
		: base("spell_cast", mainHand)
	{
		duration = 1.0f / weapon.attackRate;

		this.weapon = weapon;
	}

	public override void onStarted(Player player)
	{
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

		Vector2 position = player.position + new Vector2(0.0f, 0.3f);
		Vector2 offset = new Vector2(player.direction * 0.5f, 0.3f);

		GameState.instance.level.addEntity(new MagicProjectile(direction, player.velocity, offset, player, weapon), position);

		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);
	}

	public override void update(Player player)
	{
		base.update(player);
	}
}
