﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PickaxeSwingAction : AttackAction
{
	Vector2 startPosition;
	bool hit = false;


	public PickaxeSwingAction(Item weapon, bool mainHand, Player player)
		: base(weapon, mainHand, player)
	{
	}

	public override void onQueued(Player player)
	{
		base.onQueued(player);

		startPosition = player.position;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (elapsedTime / duration > 0.5f && !hit)
		{
			if (hitEntities.Count == 0)
			{
				if (InputManager.IsDown("Up"))
				{
					Vector2i pos = (Vector2i)Vector2.Floor(startPosition + new Vector2(0, 1.5f));
					TileType tile = GameState.instance.level.getTile(pos);
					if (tile != null && tile.isSolid && tile.health > 0 && tile.health <= weapon.upgradeLevel + 1)
					{
						GameState.instance.level.setTile(pos.x, pos.y, null);
						GameState.instance.level.updateLightmap(pos.x, pos.y, 1, 1);
						//player.actions.cancelAction();
					}
				}
				else
				{
					Vector2i pos = (Vector2i)Vector2.Floor(startPosition + direction + new Vector2(0, 0.5f));
					TileType tile = GameState.instance.level.getTile(pos);
					if (tile != null && tile.isSolid && tile.health > 0 && tile.health <= weapon.upgradeLevel + 1)
					{
						GameState.instance.level.setTile(pos.x, pos.y, null);
						GameState.instance.level.updateLightmap(pos.x, pos.y, 1, 1);
						//player.actions.cancelAction();
					}
					else
					{
						pos = (Vector2i)Vector2.Floor(startPosition + direction + new Vector2(0, -0.5f));
						tile = GameState.instance.level.getTile(pos);
						if (tile != null && tile.isSolid && tile.health > 0 && tile.health <= weapon.upgradeLevel + 1)
						{
							GameState.instance.level.setTile(pos.x, pos.y, null);
							GameState.instance.level.updateLightmap(pos.x, pos.y, 1, 1);
							//player.actions.cancelAction();
						}
					}
				}
			}

			hit = true;
		}
	}
}
