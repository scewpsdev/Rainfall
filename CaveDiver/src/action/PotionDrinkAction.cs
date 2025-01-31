using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


public class PotionDrinkAction : EntityAction
{
	public Potion potion;

	public bool useSoundPlayed = false;


	public PotionDrinkAction(Potion potion)
		: base("potion_drink", true)
	{
		this.potion = potion;

		speedMultiplier = 0.3f;

		duration = 1.5f;

		setRenderWeapon(true, potion);
	}

	public override void onFinished(Player player)
	{
		foreach (PotionEffect effect in potion.effects)
			effect.apply(player, potion);
		player.removeItemSingle(potion);

		if (potion.useSound != null)
			Audio.PlayOrganic(potion.useSound, new Vector3(player.position, 0), 1, 1, 0.0f, 0.15f);

		GlassBottle bottle = new GlassBottle();
		if (player.storedItems.Count < player.storeCapacity || player.getItem(bottle.name) != null)
			player.giveItem(bottle);
		else
			GameState.instance.level.addEntity(new ItemEntity(bottle), player.position + Vector2.Up * 0.5f);
	}

	public override Matrix getItemTransform(Player player, bool mainHand)
	{
		float rotation = elapsedTime / duration * MathF.PI * 0.75f;
		Matrix weaponTransform = Matrix.CreateScale(0.5f);
		weaponTransform = Matrix.CreateTranslation(0, -0.25f, 0) * weaponTransform;
		weaponTransform = Matrix.CreateRotation(Vector3.UnitZ, rotation) * weaponTransform;
		weaponTransform = Matrix.CreateTranslation(0.1f, 0.75f, 0) * weaponTransform;
		if (player.direction == -1)
			weaponTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * weaponTransform;
		return weaponTransform;
	}
}
