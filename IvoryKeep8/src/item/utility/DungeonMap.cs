using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DungeonMap : Item
{
	string area;


	public DungeonMap()
		: base("map", ItemType.Utility)
	{
		displayName = "Map";
		description = "Hand-drawn map that reveals nearby passages for a certain region.";

		baseValue = 27;

		canDrop = false;
		isPassiveItem = true;
		isActiveItem = false;
		isHandItem = false;
		maxUpgradeLevel = 0;
		//stackable = true;

		sprite = new Sprite(tileset, 10, 12);
	}

	public void setArea(Level level)
	{
		area = level.areaName;
		displayName = area + " Map";
		name = "map_" + area.ToLowerInvariant().Replace(" ", null);
		baseValue = 27 + level.floor * 2;
	}

	public override void onEquip(Player player)
	{
		player.collectedMaps.Add(area);
	}

	public override void onUnequip(Player player)
	{
		player.collectedMaps.Remove(area);
	}
}
