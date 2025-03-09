using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Enemy : Entity
{
	public Enemy()
	{
	}

	public override void interact(Player player)
	{
		health -= player.damage;
		if (health <= 0)
			death();
	}

	public virtual void death()
	{
		remove = true;
	}

	public override void update()
	{
		GameState.instance.moveEntity(this, x - direction, y);
		direction *= -1;
	}
}
