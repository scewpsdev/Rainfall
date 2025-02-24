using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CrossbowBolt : Projectile
{
	public CrossbowBolt(Vector3 offset)
		: base("entity/projectile/bolt/bolt.rfs", offset)
	{
		speed = 15.0f;
		spins = true;
		gravity = -4;
	}
}
