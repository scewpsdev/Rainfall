using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class DropRates
{
	public static readonly float[] defaultDroprates = new float[(int)ItemType.Count] {
		0.12f, // Weapon
		0.03f, // Shield
		0.12f, // Armor
		0.15f, // Food
		0.1f, // Potion
		0.03f, // Relic
		0.06f, // Staff
		0.09f, // Scroll
		0.04f,  // Spell
		0.1f, // Utility
		0.11f, // Ammo
		0.05f, // Gem
	};
	public static readonly float[] shop = new float[(int)ItemType.Count] {
		0.1f, // Weapon
		0.04f, // Shield
		0.09f, // Armor
		0.1f, // Food
		0.1f, // Potion
		0.1f, // Relic
		0.04f, // Staff
		0.09f, // Scroll
		0.16f,  // Spell
		0.08f, // Utility
		0.09f, // Ammo
		0.01f, // Gem
	};
	public static readonly float[] chest = new float[(int)ItemType.Count] {
		0.12f, // Weapon
		0.09f, // Shield
		0.12f, // Armor
		0.05f, // Food
		0.1f, // Potion
		0.0f, // Relic
		0.1f, // Staff
		0.09f, // Scroll
		0.1f,  // Spell
		0.09f, // Utility
		0.09f, // Ammo
		0.05f, // Gem
	};
	public static readonly float[] barrel = new float[(int)ItemType.Count] {
		0.09f, // Weapon
		0.02f, // Shield
		0.12f, // Armor
		0.25f, // Food
		0.1f, // Potion
		0.0f, // Relic
		0.04f, // Staff
		0.09f, // Scroll
		0.0f,  // Spell
		0.13f, // Utility
		0.12f, // Ammo
		0.04f, // Gem
	};
	public static readonly float[] ground = new float[(int)ItemType.Count] {
		0.05f, // Weapon
		0.01f, // Shield
		0.05f, // Armor
		0.2f, // Food
		0.1f, // Potion
		0.0f, // Relic
		0.03f, // Staff
		0.06f, // Scroll
		0.0f,  // Spell
		0.3f, // Utility
		0.2f, // Ammo
		0.0f, // Gem
	};
	public static readonly float[] mob = new float[(int)ItemType.Count] {
		0.13f, // Weapon
		0.08f, // Shield
		0.15f, // Armor
		0.18f, // Food
		0.1f, // Potion
		0.0f, // Relic
		0.05f, // Staff
		0.05f, // Scroll
		0.0f,  // Spell
		0.1f, // Utility
		0.11f, // Ammo
		0.05f, // Gem
	};
	public static readonly float[] caves = new float[(int)ItemType.Count] {
		0.12f, // Weapon
		0.04f, // Shield
		0.13f, // Armor
		0.15f, // Food
		0.1f, // Potion
		0.03f, // Relic
		0.06f, // Staff
		0.04f, // Scroll
		0.09f,  // Spell
		0.1f, // Utility
		0.11f, // Ammo
		0.03f, // Gem
	};
	public static readonly float[] mines = new float[(int)ItemType.Count] {
		0.08f, // Weapon
		0.05f, // Shield
		0.12f, // Armor
		0.1f, // Food
		0.1f, // Potion
		0.06f, // Relic
		0.06f, // Staff
		0.07f, // Scroll
		0.09f,  // Spell
		0.1f, // Utility
		0.12f, // Ammo
		0.05f, // Gem
	};
	public static readonly float[] dungeons = new float[(int)ItemType.Count] {
		0.08f, // Weapon
		0.05f, // Shield
		0.12f, // Armor
		0.1f, // Food
		0.1f, // Potion
		0.06f, // Relic
		0.06f, // Staff
		0.07f, // Scroll
		0.09f,  // Spell
		0.1f, // Utility
		0.12f, // Ammo
		0.05f, // Gem
	};

	static DropRates()
	{
		CheckDropRates(defaultDroprates);
		CheckDropRates(shop);
		CheckDropRates(chest);
		CheckDropRates(barrel);
		CheckDropRates(ground);
		CheckDropRates(mob);
	}

	static void CheckDropRates(float[] droprates)
	{
		float sum = 0;
		for (int i = 0; i < droprates.Length; i++)
			sum += droprates[i];
		Debug.Assert(MathF.Abs(sum - 1) < 0.001f);
	}
}
