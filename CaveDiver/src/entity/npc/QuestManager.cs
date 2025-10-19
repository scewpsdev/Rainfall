using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class QuestManager
{
	public static Dictionary<string, List<Quest>> quests = new Dictionary<string, List<Quest>>();
	static Dictionary<string, Action<Quest>> questCompleteCallbacks = new Dictionary<string, Action<Quest>>();


	public static void Init()
	{
		quests.Clear();
		questCompleteCallbacks.Clear();
	}

	public static void Update()
	{
		foreach (var pair in quests)
		{
			for (int i = 0; i < pair.Value.Count; i++)
			{
				Quest quest = pair.Value[i];
				if (quest.state == QuestState.InProgress && quest.completionRequirementsMet())
				{
					quest.state = QuestState.Completed;

					if (questCompleteCallbacks.TryGetValue(quest.name, out Action<Quest> callback))
						callback(quest);
					quest.onCompleted();

					if (GameState.instance.player != null)
						GameState.instance.player.hud.showMessage("Completed quest \"" + quest.displayName + "\"");
				}
			}
		}
	}

	public static void onKill(Mob mob)
	{
		foreach (var pair in quests)
		{
			for (int i = 0; i < pair.Value.Count; i++)
			{
				Quest quest = pair.Value[i];
				if (quest.state == QuestState.InProgress)
					pair.Value[i].onKill(mob);
			}
		}
	}

	public static void onItemPickup(Item item)
	{
		foreach (var pair in quests)
		{
			for (int i = 0; i < pair.Value.Count; i++)
			{
				Quest quest = pair.Value[i];
				if (quest.state == QuestState.InProgress)
					pair.Value[i].onItemPickup(item);
			}
		}
	}

	public static void AddActiveQuest(string npc, Quest quest)
	{
		if (!quests.ContainsKey(npc))
			quests.Add(npc, new List<Quest>());
		quests[npc].Add(quest);
		if (quest.state == QuestState.Uninitialized)
		{
			quest.state = QuestState.InProgress;
			GameState.instance.player.hud.showMessage($"Started quest \"{quest.displayName}\"");
		}
	}

	public static void addQuestCompletionCallback(string npc, string name, Action<Quest> callback)
	{
		if (questCompleteCallbacks.ContainsKey(name))
		{
			Console.WriteLine("Quest complete callbacks not empty! " + questCompleteCallbacks[name].ToString());
			questCompleteCallbacks.Clear();
		}
		questCompleteCallbacks.Add(name, callback);
		if (tryGetQuest(npc, name, out Quest quest))
		{
			if (quest.state == QuestState.Completed)
				callback(quest);
		}
	}

	public static bool getQuestList(string name, out List<Quest> questList)
	{
		return quests.TryGetValue(name, out questList);
	}

	public static bool tryGetQuest(string npc, string name, out Quest quest)
	{
		if (quests.TryGetValue(npc, out List<Quest> questList))
		{
			for (int i = 0; i < questList.Count; i++)
			{
				if (questList[i].name == name)
				{
					quest = questList[i];
					return true;
				}
			}
		}
		quest = null;
		return false;
	}
}
