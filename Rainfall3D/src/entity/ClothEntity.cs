using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct ClothCollider
{
	internal Vector4 sphere;
	internal Node parent;
	internal Matrix parentInvDefaultTransform;
}

public class ClothEntity : Entity
{
	Cloth cloth;
	ClothCollider[] colliders;
	Vector4[] spheres;


	public ClothEntity(Model model, int meshIdx, Animator animator, ClothParams clothParams)
	{
		this.model = model;
		this.animator = animator;

		cloth = new Cloth(model, meshIdx, animator, position, rotation, clothParams);
	}

	public void setSpheres(Span<Vector4> spheres, Node[] parents)
	{
		this.spheres = spheres.ToArray();
		if (parents != null)
		{
			colliders = new ClothCollider[spheres.Length];
			for (int i = 0; i < spheres.Length; i++)
			{
				colliders[i].sphere = spheres[i];
				colliders[i].parent = parents[i];
				colliders[i].parentInvDefaultTransform = animator.getNodeTransform(parents[i]).inverted;
			}
		}
	}

	public void setCapsules(Span<Vector2i> capsules)
	{
		cloth.setCapsules(capsules, 0, cloth.numCapsules);
	}

	public override void init()
	{
		base.init();

		cloth.setTransform(position, rotation, true);
	}

	Simplex simplex = new Simplex(12345, 3);
	public override void update()
	{
		//Cloth.SetWind(new Vector3(1, 0, -3) * 2.5f * MathHelper.Remap(simplex.sample1f(Time.gameTime), -1, 1, 0.9f, 1.2f));

		if (colliders != null)
		{
			for (int i = 0; i < spheres.Length; i++)
			{
				if (colliders[i].parent != null)
					spheres[i] = new Vector4(animator.getNodeTransform(colliders[i].parent) * colliders[i].parentInvDefaultTransform * colliders[i].sphere.xyz, colliders[i].sphere.w);
				else
					spheres[i] = colliders[i].sphere;
			}
		}

		cloth.setSpheres(spheres, 0, cloth.numSpheres);
		cloth.setTransform(position, rotation);
	}

	public unsafe override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawCloth(cloth, model.getMaterialData(0), position, rotation);

		foreach (Vector4 sphere in spheres)
			Renderer.DrawDebugSphere(sphere.w, Matrix.CreateTranslation(rotation * (sphere.xyz) + position), 0xFFFF0000);
	}
}
