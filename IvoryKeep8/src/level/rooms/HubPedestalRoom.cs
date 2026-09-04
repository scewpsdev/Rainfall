using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HubPedestalRoom : Entity
{
	Room room;


	public HubPedestalRoom(Room room)
	{
		this.room = room;
	}

	public override void init(Level level)
	{
		SaveFile save = GameState.instance.save;

		level.lightLevel = 1;

		for (int i = 0; i < save.highscores.Length; i++)
		{
			Vector2 position = room.getMarker(15) + new Vector2(i * 5, 0);
			level.addEntity(new Pedestal(), position);

			if (save.highscores[i].score > 0)
			{
				string[] label =
					i == 0 ? ["Fastest Time:", save.highscores[i].time != -1 ? StringUtils.TimeToString(save.highscores[i].time) : "???"] :
					i == 1 ? ["Highest Score:", save.highscores[i].score.ToString()] :
					i == 2 ? ["Highest Floor:", save.highscores[i].floor != -1 ? (save.highscores[i].floor + 1).ToString() : "???"] :
					i == 3 ? ["Most kills:", save.highscores[i].kills.ToString()] : ["???"];
				uint color = RunStats.recordColors[i];
				level.addEntity(new HighscoreDummy(save.highscores[i], label, color), position + Vector2.Up);
			}
		}

		level.addEntity(new LeaderboardEntity(false), level.getMarker(0x3));

		//level.addEntity(new IronDoor(save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) ? null : "dummy_key"), new Vector2(38.5f, 23));
		if (save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) && !save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED))
		{
			RatNPC rat = new RatNPC(); // NPCManager.rat;
			rat.clearShop();
			rat.direction = 1;
			level.addEntity(rat, level.getMarker(0xb));

			level.addEntity(new RopeEntity(13), level.getMarker(0xc));
		}
	}
}
