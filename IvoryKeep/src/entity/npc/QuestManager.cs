using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class QuestManager
{
	public static void LoadNPCSaves(SaveFile save)
	{
		// questline npcs
		RegisterProgression<LoganSave>(save, "logan");
		RegisterProgression<RatNPCSave>(save, "rat_npc");
		RegisterProgression<BlacksmithSave>(save, "blacksmith");
		RegisterProgression<BrokenWandererSave>(save, "broken_wanderer");
		RegisterProgression<TinkererSave>(save, "tinkerer");
		RegisterProgression<SikoSave>(save, "travelling_merchant");

		// simple npcs
		RegisterProgression<NPCSaveData>(save, "builder_merchant");
		RegisterProgression<NPCSaveData>(save, "barbarian");
		RegisterProgression<NPCSaveData>(save, "knight");
		RegisterProgression<NPCSaveData>(save, "thief");
		RegisterProgression<NPCSaveData>(save, "hunter");
	}

	public static void InitNPCSaves(SaveFile save)
	{
		foreach (NPCSaveData npcSave in save.npcSaves.Values)
		{
			npcSave.init(save);
		}
	}

	public static void Update(SaveFile save)
	{
		foreach (var pair in save.npcQuests)
		{
			for (int i = 0; i < pair.Value.Count; i++)
			{
				Quest quest = pair.Value[i];
				if (quest.state == QuestState.InProgress && quest.completionRequirementsMet())
				{
					quest.state = QuestState.Completed;

					if (save.npcQuestCompletionCallbacks.TryGetValue(quest.name, out Action<Quest> callback))
						callback(quest);
					quest.onCompleted();

					if (GameState.instance.player != null)
						GameState.instance.player.hud.showMessage("Completed quest \"" + quest.displayName + "\"");
				}
			}
		}
	}

	public static NPCSaveData RegisterProgression<T>(SaveFile save, string name) where T : NPCSaveData, new()
	{
		NPCSaveData progression;
		if (save.npcSaves.ContainsKey(name))
			progression = save.npcSaves[name];
		else
		{
			progression = new T();
			progression.name = name;
			//progression.npc = npc;
			save.npcSaves.Add(name, progression);
		}

		if (save.npcData.ContainsKey(name))
		{
			DatObject npcData = save.npcData[name];
			progression.load(save, npcData);
			save.npcData.Remove(name);
		}

		return progression;
	}

	public static NPCSaveData GetProgression(SaveFile save, string name)
	{
		if (save.npcSaves.ContainsKey(name))
			return save.npcSaves[name];
		return null;
	}

	public static void LoadNPCs(SaveFile save, DatArray npcsData)
	{
		save.npcData.Clear();
		save.npcSaves.Clear();
		save.npcQuests.Clear();
		save.npcQuestCompletionCallbacks.Clear();

		for (int i = 0; i < npcsData.size; i++)
		{
			DatObject npcData = npcsData[i].obj;
			if (npcData.getIdentifier("name", out string name))
			{
				save.npcData.Add(name, npcData);
			}
		}

		LoadNPCSaves(save);
	}

	public static DatArray SaveNPCs(SaveFile save)
	{
		List<DatValue> npcValues = new List<DatValue>();
		foreach (var pair in save.npcData)
		{
			DatObject obj = save.npcData[pair.Key];
			npcValues.Add(new DatValue(obj));
		}
		foreach (var pair in save.npcSaves)
		//for (int i = 0; i < npcs.Count; i++)
		{
			DatObject obj = new DatObject();
			SaveProgression(save, pair.Key, obj);
			npcValues.Add(new DatValue(obj));
		}
		return new DatArray(npcValues.ToArray());
	}

	public static void SaveProgression(SaveFile save, string name, DatObject obj)
	{
		NPCSaveData progression = GetProgression(save, name);
		obj.addIdentifier("name", name);
		progression.save(save, obj);
	}

	public static void onKill(SaveFile save, Mob mob)
	{
		foreach (var pair in save.npcQuests)
		{
			for (int i = 0; i < pair.Value.Count; i++)
			{
				Quest quest = pair.Value[i];
				if (quest.state == QuestState.InProgress)
					pair.Value[i].onKill(mob);
			}
		}
	}

	public static void onItemPickup(SaveFile save, Item item)
	{
		foreach (var pair in save.npcQuests)
		{
			for (int i = 0; i < pair.Value.Count; i++)
			{
				Quest quest = pair.Value[i];
				if (quest.state == QuestState.InProgress)
					pair.Value[i].onItemPickup(item);
			}
		}
	}

	public static void AddActiveQuest(SaveFile save, string npc, Quest quest)
	{
		if (!save.npcQuests.ContainsKey(npc))
			save.npcQuests.Add(npc, new List<Quest>());
		save.npcQuests[npc].Add(quest);
		if (quest.state == QuestState.Uninitialized)
		{
			quest.state = QuestState.InProgress;
			GameState.instance.player.hud.showMessage($"Started quest \"{quest.displayName}\"");
		}
	}

	public static void addQuestCompletionCallback(SaveFile save, string npc, string name, Action<Quest> callback)
	{
		if (save.npcQuestCompletionCallbacks.ContainsKey(name))
		{
			Console.WriteLine("Quest complete callbacks not empty! " + save.npcQuestCompletionCallbacks[name].ToString());
			save.npcQuestCompletionCallbacks.Clear();
		}
		save.npcQuestCompletionCallbacks.Add(name, callback);
		if (tryGetQuest(save, npc, name, out Quest quest))
		{
			if (quest.state == QuestState.Completed)
				callback(quest);
		}
	}

	public static bool getQuestList(SaveFile save, string name, out List<Quest> questList)
	{
		return save.npcQuests.TryGetValue(name, out questList);
	}

	public static bool tryGetQuest(SaveFile save, string npc, string name, out Quest quest)
	{
		if (save.npcQuests.TryGetValue(npc, out List<Quest> questList))
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
