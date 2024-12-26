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

	public static StartingClass[] startingClasses;

	public static StartingClass dev;

	static StartingClass()
	{
		barbarian = new StartingClass() { name = "Barbarian", color = 0xFFa13d3b, cost = 8, items = [new LeatherArmor(), new Handaxe(), new BerserkersChain()] };
		knight = new StartingClass() { name = "Knight", color = 0xFF7582ae, cost = 8, items = [new Shortsword(), new WoodenShield(), new ChainmailHood(), new ChainmailArmor()] };
		hunter = new StartingClass() { name = "Hunter", color = 0xFF6c8c50, cost = 8, items = [new Shortbow(), new Arrow() { stackSize = 50 }, new HuntersHat(), new EaglesEye()] };
		thief = new StartingClass() { name = "Thief", color = 0xFF676767, cost = 8, items = [new Dagger(), new DarkHood(), new DarkCloak(), (new PoisonVial() { stackSize = 3 }).makeThrowable(), new Nightstalker()] };
		wizard = new StartingClass() { name = "Wizard", color = 0xFF73549d, cost = 8, items = [new MagicArrowStaff(), new WizardsHat(), new WizardsCloak(), new WizardsLegacy()], magic = 6 };
		fool = new StartingClass() { name = "Fool", color = 0xFFc89d3b, cost = 1, items = [new Club(), new GlassRing()], hp = 4 };

		dev = new StartingClass() { name = "Dev", items = [new Revolver(), new AmethystRing(), new RingOfSwiftness(), new SapphireRing()], hp = 20, magic = 20 };

		startingClasses = [barbarian, knight, hunter, thief, wizard, fool];
	}
}
