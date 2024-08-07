using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Pickaxe : Item
{
	public Pickaxe()
		: base("pickaxe")
	{
		displayName = "Pickaxe";

		attackDamage = 2;
		attackRange = 1.5f;
		attackRate = 3.0f;
		stab = false;

		sprite = new Sprite(tileset, 0, 1);
	}

	public override Item createNew()
	{
		return new Pickaxe();
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));

		HitData hit = GameState.instance.level.raycast(player.position + new Vector2(0, 0.5f), new Vector2(player.direction, 0), attackRange, Entity.FILTER_MOB);
		if (hit != null)
		{
			if (hit.entity != null)
			{
			}
			else
			{
				if (Input.IsKeyDown(KeyCode.Up))
				{
					Vector2i pos = (Vector2i)Vector2.Floor(player.position + new Vector2(0, 1.5f));
					TileType tile = TileType.Get(GameState.instance.level.getTile(pos));
					if (tile != null && tile.isSolid)
					{
						GameState.instance.level.setTile(pos.x, pos.y, 0);
						//player.actions.cancelAction();
					}
				}
				else
				{
					Vector2i pos = (Vector2i)Vector2.Floor(player.position + new Vector2(player.direction, 0.5f));
					TileType tile = TileType.Get(GameState.instance.level.getTile(pos));
					if (tile != null && tile.isSolid)
					{
						GameState.instance.level.setTile(pos.x, pos.y, 0);
						//player.actions.cancelAction();
					}
					else
					{
						pos = (Vector2i)Vector2.Floor(player.position + new Vector2(player.direction, -0.5f));
						tile = TileType.Get(GameState.instance.level.getTile(pos));
						if (tile != null && tile.isSolid)
						{
							GameState.instance.level.setTile(pos.x, pos.y, 0);
							//player.actions.cancelAction();
						}
					}
				}
			}
		}

		return true;
	}
}
