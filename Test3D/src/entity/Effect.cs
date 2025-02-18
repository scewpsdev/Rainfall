using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Effect : Entity
{
	Entity follow;
	Matrix followOffset;


	public Effect(string path, Entity follow = null)
	{
		this.follow = follow;

		load(path);
	}

	public override void init()
	{
		base.init();

		if (follow != null)
			followOffset = follow.getModelMatrix().inverted * getModelMatrix();
	}

	public override void update()
	{
		if (follow != null)
			setTransform(follow.getModelMatrix() * followOffset);

		base.update();

		bool hasFinished = true;
		for (int i = 0; i < particles.Count; i++)
		{
			if (!particles[i].hasFinished)
			{
				hasFinished = false;
				break;
			}
		}

		if (hasFinished)
			remove();
	}
}
