using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


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

public static class SaveFile
{
	public static RunData[] highscores;


	public static void Load(int saveID)
	{
#if DEBUG
		if (saveID != 2)
#else
		if (File.Exists(path))
#endif
		{
			string path = "save" + saveID + ".dat";
			DatFile dat = new DatFile(File.ReadAllText(path), path);
			DatArray highscoresDat = dat.getField("highscores").array;
			highscores = new RunData[highscoresDat.size];
			for (int i = 0; i < highscoresDat.size; i++)
				highscores[i] = LoadRun(highscoresDat[i].obj);
		}
		else
		{
			highscores = new RunData[4];
			for (int i = 0; i < highscores.Length; i++)
			{
				highscores[i].score = 0;
				highscores[i].floor = -1;
				highscores[i].time = -1;
				highscores[i].kills = 0;
				highscores[i].handItems = new Item[2];
				highscores[i].activeItems = new Item[5];
				highscores[i].passiveItems = new Item[4];
			}

			Save(saveID);
		}
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

	public static void OnRunFinished(RunStats run, int saveID)
	{
		if (!run.isCustomRun)
		{
			if (run.score > highscores[0].score)
			{
				HighscoreRun(run, 0, saveID);
				run.scoreRecord = true;
			}
			if (run.floor > highscores[1].floor)
			{
				HighscoreRun(run, 1, saveID);
				run.floorRecord = true;
			}
			if (run.hasWon && (run.duration < highscores[2].time || highscores[2].time == -1))
			{
				HighscoreRun(run, 2, saveID);
				run.timeRecord = true;
			}
			if (run.kills > highscores[3].kills)
			{
				HighscoreRun(run, 3, saveID);
				run.killRecord = true;
			}
		}
	}

	static void HighscoreRun(RunStats run, int idx, int saveID)
	{
		Player player = GameState.instance.player;

		highscores[idx].score = run.score;
		highscores[idx].floor = run.floor;
		highscores[idx].time = run.duration;
		highscores[idx].kills = run.kills;

		highscores[idx].handItems = new Item[2];
		highscores[idx].handItems[0] = player.handItem != null ? player.handItem.copy() : null;
		highscores[idx].handItems[1] = player.offhandItem != null ? player.offhandItem.copy() : null;

		highscores[idx].activeItems = new Item[player.activeItems.Length];
		for (int i = 0; i < player.activeItems.Length; i++)
			highscores[idx].activeItems[i] = player.activeItems[i] != null ? player.activeItems[i].copy() : null;

		highscores[idx].passiveItems = new Item[player.passiveItems.Length];
		for (int i = 0; i < player.passiveItems.Length; i++)
			highscores[idx].passiveItems[i] = player.passiveItems[i] != null ? player.passiveItems[i].copy() : null;

		Save(saveID);
	}

	public static void Save(int saveID)
	{
		DatFile file = new DatFile();

		DatValue[] highscoresDat = new DatValue[highscores.Length];
		for (int i = 0; i < highscores.Length; i++)
		{
			DatObject run = new DatObject();
			run.addInteger("score", highscores[i].score);
			run.addInteger("floor", highscores[i].floor);
			run.addNumber("time", highscores[i].time);
			run.addInteger("kills", highscores[i].kills);

			DatValue[] items0 = new DatValue[highscores[i].handItems.Length];
			for (int j = 0; j < highscores[i].handItems.Length; j++)
				items0[j] = new DatValue(highscores[i].handItems[j] != null ? highscores[i].handItems[j].id : 0);
			run.addArray("items0", new DatArray(items0));

			DatValue[] items1 = new DatValue[highscores[i].activeItems.Length];
			for (int j = 0; j < highscores[i].activeItems.Length; j++)
				items1[j] = new DatValue(highscores[i].activeItems[j] != null ? highscores[i].activeItems[j].id : 0);
			run.addArray("items1", new DatArray(items1));

			DatValue[] items2 = new DatValue[highscores[i].passiveItems.Length];
			for (int j = 0; j < highscores[i].passiveItems.Length; j++)
				items2[j] = new DatValue(highscores[i].passiveItems[j] != null ? highscores[i].passiveItems[j].id : 0);
			run.addArray("items2", new DatArray(items2));

			highscoresDat[i] = new DatValue(run);
		}

		file.addArray("highscores", new DatArray(highscoresDat));

		string path = "save" + saveID + ".dat";
		file.serialize(path);

#if DEBUG
		Utils.RunCommand("xcopy", "/y \"" + path + "\" \"..\\..\\..\\\"");
#endif

		Console.WriteLine("Saved global save");
	}
}
