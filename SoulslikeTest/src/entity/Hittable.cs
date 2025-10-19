using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


interface Hittable
{
	bool hit(float damage, float poiseDamage, Entity from, Item item, Vector3 hitPosition, Vector3 hitDirection, RigidBody body);
}
