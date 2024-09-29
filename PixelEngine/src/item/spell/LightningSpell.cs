using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightningSpell : Spell
{
	public LightningSpell()
		: base("lightning_spell")
	{
		displayName = "Lightning";

		value = 27;

		attackRate = 2;
		attackDamage = 2;
		manaCost = 0.25f;
		trigger = false;

		sprite = new Sprite(tileset, 3, 6);

		useSound = Resource.GetSounds("res/sounds/lightning", 4);
	}

	public override void cast(Player player, Item staff)
	{
		Vector2 position = player.position + new Vector2(0.0f, 0.5f);
		Vector2 offset = new Vector2(player.direction * 0.5f, 0.0f);

		Vector2 direction = player.lookDirection.normalized;

		GameState.instance.level.addEntity(new LightningProjectile(direction, Vector2.Zero, player, staff), position + offset);
		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);
	}
}
