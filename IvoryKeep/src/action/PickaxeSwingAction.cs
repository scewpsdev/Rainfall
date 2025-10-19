using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PickaxeSwingAction : AttackAction
{
	Vector2 startPosition;
	bool hit = false;
	bool up, down;


	public PickaxeSwingAction(Item weapon, bool mainHand, Player player)
		: base(weapon, mainHand, player)
	{
	}

	public override void onQueued(Player player)
	{
		base.onQueued(player);

		startPosition = player.center;
		up = InputManager.IsDown("Up");
		down = InputManager.IsDown("Down");
	}

	public override void update(Player player)
	{
		base.update(player);

		if (elapsedTime / duration > 0.1f && !hit)
		{
			if (hitEntities.Count == 0)
			{
				if (up)
				{
					Vector2i pos = (Vector2i)Vector2.Floor(startPosition + Vector2.Up);
					TileType tile = GameState.instance.level.getTile(pos);
					if (tile != null && tile.isSolid && tile.health > 0 && tile.health <= weapon.upgradeLevel + 1)
					{
						SpellEffects.BreakBlock(pos);
						//player.actions.cancelAction();
						hit = true;
					}
				}
				else if (down)
				{
					Vector2i pos = (Vector2i)Vector2.Floor(startPosition + Vector2.Down);
					TileType tile = GameState.instance.level.getTile(pos);
					if (tile != null && tile.isSolid && tile.health > 0 && tile.health <= weapon.upgradeLevel + 1)
					{
						SpellEffects.BreakBlock(pos);
						//player.actions.cancelAction();
						hit = true;
					}
				}
				else
				{
					Vector2i pos = (Vector2i)Vector2.Floor(startPosition + getWorldDirection(currentProgress).normalized);
					TileType tile = GameState.instance.level.getTile(pos);
					if (tile != null && tile.isSolid)
					{
						SpellEffects.BreakBlock(pos);
						//player.actions.cancelAction();
						hit = true;
					}
				}
			}
		}
	}
}
