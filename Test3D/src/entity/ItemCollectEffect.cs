using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemCollectEffect : Effect
{
	public ItemCollectEffect(Entity follow)
		: base("item_collect.rfs", follow)
	{
	}
}
