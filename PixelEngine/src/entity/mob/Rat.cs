using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rat : Mob
{
	public Rat()
		: base("rat")
	{
		displayName = "Rat";

		sprite = new Sprite(Resource.GetTexture("res/sprites/rat.png", false));

		collider = new FloatRect(-0.4f, 0, 0.8f, 0.55f);

		ai = new WanderAI();

		health = 4;
	}
}
