using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MinesBossRoom : BossRoom
{
	public MinesBossRoom(Room room)
		: base(room, 1)
	//: base(room, new Garran() { itemDrops = [new LostSigil()] })
	{
		bosses.Add(new Raya() { itemDrops = [new QuestlineLoganStaff()] });

		track = new MultilayerTrack("sounds/ost/nighthaven/nighthaven", 3);
		trackHasIdleLayer = true;
	}

	public override void init(Level level)
	{
		base.init(level);

		setActivateTrigger(new Vector2(14, 1), new Vector2(8, 5));
	}
}
