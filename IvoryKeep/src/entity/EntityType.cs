﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class EntityType
{
	static List<Func<Entity>> createFuncs = new List<Func<Entity>>();
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();

	public static void InitTypes()
	{
		InitType<Bat>("bat");
		InitType<Gandalf>("gandalf");
		InitType<GreenSpider>("green_spider");
		InitType<OrangeBat>("orange_bat");
		InitType<Rat>("rat");
		InitType<SkeletonArcher>("skeleton_archer");
		InitType<Snake>("snake");
		InitType<Spider>("spider");
		InitType<Golem>("golem");
		InitType<GolemBoss>("golem_boss");
		InitType<Slime>("slime");
		InitType<BlueSlime>("blue_slime");
		InitType<Leprechaun>("leprechaun");
		InitType<Raya>("raya");
		InitType<Beetle>("beetle");
		InitType<Stalker>("stalker");

		InitType<Blacksmith>("blacksmith");
		InitType<BuilderMerchant>("builder_merchant");
		InitType<Logan>("logan");
		InitType<RatNPC>("rat_npc");
		InitType<Tinkerer>("tinkerer");
		InitType<TravellingMerchant>("travelling_merchant");
		InitType<Barbarian>("barbarian");

		InitType<Fountain>("fountain");
		InitType<Checkpoint>("checkpoint");
		InitType<Barrel>("barrel");
		InitType<Chest>("chest");
		InitType<ExplosiveBarrel>("explosive_barrel");
		InitType<Spike>("spike");
		InitType<SpikeTrap>("spike_trap");
		InitType<Trampoline>("trampoline");
		InitType<TorchEntity>("torch");
		InitType<CoinStack>("coin_stack");
		InitType<Minecart>("minecart");
		InitType<IronDoor>("iron_door");
		InitType<Pot>("pot");
		InitType<FireSconce>("fire_sconce");
		InitType<Crate>("crate");
		InitType<ExplosiveCrate>("explosive_crate");
		InitType<ExplosivePot>("explosive_pot");
		InitType<Rock>("rock");
		InitType<Skull>("skull");
	}

	static void InitType<T>(string name) where T : Entity, new()
	{
		Func<Entity> createFunc = () =>
		{
			Entity e = new T();
			e.name = name;
			return e;
		};
		createFuncs.Add(createFunc);
		nameMap.Add(name, createFuncs.Count - 1);
	}

	public static Entity CreateInstance(string name)
	{
		if (nameMap.TryGetValue(name, out int idx))
			return createFuncs[idx].Invoke();
		return null;
	}
}
