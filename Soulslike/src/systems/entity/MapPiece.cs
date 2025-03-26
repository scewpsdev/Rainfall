using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MapPiece : Entity
{
	public List<Entity> entities = new List<Entity>();
	public Matrix spawnPoint;


	public override void destroy()
	{
		foreach (Entity entity in entities)
			entity.remove();
	}
}
