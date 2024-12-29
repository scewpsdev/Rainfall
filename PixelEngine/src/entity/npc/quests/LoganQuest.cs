using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LoganQuest : Quest
{
	bool staffFound = false;


	public LoganQuest()
		: base("logan_quest", "Ancient staff")
	{
		description = "Find an ancient magic staff hidden in the weeping catacombs.";
	}

	public override bool completionRequirementsMet()
	{
		return GameState.instance.player.getItem("questline_logan_staff") != null;
	}

	public override string getProgressString()
	{
		return staffFound ? "Found" : "Not found";
	}

	public override void onItemPickup(Item item)
	{
		if (item.name == "questline_logan_staff")
			staffFound = true;
	}

	public override void save(DatObject obj)
	{
		obj.addBoolean("staff_found", staffFound);
	}

	public override void load(DatObject obj)
	{
		obj.getBoolean("staff_found", out staffFound);
	}
}
