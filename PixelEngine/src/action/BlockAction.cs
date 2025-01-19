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

		if (mainHand) renderMainWeapon = true;
		else renderSecondaryWeapon = true;

		//duration = shield.blockDuration;
		duration = shield.type == ItemType.Shield ? 1000 : shield.blockDuration;
		speedMultiplier = shield.blockMovementSpeed;
	}

	public override void onQueued(Player player)
	{
		direction = player.lookDirection.normalized;
	}

	public override void onStarted(Player player)
	{
		player.blockingItem = shield;
	}

	public override void onFinished(Player player)
	{
		player.blockingItem = null;
	}

	public override void update(Player player)
	{
		base.update(player);

		bool input = InputManager.IsDown(mainHand ? "Attack" : "Attack2") || Input.IsKeyDown(KeyCode.Ctrl) && (Input.IsKeyDown(KeyCode.Left) || Input.IsKeyDown(KeyCode.Right) || Input.IsKeyDown(KeyCode.Up) || Input.IsKeyDown(KeyCode.Down));
		if (shield.type == ItemType.Shield)
		{
			direction = player.lookDirection.normalized;
			if (!input || player.actions.actionQueue.Count > 1)
				cancel();
		}
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
