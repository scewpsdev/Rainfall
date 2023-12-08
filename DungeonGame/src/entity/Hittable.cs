using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


interface Hittable
{
	void hit(int damage, Entity from, Vector3 hitPosition, Vector3 force, int linkID);
}
