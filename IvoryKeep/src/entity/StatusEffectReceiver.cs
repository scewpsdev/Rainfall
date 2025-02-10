using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface StatusEffectReceiver
{
	StatusEffect addStatusEffect(StatusEffect effect);
	void heal(float amount);
	void setVisible(bool visible);
}
