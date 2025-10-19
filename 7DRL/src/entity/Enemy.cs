using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Enemy : Entity
{
	public bool stunned = false;


	public Enemy()
	{
	}

	public override void interact(Player player)
	{
		health -= player.damage;
		if (health <= 0)
			death();
		else
		{
			stunned = true;
		}
	}

	public virtual void death()
	{
		remove = true;
	}

	public override void update()
	{
		if (stunned)
		{
			stunned = false;
		}
		else
		{
			GameState.instance.moveEntity(this, x - direction, y);
			direction *= -1;
		}
	}
}
