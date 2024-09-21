using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface Hittable
{
	bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true);
}
