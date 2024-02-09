using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Firebomb : Throwable
{
	public Firebomb(Entity shooter, Vector3 direction, Vector3 offset)
		: base(Item.Get("firebomb"), shooter, direction, offset)
	{
	}

	public override void onEntityHit(Entity entity)
	{
		DungeonGame.instance.level.addEntity(new Explosion(shooter), position - 0.1f * velocity.normalized, rotation);
	}
}
