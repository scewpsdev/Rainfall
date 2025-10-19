using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TestQuest : Quest
{
	public TestQuest()
		: base("test_quest", "Test Quest")
	{
		description = "This is a test quest. Very epical";
	}

	public override bool completionRequirementsMet()
	{
		return false;
	}

	public override string getProgressString()
	{
		return "50/100";
	}

	public override void save(DatObject obj)
	{
	}

	public override void load(DatObject obj)
	{
	}
}
