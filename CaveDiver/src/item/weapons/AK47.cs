using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AK47 : Weapon
{
	public AK47()
		: base("ak47", WeaponType.Ranged)
	{
		displayName = "AK-47";

		baseAttackRate = 10;
		trigger = false;
		twoHanded = true;

		baseDamage = 1.5f;

		value = 1000;
		canDrop = false;

		sprite = new Sprite(tileset, 12, 8);
		renderOffset.x = 0.3f;

		useSound = Resource.GetSounds("sounds/shoot", 2);
	}

	public override bool use(Player player)
	{
		base.use(player);
		player.actions.queueAction(new GunShootAction(this, player.handItem == this));
		return false;
	}

	public override void render(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			if (player.actions.currentAction != null && player.actions.currentAction is GunShootAction && player.actions.currentAction.elapsedTime < 0.1f)
				Renderer.DrawLight(player.position + new Vector2(0, 0.5f), Vector3.One * 3, 6.0f);
		}
	}
}
