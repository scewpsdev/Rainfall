using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StatusEffect
{
	public virtual void init(Player player)
	{
	}

	public virtual bool update(Player player)
	{
		return true;
	}

	public virtual void render(Player player)
	{
	}
}
