using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlockAction : EntityAction
{
	public Item shield;
	Vector2 direction;

	public List<Entity> hitEntities = new List<Entity>();


	public BlockAction(Item shield, bool mainHand)
		: base("block", mainHand)
	{
		this.shield = shield;

		//duration = shield.blockDuration;
		duration = shield.type == ItemType.Shield ? 1000 : shield.blockDuration;
		speedMultiplier = shield.blockMovementSpeed;
	}

	public override void update(Player player)
	{
		base.update(player);

		bool input = InputManager.IsDown(mainHand ? "Attack" : "Attack2");
		if (shield.type == ItemType.Shield)
		{
			direction = player.lookDirection.normalized;
			if (!input || player.actions.actionQueue.Count > 1)
				cancel();
		}
	}

	public override void onStarted(Player player)
	{
		direction = player.lookDirection.normalized;
		player.blockingItem = shield;
	}

	public override void onFinished(Player player)
	{
		player.blockingItem = null;
	}

	public float progress
	{
		get => MathF.Min(elapsedTime / shield.blockCharge, 1);
	}

	public bool isBlocking
	{
		get => progress >= 1;
	}

	public override Matrix getItemTransform(Player player)
	{
		Matrix shieldTransform = Matrix.CreateTranslation(progress * 0.5f, 0, 0)
			* Matrix.CreateTranslation(player.getWeaponOrigin(mainHand).x, player.getWeaponOrigin(mainHand).y, 0)
			* (shield.type == ItemType.Weapon ? Matrix.CreateRotation(Vector3.UnitZ, MathF.PI * 0.5f) * Matrix.CreateTranslation(shield.renderOffset.x, 0, 0) : Matrix.Identity)
			;
		bool flip = direction.x < 0;
		if (flip)
			shieldTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * shieldTransform;
		return shieldTransform;
	}
}
