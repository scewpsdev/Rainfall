using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	public int mana = 10;
	public int maxMana = 10;


	public Player()
	{
		sprite = new Sprite(sprites, 0, 0);

		health = 30;
		maxHealth = 30;
	}
}
