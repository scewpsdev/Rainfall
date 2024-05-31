using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class AI
{
	protected const int TICK_RATE = 5;

	protected static List<AI> aiList = new List<AI>();

	public static void Update()
	{
		void updateAI(int i)
		{
			if ((Time.currentTime - aiList[i].lastUpdate) / 1e9f > 1.0f / TICK_RATE)
			{
				aiList[i].update(1.0f / TICK_RATE);
				aiList[i].lastUpdate = Time.currentTime;
			}
		}
		Parallel.For(0, aiList.Count, updateAI);
	}


	protected Mob mob;

	public long lastUpdate;

	public AI(Mob mob)
	{
		this.mob = mob;
	}

	public abstract void onHit(Entity from);
	public abstract void onSound(Vector3 position);
	public abstract void update(float delta);
}
