using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BulletDisappearEffect : AnimatedEffect
{
	public BulletDisappearEffect()
		: base(Resource.GetTexture("sprites/bullet_disappear.png", false), 0.2f)
	{
		additive = true;
		color = new Vector4(3, 3, 3, 1);
	}
}
