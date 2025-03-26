using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface Interactable
{
	bool canInteract(Player player) => true;
	void interact(Player player);
}
