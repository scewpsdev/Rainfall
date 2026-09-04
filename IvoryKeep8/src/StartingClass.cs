using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StartingClass
{
	public string name = "???";
	public uint color = 0xFFFF00FF;
	public int cost;
	public Item[] items;

	public int strength = 1;
	public int dexterity = 1;
	public int intelligence = 1;
	public int hp = 6;
	public int magic = 4;


	public static StartingClass barbarian;
	public static StartingClass knight;
	public static StartingClass hunter;
	public static StartingClass thief;
	public static StartingClass wizard;
	public static StartingClass fool;
	public static StartingClass dev;

	public static StartingClass[] startingClasses;

	static StartingClass()
	{
		barbarian = new StartingClass() { name = "Barbarian", color = 0xFFa13d3b, cost = 12, items = [new Handaxe(), new BerserkersChain()], hp = 8, magic = 2, strength = 2 };
		knight = new StartingClass() { name = "Knight", color = 0xFF7582ae, cost = 12, items = [new Shortsword(), new WoodenShield(), new ChainmailHood(), new ChainmailArmor()], hp = 6, magic = 4 };
		hunter = new StartingClass() { name = "Hunter", color = 0xFF6c8c50, cost = 12, items = [new Shortbow(), /*new Arrow() { stackSize = 50 }, */new HuntersHat(), new EaglesEye()], hp = 6, magic = 2 };
		thief = new StartingClass() { name = "Thief", color = 0xFF676767, cost = 12, items = [new Dagger(), new DarkHood(), new DarkCloak(), (new GreenPotion() { stackSize = 3 }).makeThrowable(), new ThrowingKnife() { stackSize = 5 }, /*new Parsley(),*/], hp = 5, magic = 4, dexterity = 2 };
		wizard = new StartingClass() { name = "Wizard", color = 0xFF73549d, cost = 12, items = [/*new MagicArrowStaff(),*/ new MagicStaff(), new MagicArrow(), new WizardHat(), new WizardsCloak()/*, new WizardsLegacy()*/], hp = 5, magic = 5, intelligence = 2 };
		fool = new StartingClass() { name = "Fool", color = 0xFFc89d3b, cost = 12, items = [new Club(), new GlassRing()], hp = 4, magic = 4 };

		dev = new StartingClass() { name = "Dev", items = [new Revolver(), new Jetpack(), new AmethystRing(), new RingOfSwiftness(), new SapphireRing()], hp = 20, magic = 20 };

		startingClasses = [barbarian, knight, hunter, thief, wizard, fool];
	}

	public static StartingClass GetByName(string name)
	{
		for (int i = 0; i < startingClasses.Length; i++)
		{
			if (startingClasses[i].name.ToLower() == name.ToLower())
				return startingClasses[i];
		}
		return null;
	}
}
