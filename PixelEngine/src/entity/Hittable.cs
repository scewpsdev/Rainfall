using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface Hittable
{
	void hit(float damage, Entity by = null, Item item = null, bool triggerInvincibility = true);
}
