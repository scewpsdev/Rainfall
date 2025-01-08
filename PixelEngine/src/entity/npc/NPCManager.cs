using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


public static class NPCManager
{
	static List<NPC> npcs = new List<NPC>();
	static Dictionary<string, NPC> nameMap = new Dictionary<string, NPC>();

	public static RatNPC rat;
	public static Logan logan;
	public static BrokenWanderer brokenWanderer;
	public static Blacksmith blacksmith;


	static void RegisterNPC(NPC npc)
	{
		npcs.Add(npc);
		nameMap.Add(npc.name, npc);

		if (GameState.instance.save.file != null && GameState.instance.save.file.getArray("npcs", out DatArray npcsData))
		{
			for (int i = 0; i < npcsData.size; i++)
			{
				DatObject npcData = npcsData[i].obj;
				if (npcData.getIdentifier("name", out string name))
				{
					if (name == npc.name)
					{
						npc.load(npcData);
					}
				}
			}
		}
	}

	static void SaveNPCData(NPC npc, DatObject obj)
	{
		obj.addIdentifier("name", npc.name);
		npc.save(obj);
	}

	public static DatArray SaveNPCs()
	{
		List<DatValue> npcValues = new List<DatValue>();
		for (int i = 0; i < npcs.Count; i++)
		{
			DatObject npc = new DatObject();
			SaveNPCData(npcs[i], npc);
			npcValues.Add(new DatValue(npc));
		}
		return new DatArray(npcValues.ToArray());
	}

	public static NPC GetNPC(string name)
	{
		if (nameMap.TryGetValue(name, out NPC npc))
			return npc;
		return null;
	}

	public static void Init()
	{
		for (int i = 0; i < npcs.Count; i++)
		{
			if (npcs[i].level != null)
				npcs[i].destroy();
		}
		npcs.Clear();
		nameMap.Clear();

		RegisterNPC(rat = new RatNPC());
		RegisterNPC(logan = new Logan());
		RegisterNPC(brokenWanderer = new BrokenWanderer());
		RegisterNPC(blacksmith = new Blacksmith());
	}
}
