using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IntroBridge : Entity
{
	Vector2i[] explosionPoints = [new Vector2i(101, 27), new Vector2i(106, 24), new Vector2i(106, 17), new Vector2i(94, 27)];
	bool[] explosionTriggered = [false, false, false, false];
	long explosionTriggerTime = -1;
	int numExplosionsTriggered = 0;


	public override void init(Level level)
	{
		LevelTransition transition = new LevelTransition(GameState.instance.cliffside, null, new Vector2i(45, 2), Vector2i.Down) { destinationPosition = GameState.instance.cliffside.rooms[0].getMarker(0x22) };
		level.addEntity(transition, new Vector2(level.width - transition.size.x, 0));

		transition.onTrigger = () =>
		{
			GameState.instance.player.actions.queueAction(new UnconciousAction());
		};


		EventTrigger explosionTrigger = new EventTrigger(new Vector2(12, 8), (Player player) =>
		{
			explosionTriggerTime = Time.currentTime;
		}, null);
		level.addEntity(explosionTrigger, new Vector2(87, 24));
	}

	public override void update()
	{
		for (int i = 0; i < explosionPoints.Length; i++)
		{
			float offset = Hash.hash(explosionPoints[i]) / uint.MaxValue;

			if (explosionTriggerTime != -1 && (Time.currentTime - explosionTriggerTime) / 1e9f > offset && !explosionTriggered[i])
			{
				SpellEffects.Explode(explosionPoints[i] + 0.5f, 4, 0, null, null);
				explosionTriggered[i] = true;
				numExplosionsTriggered++;
			}
		}

		if (numExplosionsTriggered == explosionPoints.Length)
		{
			for (int y = 0; y < 33; y++)
			{
				for (int x = level.width - 35; x < level.width; x++)
				{
					level.setTile(x, y, null);
				}
			}
			level.updateLightmap(level.width - 35, 0, 35, 33);
			numExplosionsTriggered = 0;
		}
	}
}
