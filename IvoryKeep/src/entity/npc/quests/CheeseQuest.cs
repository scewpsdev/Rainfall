using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CheeseQuest : Quest
{
	int snakeKills = 0;


	public CheeseQuest()
		: base("cheese_quest", "Cheese Quest")
	{
		description = "Kill snakes for Jack so he can make his favorite cheese.";
	}

	public override bool completionRequirementsMet()
	{
		return snakeKills >= 20;
	}

	public override string getProgressString()
	{
		return snakeKills + "/20";
	}

	public override void onKill(Mob mob)
	{
		if (mob is Snake)
			snakeKills++;
	}

	public override void save(DatObject obj)
	{
		obj.addInteger("snake_kills", snakeKills);
	}

	public override void load(DatObject obj)
	{
		obj.getInteger("snake_kills", out snakeKills);
	}
}
