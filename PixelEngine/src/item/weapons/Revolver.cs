using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Revolver : Item
{
	public Revolver()
		: base("revolver", ItemType.Weapon)
	{
		displayName = "Revolver";

		attackRate = 10.0f;
		//trigger = false;

		attackDamage = 3;

		value = 1000;
		canDrop = false;

		sprite = new Sprite(tileset, 14, 0);
		renderOffset.x = 0.3f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new RevolverShootAction(this, player.handItem == this));
		return false;
	}

	public override void render(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			if (player.actions.currentAction != null && player.actions.currentAction is RevolverShootAction && player.actions.currentAction.elapsedTime < 0.1f)
				Renderer.DrawLight(player.position + new Vector2(0, 0.5f), Vector3.One * 3, 6.0f);
		}
	}
}
