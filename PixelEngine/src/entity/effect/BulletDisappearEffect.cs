using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BulletDisappearEffect : AnimatedEffect
{
	public BulletDisappearEffect()
		: base(Resource.GetTexture("res/sprites/bullet_disappear.png", false), 24)
	{
		additive = true;
		color = new Vector4(3, 3, 3, 1);
	}
}
