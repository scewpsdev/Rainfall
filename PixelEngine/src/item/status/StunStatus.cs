using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StunStatus : StatusEffect
{
	float duration;

	long startTime;

	public StunStatus(float duration)
		: base("stun", new Sprite(tileset, 0, 2))
	{
		this.duration = duration;
	}

	public override void init(Entity entity)
	{
		startTime = Time.currentTime;
	}

	public override bool update(Entity entity)
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		return elapsed < duration;
	}
}
