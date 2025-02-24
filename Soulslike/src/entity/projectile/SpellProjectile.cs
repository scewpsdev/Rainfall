using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpellProjectile : Projectile
{
	public SpellProjectile(Vector3 offset)
		: base("entity/projectile/spell_projectile/projectile.rfs", offset)
	{
		speed = 8.0f;
		spins = true;
	}
}
