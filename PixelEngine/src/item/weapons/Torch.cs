using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Item
{
	public Torch()
		: base("torch", ItemType.Weapon)
	{
		displayName = "Torch";

		attackDamage = 1;
		attackRange = 1.0f;
		attackRate = 3.0f;
		stab = false;

		value = 2;

		canDrop = false;
		isSecondaryItem = true;

		sprite = new Sprite(tileset, 8, 0);
		particleEffect = "res/effects/torch.rfs";
		particlesOffset = new Vector2(0.25f, 0.25f);
		renderOffset.x = 0.3f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}

	public override void render(Entity entity)
	{
		Renderer.DrawLight(entity.position, new Vector3(1.0f, 0.9f, 0.7f) * 2, 9);
	}
}
