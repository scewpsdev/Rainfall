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

		setRenderWeapon(mainHand, shield);

		//duration = shield.blockDuration;
		if (shield.canParry)
		{
			duration = shield.blockCharge + shield.parryWindow;
			postActionLinger = 0.2f;
		}
		else
		{
			duration = 1000;
		}
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

		if (!shield.canParry)
		{
			bool input = player.currentAttackInput != null && player.currentAttackInput.isDown();
			direction = player.lookDirection.normalized;
			if (!input || player.actions.actionQueue.Count > 1)
				cancel();
		}

		speedMultiplier = elapsedTime >= shield.blockCharge && player.isGrounded ? shield.actionMovementSpeed : 1;
	}

	public float progress
	{
		get => shield.canParry ? 1 : MathF.Min(elapsedTime / shield.blockCharge, 1);
	}

	public bool isParrying
	{
		get => elapsedTime > shield.blockCharge && elapsedTime < shield.blockCharge + shield.parryWindow;
	}

	public bool isBlocking
	{
		get => elapsedTime >= shield.blockCharge + shield.parryWindow && elapsedTime < duration;
	}

	public override Matrix getItemTransform(Player player, bool mainHand)
	{
		float rotation = shield.parryWeaponRotation;
		if (shield.canParry)
		{
			float parryRotation = shield.parryWeaponRotation;
			rotation = isParrying ? parryRotation : elapsedTime < shield.blockCharge ? elapsedTime / shield.blockCharge * parryRotation : (1 - (elapsedTime - shield.blockCharge - shield.parryWindow) / postActionLinger) * parryRotation;
		}
		Matrix shieldTransform = Matrix.CreateTranslation(progress * 0.5f, 0, 0)
			* Matrix.CreateTranslation(player.getWeaponOrigin(mainHand).x, player.getWeaponOrigin(mainHand).y, 0)
			* (shield.type == ItemType.Weapon ? Matrix.CreateTranslation(-0.2f, 0.3f, 0) * Matrix.CreateRotation(Vector3.UnitZ, rotation) : Matrix.Identity)
			;
		bool flip = direction.x < 0;
		if (flip)
			shieldTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * shieldTransform;
		return shieldTransform;
	}
}
