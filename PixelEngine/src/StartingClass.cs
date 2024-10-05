using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StartingClass
{
	public string name;
	public int cost;
	public Item[] items;
	public int maxHealth = 3;


	public static StartingClass barbarian;
	public static StartingClass hunter;
	public static StartingClass thief;
	public static StartingClass wizard;
	public static StartingClass fool;

	public static StartingClass dev;

	static StartingClass()
	{
		barbarian = new StartingClass() { name = "Barbarian", cost = 8, items = [new LeatherArmor(), new Handaxe(), new BerserkersChain()] };
		hunter = new StartingClass() { name = "Hunter", cost = 8, items = [new Shortbow(), new Arrow() { stackSize = 50 }, new EaglesEye()] };
		thief = new StartingClass() { name = "Thief", cost = 8, items = [new Dagger(), new DarkHood(), new DarkCloak(), (new PoisonVial() { stackSize = 3 }).makeThrowable(), new Nightstalker()] };
		wizard = new StartingClass() { name = "Wizard", cost = 8, items = [new MagicStaff(), new MagicProjectileSpell(), new WizardsHood(), new WizardsCloak(), new WizardsLegacy()] };
		fool = new StartingClass() { name = "Fool", cost = 1, items = [new Stick(), new GlassRing()], maxHealth = 2 };

		dev = new StartingClass() { name = "Dev", items = [new Revolver(), new RingOfVitality(), new RingOfSwiftness(), new AmethystRing()] };
	}
}
