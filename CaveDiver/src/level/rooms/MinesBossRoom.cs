using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MinesBossRoom : BossRoom
{
	public MinesBossRoom(Room room)
		: base(room, new Raya() { itemDrops = [new LostSigil()] })
	//: base(room, new Garran() { itemDrops = [new LostSigil()] })
	{
	}
}
