using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class BlockStanceAction : Action
{
	const int WEAPON_PARRY_FRAMES = 4;


	public readonly int handID;
	public readonly Item item;

	bool resumeBlock;


	public BlockStanceAction(Item item, int handID, bool resumeBlock)
		: base("block_stance")
	{
		this.handID = handID;
		this.item = item;
		this.resumeBlock = resumeBlock;

		//animationName[0] = "block_stance";
		//animationSet[0] = item.moveset;

		if (item.twoHanded)
		{
			animationName[1] = "block_stance";
			animationName[2] = "block_stance";
			animationSet[1] = item.moveset;
			animationSet[2] = item.moveset;
		}
		else
		{
			animationName[1 + handID] = "block_stance";
			animationSet[1 + handID] = item.moveset;
		}

		Debug.Assert(item.category == ItemCategory.Weapon);
		if (resumeBlock)
			animationTransitionDuration = 0.0f;
		else
			animationTransitionDuration = 0.2f;

		mirrorAnimation = handID == 1;
		duration = 1000.0f;
	}

	public override void onStarted(Player player)
	{
		base.onStarted(player);

		player.blockingItem = item;
		player.blockingHand = handID;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (item.weaponType == WeaponType.Melee)
		{
			if (elapsedTime < WEAPON_PARRY_FRAMES / 24.0f && !resumeBlock)
			{
				player.parryingItem = item;
				player.parryingHand = handID;
			}
			else
			{
				player.parryingItem = null;
				player.parryingHand = -1;
			}
		}
	}

	public override void onFinished(Player player)
	{
		base.onFinished(player);

		player.blockingItem = null;
		player.blockingHand = -1;

		player.parryingItem = null;
		player.parryingHand = -1;
	}
}
