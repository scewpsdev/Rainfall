using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class QuestManager
{
	static Dictionary<string, DatObject> loadedNPCSaves = new Dictionary<string, DatObject>();
	static Dictionary<string, NPCSaveData> progressions = new Dictionary<string, NPCSaveData>();
	public static Dictionary<string, List<Quest>> quests = new Dictionary<string, List<Quest>>();
	static Dictionary<string, Action<Quest>> questCompleteCallbacks = new Dictionary<string, Action<Quest>>();


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

	public static NPCSaveData RegisterProgression(NPC npc)
	{
		NPCSaveData progression;
		if (progressions.ContainsKey(npc.name))
			progression = progressions[npc.name];
		else
		{
			progression = npc.createSave();
			progressions.Add(npc.name, progression);
		}

		if (loadedNPCSaves.ContainsKey(npc.name))
		{
			DatObject npcData = loadedNPCSaves[npc.name];
			progression.load(npcData);
			loadedNPCSaves.Remove(npc.name);
		}

		return progression;
	}

	public static NPCSaveData GetProgression(string name)
	{
		if (progressions.ContainsKey(name))
			return progressions[name];
		return null;
	}

	public static void LoadNPCs(DatArray npcsData)
	{
		loadedNPCSaves.Clear();
		progressions.Clear();
		quests.Clear();
		questCompleteCallbacks.Clear();

		for (int i = 0; i < npcsData.size; i++)
		{
			DatObject npcData = npcsData[i].obj;
			if (npcData.getIdentifier("name", out string name))
			{
				loadedNPCSaves.Add(name, npcData);
			}
		}
	}

	public static DatArray SaveNPCs()
	{
		List<DatValue> npcValues = new List<DatValue>();
		foreach (var pair in loadedNPCSaves)
		{
			DatObject obj = loadedNPCSaves[pair.Key];
			npcValues.Add(new DatValue(obj));
		}
		foreach (var pair in progressions)
		//for (int i = 0; i < npcs.Count; i++)
		{
			DatObject obj = new DatObject();
			SaveProgression(pair.Key, obj);
			npcValues.Add(new DatValue(obj));
		}
		return new DatArray(npcValues.ToArray());
	}

	public static void SaveProgression(string name, DatObject obj)
	{
		NPCSaveData progression = GetProgression(name);
		obj.addIdentifier("name", name);
		progression.save(obj);
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
