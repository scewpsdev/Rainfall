using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Staff : Item
{
	long lastUse = -1;


	public Staff()
		: base("staff")
	{
		displayName = "Staff";

		sprite = new Sprite(tileset, 8, 1);

		chargeTime = 1.0f / 2;
	}

	public override Item createNew()
	{
		return new Staff();
	}

	void shoot(Player player)
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

		Vector2 position = player.position + new Vector2(player.direction * 0.5f, 0.4f);
		Vector2 offset = new Vector2(0, 0.2f);

		GameState.instance.level.addEntity(new MagicProjectile(direction, player.velocity, offset, player), position);

		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);
	}

	public override bool use(Player player)
	{
		if ((Time.currentTime - lastUse) / 1e9f > chargeTime)
		{
			lastUse = Time.currentTime;
			shoot(player);
		}
		return true;
	}

	public override bool useSecondary(Player player)
	{
		shoot(player);
		return true;
	}
}
