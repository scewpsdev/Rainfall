using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface Interactable
{
	public void interact(Player player);
	public bool canInteract(Player player) { return true; }
	public float getRange() { return 1; }
	public void onFocusEnter(Player player);
	public void onFocusLeft(Player player);
}
