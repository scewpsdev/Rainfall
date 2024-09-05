using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Effects
{
	public static unsafe ParticleEffect CreateBloodEffect(Vector2 direction)
	{
		ParticleEffect effect = new ParticleEffect(null, "res/effects/blood.rfs");
		effect.system.handle->startVelocity = new Vector3(direction, 0);
		effect.collision = true;
		return effect;
	}
}
