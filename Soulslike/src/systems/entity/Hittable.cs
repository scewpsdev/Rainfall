using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface Hittable
{
	void hit(int damage, bool criticalHit, Vector3 hitDirection, Entity by, Item item, RigidBody hitbox);
}
