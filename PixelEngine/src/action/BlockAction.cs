using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;


public class BlockAction : EntityAction
{
	public Item shield;
	public int direction;

	public List<Entity> hitEntities = new List<Entity>();


	public BlockAction(Item shield, bool mainHand)
		: base("block", mainHand)
	{
		this.shield = shield;

		duration = shield.blockDuration;
		speedMultiplier = 0.2f;
	}

	public override void onStarted(Player player)
	{
		direction = player.direction;
		player.blockingItem = shield;
	}

	public override void onFinished(Player player)
	{
		player.blockingItem = null;
	}

	public float progress
	{
		get => MathF.Min(elapsedTime / duration * 5, 1);
	}

	public bool isBlocking
	{
		get => progress >= 1;
	}

	public override Matrix getItemTransform(Player player)
	{
		return Matrix.CreateTranslation(progress * 0.5f * player.direction, 0, 0)
			* Matrix.CreateTranslation(player.getWeaponOrigin(mainHand).x * player.direction, player.getWeaponOrigin(mainHand).y, 0)
			* (shield.type == ItemType.Weapon ? Matrix.CreateRotation(Vector3.UnitZ, MathF.PI * 0.5f) * Matrix.CreateTranslation(shield.renderOffset.x, 0, 0) : Matrix.Identity)
			;
	}
}
