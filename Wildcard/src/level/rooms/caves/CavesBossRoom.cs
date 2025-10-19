using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CavesBossRoom : BossRoom
{
	public CavesBossRoom()
	{
		boss = new GolemBoss();
		//boss.direction = -1;
		//AdvancedAI ai = boss.ai as AdvancedAI;
		//ai.walkDirection = -1;

		track = new MultilayerTrack("sounds/ost/battle/ost", 2);
	}

	public override void init(Level level)
	{
		base.init(level);

		setup(level, new Vector2i(5, 3), new Vector2i(26, 3), new Vector2i(13, 8), new Vector2i(44, 0), new Vector2i(46, 0));
		setActivateTrigger(new Vector2(14, 1), new Vector2(8, 5));
	}
}
