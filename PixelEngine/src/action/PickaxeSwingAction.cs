using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PickaxeSwingAction : AttackAction
{
	Vector2 hitPosition;
	bool hit = false;


	public PickaxeSwingAction(Item weapon)
		: base(weapon)
	{
	}

	public override void onStarted(Player player)
	{
		base.onStarted(player);

		hitPosition = player.position;
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
					Vector2i pos = (Vector2i)Vector2.Floor(hitPosition + new Vector2(0, 1.5f));
					TileType tile = GameState.instance.level.getTile(pos);
					if (tile != null && tile.isSolid)
					{
						GameState.instance.level.setTile(pos.x, pos.y, null);
						//player.actions.cancelAction();
					}
				}
				else
				{
					Vector2i pos = (Vector2i)Vector2.Floor(hitPosition + new Vector2(base.direction, 0.5f));
					TileType tile = GameState.instance.level.getTile(pos);
					if (tile != null && tile.isSolid)
					{
						GameState.instance.level.setTile(pos.x, pos.y, null);
						//player.actions.cancelAction();
					}
					else
					{
						pos = (Vector2i)Vector2.Floor(hitPosition + new Vector2(base.direction, -0.5f));
						tile = GameState.instance.level.getTile(pos);
						if (tile != null && tile.isSolid)
						{
							GameState.instance.level.setTile(pos.x, pos.y, null);
							//player.actions.cancelAction();
						}
					}
				}
			}

			hit = true;
		}
	}
}
