using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ExplosionEffect : ParticleEffect
{
	long startTime;

	Vector3 color = new Vector3(1, 0.7f, 0.4f) * 40;


	public ExplosionEffect()
		: base(null, "effects/explosion.rfs")
	{
	}

	public override void init(Level level)
	{
		startTime = Time.currentTime;
	}

	public override void render()
	{
		base.render();

		float fadeout = MathF.Exp(-(Time.currentTime - startTime) / 1e9f * 5.0f);
		Renderer.DrawLight(position, color * fadeout, 10);
	}
}
