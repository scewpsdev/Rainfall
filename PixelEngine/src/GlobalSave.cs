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
	public int time;
	public int kills;
	public Item[] handItems;
	public Item[] activeItems;
	public Item[] passiveItems;
}

public static class GlobalSave
{
	const string path = "save0.dat";

	public static RunData[] highscores;


	public static void Load()
	{
		DatFile dat = new DatFile(File.ReadAllText(path), path);
		DatArray highscoresDat = dat.getField("highscores").array;
		highscores = new RunData[highscoresDat.size];
		for (int i = 0; i < highscoresDat.size; i++)
			highscores[i] = LoadRun(highscoresDat[i].obj);
	}

	static RunData LoadRun(DatObject run)
	{
		RunData data = new RunData();
		run.getInteger("score", out data.score);
		run.getInteger("time", out data.time);
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

	public static void Save()
	{
		Utils.RunCommand("xcopy", "/y \"" + path + "\" \"..\\..\\..\\\"");
	}
}
