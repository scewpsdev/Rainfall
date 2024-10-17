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
	public int hp = 3;
	public int magic = 2;


	public static StartingClass barbarian;
	public static StartingClass knight;
	public static StartingClass hunter;
	public static StartingClass thief;
	public static StartingClass wizard;
	public static StartingClass fool;

	public static StartingClass dev;

	static StartingClass()
	{
		barbarian = new StartingClass() { name = "Barbarian", color = 0xFFa13d3b, cost = 8, items = [new LeatherArmor(), new Handaxe(), new BerserkersChain()] };
		knight = new StartingClass() { name = "Knight", color = 0xFF7582ae, cost = 8, items = [new Shortsword(), new IronShield(), new ChainmailHood(), new IronArmor()] };
		hunter = new StartingClass() { name = "Hunter", color = 0xFF6c8c50, cost = 8, items = [new Shortbow(), new Arrow() { stackSize = 50 }, new HuntersHat(), new EaglesEye()] };
		thief = new StartingClass() { name = "Thief", color = 0xFF676767, cost = 8, items = [new Dagger(), new DarkHood(), new DarkCloak(), (new PoisonVial() { stackSize = 3 }).makeThrowable(), new Nightstalker()] };
		wizard = new StartingClass() { name = "Wizard", color = 0xFF73549d, cost = 8, items = [new MagicArrowStaff(), new WizardsHat(), new WizardsCloak(), new WizardsLegacy()], magic = 3 };
		fool = new StartingClass() { name = "Fool", color = 0xFFc89d3b, cost = 1, items = [new Stick(), new GlassRing()], hp = 2 };

		dev = new StartingClass() { name = "Dev", items = [new Revolver(), new AmethystRing(), new RingOfSwiftness(), new SapphireRing()] };
	}
}
