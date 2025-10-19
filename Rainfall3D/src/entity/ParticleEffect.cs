using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class ParticleEffect : Entity
{
	string file;
	public Entity follow;

	Matrix offset = Matrix.Identity;


	public ParticleEffect(string file, Entity follow = null)
	{
		this.file = file;
		this.follow = follow;
	}

	public override void init()
	{
		base.init();

		load(file);

		if (follow != null)
		{
			offset = follow.getModelMatrix().inverted * getModelMatrix();
		}
	}

	public override void update()
	{
		if (follow != null)
		{
			setTransform(follow.getModelMatrix() * offset);
		}

		bool hasFinished = true;
		for (int i = 0; i < particles.Count; i++)
		{
			if (!particles[i].hasFinished)
				hasFinished = false;
		}

		if (hasFinished)
			remove();
	}
}
