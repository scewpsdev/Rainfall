using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface Interactable
{
	public bool canInteract(Player player);
	public void interact(Player player);
	public float getRange();
	public KeyCode getInput();
}
