using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Longsword : Weapon
{
	public Longsword()
		: base("longsword")
	{
		displayName = "Longsword";

		baseDamage = 1.8f;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.2f;

		strengthScaling = 0.4f;
		dexterityScaling = 0.2f;

		value = 14;

		sprite = new Sprite(tileset, 0, 10, 2, 1);
		size = new Vector2(2, 1);
		icon = new Sprite(tileset.texture, 12, 10 * 16, 16, 16);
		renderOffset.x = 0.0f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}
}
