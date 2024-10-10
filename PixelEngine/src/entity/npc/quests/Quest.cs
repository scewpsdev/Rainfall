using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum QuestState
{
	Uninitialized,
	InProgress,
	Completed,
	Collected,
}

public abstract class Quest
{
	public static Dictionary<string, Quest> questInstances = new Dictionary<string, Quest>();

	static Quest()
	{
		InitQuestType(new CheeseQuest());
	}

	static void InitQuestType(Quest quest)
	{
		questInstances.Add(quest.name, quest);
	}


	public string name;
	public string displayName;
	public string description;

	public QuestState state = QuestState.Uninitialized;
	public bool isRunning => state == QuestState.InProgress;
	public bool isCompleted => state == QuestState.Completed;
	public bool isCollected => state == QuestState.Collected;
	public void collect() { state = QuestState.Collected; }


	public Quest(string name, string displayName)
	{
		this.name = name;
		this.displayName = displayName;
	}


	public abstract bool completionRequirementsMet();
	public abstract string getProgressString();

	public abstract void save(DatObject obj);
	public abstract void load(DatObject obj);

	public virtual void onCompleted()
	{
	}

	public virtual void onKill(Mob mob)
	{
	}
}
