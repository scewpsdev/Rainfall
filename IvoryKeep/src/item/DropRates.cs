using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class DropRates
{
	public static readonly float[] defaultDroprates = new float[(int)ItemType.Count] {
		0.18f, // Weapon
		0.04f, // Shield
		0.17f, // Armor
		0.18f, // Food
		0.05f, // Potion
		0.01f, // Relic
		0.09f, // Staff
		0.03f, // Scroll
		0.11f, // Spell
		0.11f, // Utility
		0.02f, // Ammo
		0.02f, // Gem
	};

	public static readonly float[] cavesDroprates = new float[(int)ItemType.Count] {
		0.18f, // Weapon
		0.04f, // Shield
		0.17f, // Armor
		0.18f, // Food
		0.05f, // Potion
		0.01f, // Relic
		0.09f, // Staff
		0.03f, // Scroll
		0.11f, // Spell
		0.11f, // Utility
		0.02f, // Ammo
		0.02f, // Gem
	};

	public static readonly float[] minesDroprates = new float[(int)ItemType.Count] {
		0.08f, // Weapon
		0.07f, // Shield
		0.20f, // Armor
		0.12f, // Food
		0.07f, // Potion
		0.04f, // Relic
		0.06f, // Staff
		0.04f, // Scroll
		0.10f, // Spell
		0.08f, // Utility
		0.10f, // Ammo
		0.04f, // Gem
	};


	static DropRates()
	{
		NormalizeDropRates(cavesDroprates);
	}

	static void NormalizeDropRates(float[] droprates)
	{
		float sum = 0;
		for (int i = 0; i < droprates.Length; i++)
			sum += droprates[i];
		for (int i = 0; i < droprates.Length; i++)
			droprates[i] /= sum;
		//Debug.Assert(MathF.Abs(sum - 1) < 0.001f);
	}
}
