using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Cliffside : Entity
{
	Room room;
	Texture waves;


	public Cliffside(Room room)
	{
		this.room = room;

		waves = Resource.GetTexture("level/cliffside/waves.png", (uint)SamplerFlags.Point | (uint)SamplerFlags.VClamp);
	}

	public override void init(Level level)
	{
		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
		{
			/*
			level.addEntity(new ExplosiveBarrel() { health = 1000 }, (Vector2)room.getMarker(40) + new Vector2(0.5f, 0.0f));
			level.addEntity(new ExplosiveBarrel() { health = 1000 }, (Vector2)room.getMarker(40) + new Vector2(-0.5f, 0.0f));
			for (int i = 0; i < 3; i++)
				level.setTile(room.getMarker(40).x + 1, room.getMarker(40).y + i, TileType.dirt);
			*/

			level.addEntity(new EventTrigger(new Vector2(1, 2), (Player player) =>
			{
				player.clearInventory();
			}, null), new Vector2(room.width - 1, 38));
		}

		level.addEntity(level.exit = new LevelTransition(GameState.instance.hub, GameState.instance.hub.entrance, new Vector2i(1, 2), Vector2i.Right), new Vector2(room.width, 38));
	}
}
