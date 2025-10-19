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
		0.04f, // Shield
		0.13f, // Armor
		0.15f, // Food
		0.1f, // Potion
		0.12f, // Relic
		0.06f, // Staff
		0.04f, // Scroll
		//0.0f,  // Spell
		0.1f, // Utility
		0.11f, // Ammo
		0.03f, // Gem
	};


	static DropRates()
	{
		NormalizeDropRates(defaultDroprates);
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
