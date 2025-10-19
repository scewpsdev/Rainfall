using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface Interactable
{
	bool canInteract(Entity entity);
	void getInteractionPrompt(Entity entity, out string text, out uint color);
	void interact(Entity entity);
}
