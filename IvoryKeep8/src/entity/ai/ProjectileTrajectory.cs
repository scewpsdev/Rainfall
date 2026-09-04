using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ProjectileTrajectory
{
	public static Vector2 Calculate(float dx, float dy, float xspeed, float gravity)
	{
		float xtime = MathF.Abs(dx) / xspeed;
		float xt0 = (0.5f * -gravity * MathF.Pow(xtime, 2) + dy) / (-gravity * xtime);
		float yvelocity = -gravity * xt0;
		return new Vector2(MathF.Sign(dx) * xspeed, yvelocity);
	}
}
