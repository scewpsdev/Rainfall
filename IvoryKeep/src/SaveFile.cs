using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;


public struct RunData
{
	public int score;
	public int floor;
	public float time;
	public int kills;
	public Item[] handItems;
	public Item[] activeItems;
	public Item[] passiveItems;
}

public class SaveFile
{
	public static readonly uint FLAG_TUTORIAL_FINISHED = Hash.hash("tutorial_finished");

	public static readonly uint FLAG_CAVES_FOUND = Hash.hash("caves_found");
	public static readonly uint FLAG_MINES_FOUND = Hash.hash("mines_found");
	public static readonly uint FLAG_DUNGEONS_FOUND = Hash.hash("dungeons_found");

	public static readonly uint FLAG_CASTLE_UNLOCKED = Hash.hash("castle_unlocked");

	public static readonly uint FLAG_NPC_RAT_MET = Hash.hash("rat_questline_init");
	public static readonly uint FLAG_NPC_RAT_QUESTLINE_COMPLETED = Hash.hash("rat_questline_complete");

	public static readonly uint FLAG_NPC_TRAVELLER_MET = Hash.hash("traveller_questline_init");

	public static readonly uint FLAG_NPC_BLACKSMITH_MET = Hash.hash("blacksmith_questline_init");

	public static readonly uint FLAG_NPC_TINKERER_MET = Hash.hash("tinkerer_questline_init");

	public static readonly uint FLAG_NPC_GATEKEEPER_MET = Hash.hash("gatekeeper_questline_init");

	public static readonly uint FLAG_NPC_LOGAN_MET = Hash.hash("logan_questline_init");

	public static readonly uint FLAG_NPC_BARBARIAN_MET = Hash.hash("barbarian_questline_init");
	public static readonly uint FLAG_NPC_KNIGHT_MET = Hash.hash("knight_questline_init");
	public static readonly uint FLAG_NPC_HUNTER_MET = Hash.hash("hunter_questline_init");
	public static readonly uint FLAG_NPC_THIEF_MET = Hash.hash("thief_questline_init");


	public static SaveFile customRun => new SaveFile() { id = -1, isCustom = true, flags = [FLAG_TUTORIAL_FINISHED] };
	public static SaveFile dailyRun => new SaveFile() { id = -1, isDaily = true };


	public int id;
	public string path;
	public DatFile file;
	public bool isDaily, isCustom;

	public int runsFinished = 0;
	public RunData[] highscores = new RunData[0];
	public HashSet<uint> flags = new HashSet<uint>();

	public string currentCheckpointLevel;
	public Vector2 currentCheckpoint;


	public bool hasFlag(uint flag)
	{
		return flags.Contains(flag);
	}

	public void setFlag(uint flag)
	{
		if (!flags.Contains(flag))
			flags.Add(flag);
	}

	public void clearFlag(uint flag)
	{
		Debug.Assert(flags.Remove(flag));
	}

	public void unlockStartingClass(StartingClass startingClass)
	{
		uint h = Hash.hash(startingClass.name);
		if (!hasFlag(h))
		{
			setFlag(h);
			GameState.instance.player.hud.showMessage($"Unlocked starting class \"{startingClass.name}\"!");
		}
	}

	public bool isStartingClassUnlocked(StartingClass startingClass)
	{
		return hasFlag(Hash.hash(startingClass.name));
	}


	public static SaveFile Load(int saveID)
	{
		string path = "saves/save" + (saveID + 1) + ".dat";
		SaveFile save = new SaveFile() { id = saveID, path = path };

#if DEBUG
		if (File.Exists(path) && saveID != 2)
#else
		if (File.Exists(path))
#endif
		{
			DatFile dat = new DatFile(File.ReadAllText(path), path);
			save.file = dat;

			dat.getInteger("runs_finished", out save.runsFinished);

			DatArray flagsDat = dat.getField("flags")?.array;
			if (flagsDat != null)
			{
				for (int i = 0; i < flagsDat.size; i++)
					save.flags.Add(flagsDat[i].uinteger);
			}

			DatArray highscoresDat = dat.getField("highscores").array;
			save.highscores = new RunData[highscoresDat.size];
			for (int i = 0; i < highscoresDat.size; i++)
				save.highscores[i] = LoadRun(highscoresDat[i].obj);

			dat.getStringContent("checkpoint_level", out save.currentCheckpointLevel);
			dat.getVector2("checkpoint_position", out save.currentCheckpoint);

			LoadInventory(save, dat, GameState.instance.player);
		}
		else
		{
			save.highscores = new RunData[4];
			for (int i = 0; i < save.highscores.Length; i++)
			{
				save.highscores[i].score = 0;
				save.highscores[i].floor = -1;
				save.highscores[i].time = -1;
				save.highscores[i].kills = 0;
				save.highscores[i].handItems = new Item[2];
				save.highscores[i].activeItems = new Item[5];
				save.highscores[i].passiveItems = new Item[4];
			}

			save.file = Save(save);
		}

		QuestManager.Update();

		return save;
	}

	static RunData LoadRun(DatObject run)
	{
		RunData data = new RunData();
		run.getInteger("score", out data.score);
		run.getInteger("floor", out data.floor);
		run.getNumber("time", out data.time);
		run.getInteger("kills", out data.kills);

		run.getArray("items0", out DatArray items0);
		data.handItems = new Item[items0.size];
		for (int i = 0; i < items0.size; i++)
			data.handItems[i] = Item.GetItemPrototype(items0[i].uinteger);

		run.getArray("items1", out DatArray items1);
		data.activeItems = new Item[items1.size];
		for (int i = 0; i < items1.size; i++)
			data.activeItems[i] = Item.GetItemPrototype(items1[i].uinteger);

		run.getArray("items2", out DatArray items2);
		data.passiveItems = new Item[items2.size];
		for (int i = 0; i < items2.size; i++)
			data.passiveItems[i] = Item.GetItemPrototype(items2[i].uinteger);

		return data;
	}

	static void LoadInventory(SaveFile save, DatFile file, Player player)
	{
		if (file.getArray("items", out DatArray itemsArray))
		{
			for (int i = 0; i < itemsArray.size; i++)
			{
				itemsArray[i].obj.getIdentifier("type", out string itemType);
				Item item = Item.GetItemPrototype(itemType).copy();
				itemsArray[i].obj.getInteger("stack_size", out item.stackSize);
				player.items.Add(item);
				item.deserialize(itemsArray[i].obj);
			}
		}

		if (file.getInteger("hand_item", out int handItemIdx))
		{
			if (handItemIdx != -1)
				player.handItem = player.items[handItemIdx];
		}

		if (file.getInteger("offhand_item", out int offhandItemIdx))
		{
			if (offhandItemIdx != -1)
				player.offhandItem = player.items[offhandItemIdx];
		}

		if (file.getInteger("num_active_items", out int numActiveItems))
		{
			for (int i = 0; i < numActiveItems; i++)
			{
				file.getInteger("active_item" + i, out int activeItemIdx);
				if (activeItemIdx != -1)
					player.activeItems[i] = player.items[activeItemIdx];
			}
		}

		if (file.getInteger("num_spell_items", out int numSpellItems))
		{
			for (int i = 0; i < numSpellItems; i++)
			{
				file.getInteger("spell_item" + i, out int spellItemIdx);
				if (spellItemIdx != -1)
					player.spellItems.Add((Spell)player.items[spellItemIdx]);
			}
		}

		if (file.getInteger("num_passive_items", out int numPassiveItems))
		{
			for (int i = 0; i < numPassiveItems; i++)
			{
				file.getInteger("passive_item" + i, out int passiveItemIdx);
				if (passiveItemIdx != -1)
					player.passiveItems.Add(player.items[passiveItemIdx]);
			}
		}

		if (file.getInteger("num_stored_items", out int numStoredItems))
		{
			for (int i = 0; i < numStoredItems; i++)
			{
				file.getInteger("stored_item" + i, out int storedItemIdx);
				if (storedItemIdx != -1)
					player.storedItems.Add(player.items[storedItemIdx]);
			}
		}

		if (file.getIdentifier("carried_object", out string carriedObject))
		{
			Entity obj = EntityType.CreateInstance(carriedObject);
			if (obj != null)
				player.carriedObject = (Object)obj;
		}
	}

	public static DatFile Save(SaveFile save)
	{
		DatFile file = new DatFile();

		{
			DatValue[] flags = new DatValue[save.flags.Count];
			int i = 0;
			foreach (uint flag in save.flags)
			{
				flags[i++] = new DatValue(flag);
			}
			file.addArray("flags", new DatArray(flags));
		}

		file.addInteger("runs_finished", save.runsFinished);

		DatValue[] highscoresDat = new DatValue[save.highscores.Length];
		for (int i = 0; i < save.highscores.Length; i++)
		{
			DatObject run = new DatObject();
			run.addInteger("score", save.highscores[i].score);
			run.addInteger("floor", save.highscores[i].floor);
			run.addNumber("time", save.highscores[i].time);
			run.addInteger("kills", save.highscores[i].kills);

			DatValue[] items0 = new DatValue[save.highscores[i].handItems.Length];
			for (int j = 0; j < save.highscores[i].handItems.Length; j++)
				items0[j] = new DatValue(save.highscores[i].handItems[j] != null ? save.highscores[i].handItems[j].id : 0);
			run.addArray("items0", new DatArray(items0));

			DatValue[] items1 = new DatValue[save.highscores[i].activeItems.Length];
			for (int j = 0; j < save.highscores[i].activeItems.Length; j++)
				items1[j] = new DatValue(save.highscores[i].activeItems[j] != null ? save.highscores[i].activeItems[j].id : 0);
			run.addArray("items1", new DatArray(items1));

			DatValue[] items2 = new DatValue[save.highscores[i].passiveItems.Length];
			for (int j = 0; j < save.highscores[i].passiveItems.Length; j++)
				items2[j] = new DatValue(save.highscores[i].passiveItems[j] != null ? save.highscores[i].passiveItems[j].id : 0);
			run.addArray("items2", new DatArray(items2));

			highscoresDat[i] = new DatValue(run);
		}
		file.addArray("highscores", new DatArray(highscoresDat));

		if (save.currentCheckpointLevel != null)
		{
			file.addString("checkpoint_level", save.currentCheckpointLevel);
			file.addVector2("checkpoint_position", save.currentCheckpoint);
		}

		SaveInventory(save, file, GameState.instance.player);

		file.addArray("npcs", NPCManager.SaveNPCs());

		file.serialize(save.path);
		save.file = file;

#if DEBUG
		Utils.RunCommand("xcopy", "/y \"" + save.path + "\" \"..\\..\\..\\saves\\\"");
#endif

		Console.WriteLine("Saved file " + save.id);

		return file;
	}

	static void SaveInventory(SaveFile save, DatFile file, Player player)
	{
		DatValue[] itemValues = new DatValue[player.items.Count];
		for (int i = 0; i < player.items.Count; i++)
		{
			DatObject itemObj = new DatObject()
			{
				fields =
				[
					new DatField("type", new DatValue(player.items[i].name, DatValueType.Identifier)),
					new DatField("stack_size", new DatValue(player.items[i].stackSize))
				]
			};
			player.items[i].serialize(itemObj);
			itemValues[i] = new DatValue(itemObj);
		}
		file.addArray("items", new DatArray(itemValues));

		file.addInteger("hand_item", player.handItem != null ? player.items.IndexOf(player.handItem) : -1);
		file.addInteger("offhand_item", player.offhandItem != null ? player.items.IndexOf(player.offhandItem) : -1);

		file.addInteger("num_active_items", player.activeItems.Length);
		for (int i = 0; i < player.activeItems.Length; i++)
			file.addInteger("active_item" + i, player.activeItems[i] != null ? player.items.IndexOf(player.activeItems[i]) : -1);
		file.addInteger("num_spell_items", player.spellCapacity);
		for (int i = 0; i < player.spellCapacity; i++)
			file.addInteger("spell_item" + i, i < player.spellItems.Count ? player.items.IndexOf(player.spellItems[i]) : -1);
		file.addInteger("num_passive_items", player.passiveItems.Count);
		for (int i = 0; i < player.passiveItems.Count; i++)
			file.addInteger("passive_item" + i, i < player.passiveItems.Count ? player.items.IndexOf(player.passiveItems[i]) : -1);
		file.addInteger("num_stored_items", player.storeCapacity);
		for (int i = 0; i < player.storeCapacity; i++)
			file.addInteger("stored_item" + i, i < player.storedItems.Count ? player.items.IndexOf(player.storedItems[i]) : -1);

		if (player.carriedObject != null)
			file.addIdentifier("carried_object", player.carriedObject.name);
	}

	public static void LoadQuest(DatObject obj, Quest quest, NPC npc)
	{
		if (obj.getArray("quests", out DatArray quests))
		{
			for (int i = 0; i < quests.size; i++)
			{
				if (quests[i].obj.getIdentifier("name", out string questName) && questName == quest.name)
				{
					QuestState state = Utils.ParseEnum<QuestState>(quests[i].obj.getField("state").identifier);
					quest.state = state;
					quest.load(quests[i].obj);

					if (quest.state == QuestState.InProgress || quest.state == QuestState.Completed)
						QuestManager.AddActiveQuest(npc.name, quest);
				}
			}
		}
	}

	public static void SaveQuest(DatObject obj, Quest quest, NPC npc)
	{
		if (!obj.getArray("quests", out DatArray quests))
			obj.addArray("quests", quests = new DatArray());
		DatObject questDat = new DatObject();
		questDat.addIdentifier("name", quest.name);
		questDat.addIdentifier("state", quest.state.ToString());
		quest.save(questDat);
		quests.addObject(questDat);
	}

	public static void OnRunFinished(RunStats run, SaveFile save)
	{
		if (!run.isCustomRun)
		{
			if (run.hasWon && (run.duration < save.highscores[0].time || save.highscores[0].time == -1))
			{
				HighscoreRun(run, 0, save);
				run.timeRecord = true;
			}
			if (run.score > save.highscores[1].score)
			{
				HighscoreRun(run, 1, save);
				run.scoreRecord = true;
			}
			if (run.floor > save.highscores[2].floor)
			{
				HighscoreRun(run, 2, save);
				run.floorRecord = true;
			}
			if (run.kills > save.highscores[3].kills)
			{
				HighscoreRun(run, 3, save);
				run.killRecord = true;
			}

			save.runsFinished++;
		}
	}

	static void HighscoreRun(RunStats run, int idx, SaveFile save)
	{
		Player player = GameState.instance.player;

		save.highscores[idx].score = run.score;
		save.highscores[idx].floor = run.floor;
		save.highscores[idx].time = run.duration;
		save.highscores[idx].kills = run.kills;

		save.highscores[idx].handItems = new Item[2];
		save.highscores[idx].handItems[0] = player.handItem != null ? player.handItem.copy() : null;
		save.highscores[idx].handItems[1] = player.offhandItem != null ? player.offhandItem.copy() : null;

		save.highscores[idx].activeItems = new Item[player.activeItems.Length];
		for (int i = 0; i < player.activeItems.Length; i++)
			save.highscores[idx].activeItems[i] = player.activeItems[i] != null ? player.activeItems[i].copy() : null;

		save.highscores[idx].passiveItems = new Item[player.passiveItems.Count];
		for (int i = 0; i < player.passiveItems.Count; i++)
			save.highscores[idx].passiveItems[i] = player.passiveItems[i].copy();

		Save(save);
	}
}
