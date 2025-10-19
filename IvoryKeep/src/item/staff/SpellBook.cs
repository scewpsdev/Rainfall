using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class SpellBook : Staff
{
	public Spell spell;


	public SpellBook(Spell spell)
		: base("spell_book_" + spell.name)
	{
		this.spell = spell;

		displayName = "Spell Book";

		baseValue = 14 + spell.getValue() / 2;

		sprite = new Sprite(tileset, 1, 11);
		renderOffset.x = 0.6f;

		intelligenceScaling = 0.05f;
	}

	public override bool use(Player player)
	{
		float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
		player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));

		if (useSound != null)
			Audio.PlayOrganic(useSound, new Vector3(player.position, 0), 1, 1, 0.0f, 0.15f);

		return false;
	}
}

public class MagicArrowSpellBook : SpellBook
{
	public MagicArrowSpellBook()
		: base((Spell)GetItemPrototype("magic_arrow"))
	{
		sprite = new Sprite(tileset, 2, 11);
	}
}

public class LightningSpellBook : SpellBook
{
	public LightningSpellBook()
		: base((Spell)GetItemPrototype("lightning"))
	{
		sprite = new Sprite(tileset, 2, 11);
	}
}

public class TripleShotSpellBook : SpellBook
{
	public TripleShotSpellBook()
		: base((Spell)GetItemPrototype("triple_shot"))
	{
		sprite = new Sprite(tileset, 2, 11);
	}
}

public class BurstShotSpellBook : SpellBook
{
	public BurstShotSpellBook()
		: base((Spell)GetItemPrototype("burst_shot"))
	{
		sprite = new Sprite(tileset, 2, 11);
	}
}

public class MissileSpellBook : SpellBook
{
	public MissileSpellBook()
		: base((Spell)GetItemPrototype("missile"))
	{
		sprite = new Sprite(tileset, 5, 11);
	}
}

public class IlluminationSpellBook : SpellBook
{
	public IlluminationSpellBook()
		: base((Spell)GetItemPrototype("illumination"))
	{
		sprite = new Sprite(tileset, 7, 11);
	}
}

public class HealSpellBook : SpellBook
{
	public HealSpellBook()
		: base((Spell)GetItemPrototype("heal"))
	{
		sprite = new Sprite(tileset, 7, 11);
	}
}

public class SpectralShieldSpellBook : SpellBook
{
	public SpectralShieldSpellBook()
		: base((Spell)GetItemPrototype("spectral_shield"))
	{
		sprite = new Sprite(tileset, 7, 11);
	}
}

public class EarthSpellBook : SpellBook
{
	public EarthSpellBook()
		: base((Spell)GetItemPrototype("earth"))
	{
		sprite = new Sprite(tileset, 3, 11);
	}
}

public class TeleportationSpellBook : SpellBook
{
	public TeleportationSpellBook()
		: base((Spell)GetItemPrototype("teleportation"))
	{
		sprite = new Sprite(tileset, 5, 11);
	}
}
