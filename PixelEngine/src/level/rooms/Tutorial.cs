using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Tutorial : Entity
{
	Room room;

	Sound caveAmbience;


	public Tutorial(Room room)
	{
		this.room = room;

		caveAmbience = Resource.GetSound("res/sounds/ambience.ogg");
	}

	public override void init(Level level)
	{
		//tutorial.addEntity(tutorialExit, (Vector2)tutorial.rooms[0].getMarker(01));

		//level.addEntity(new TutorialText(InputManager.GetBinding("Up").ToString() + InputManager.GetBinding("Left").ToString() + InputManager.GetBinding("Down").ToString() + InputManager.GetBinding("Right").ToString() + " to move", 0xFFFFFFFF), new Vector2(10, level.height - 8));
		//level.addEntity(new TutorialText(InputManager.GetBinding("Jump").ToString() + " to jump", 0xFFFFFFFF), new Vector2(14.5f, level.height - 9.5f));
		//level.addEntity(new TutorialText("Hold to jump higher", 0xFFFFFFFF), new Vector2(25.5f, level.height - 7));
		//level.addEntity(new TutorialText(InputManager.GetBinding("Down").ToString() + " to drop", 0xFFFFFFFF), new Vector2(41, level.height - 9));
		//level.addEntity(new TutorialText(InputManager.GetBinding("Up").ToString() + " to climb", 0xFFFFFFFF), new Vector2(42, level.height - 20));
		level.addEntity(new TutorialText(InputManager.GetBinding("Sprint").ToString() + " to sprint", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(01));

		level.addEntity(new TutorialText("Hug wall to wall jump", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(02));

		level.addEntity(new ItemEntity(new Stick()), (Vector2)level.rooms[0].getMarker(03) + Vector2.Up);
		level.addEntity(new ItemEntity(new IronShield()), (Vector2)level.rooms[0].getMarker(04) + Vector2.Up);
		level.addEntity(new ItemEntity(new Bomb() { stackSize = 20 }), (Vector2)level.rooms[0].getMarker(10) + new Vector2(0.5f));
		level.addEntity(new TorchEntity(), (Vector2)level.rooms[0].getMarker(11) + new Vector2(0.5f));
		//level.addEntity(new TutorialText(InputManager.GetBinding("Interact").ToString() + " to interact", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(03) + new Vector2(0, 3.5f));
		//level.addEntity(new TutorialText("Down+" + InputManager.GetBinding("Interact").ToString() + " to drop", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(03) + new Vector2(0, 3));
		//level.addEntity(new TutorialText(InputManager.GetBinding("Attack").ToString() + " to attack", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(03) + new Vector2(-9, 6));
		//level.addEntity(new TutorialText(InputManager.GetBinding("Attack2").ToString() + " to block", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(03) + new Vector2(-9, 5.5f));
		level.addEntity(new TutorialText(InputManager.GetBinding("UseItem").ToString() + " to use item", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(05) + new Vector2(0, 2.0f));
		level.addEntity(new TutorialText(InputManager.GetBinding("SwitchItem").ToString() + " to switch item", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(10) + new Vector2(0, 2.0f));
		level.addEntity(new TutorialText(InputManager.GetBinding("Inventory").ToString() + " to open inventory", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(05) + new Vector2(0, 1.5f));
		level.addEntity(new Chest(new PotionOfHealing()), (Vector2)level.rooms[0].getMarker(05));

		level.addEntity(new Rat() { itemDropChance = 0, coinDropChance = 0 }, (Vector2)level.rooms[0].getMarker(06));
		level.addEntity(new Rat() { itemDropChance = 0, coinDropChance = 0 }, (Vector2)level.rooms[0].getMarker(07));
		level.addEntity(new Rat() { itemDropChance = 0, coinDropChance = 0 }, (Vector2)level.rooms[0].getMarker(08));
		level.addEntity(new Rat() { itemDropChance = 0, coinDropChance = 0 }, (Vector2)level.rooms[0].getMarker(09));

		//level.addEntity(new ItemGate(), (Vector2)level.rooms[0].getMarker(09));

		GameState.instance.setAmbience(caveAmbience);
	}
}
