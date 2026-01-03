using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface CoinTarget
{
	public void giveMoney(int amount);
	public float getCoinFollowDistance() { return 1.5f; }
}
