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

public class ArmorEntity : Entity
{
	Armor armor;
	Cloth cloth;
	ClothCollider[] colliders;
	Vector4[] spheres;


	public ArmorEntity(Armor armor, Player player)
	{
		this.armor = armor;

		model = armor.model;
		animator = armor.model != null ? Animator.Create(armor.model) : armor.cloth != null ? Animator.Create(armor.cloth) : null;

		if (armor.cloth != null)
		{
			animator.setAnimation(player.defaultAnim);
			animator.update();
			animator.applyAnimation();

			ClothParams clothParams = new ClothParams(0);
			clothParams.inertia = 0.1f;
			clothParams.gravity = new Vector3(0, -3, 0);

			cloth = new Cloth(armor.cloth, 0, animator, player.position, player.rotation * Quaternion.FromAxisAngle(Vector3.Up, MathF.PI), clothParams);

			setSpheres(
				[new Vector4(0, 1.2f, 0, 0.2f), new Vector4(0, 0.1f, 0, 0.2f), new Vector4(0, 1.6f, 0, 0.1f)],
				[animator.model.skeleton.getNode("Chest"), animator.model.skeleton.getNode("Hips"), animator.model.skeleton.getNode("Head")]
			);
			setCapsules([new Vector2i(0, 1)]);
		}
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

		if (cloth != null)
			cloth.setTransform(position, rotation, true);
	}

	Simplex simplex = new Simplex(12345, 3);
	public override void update()
	{
		Cloth.SetWind(new Vector3(1, 0, -3) * 0.5f * MathHelper.Remap(simplex.sample1f(Time.gameTime), -1, 1, 0.9f, 1.2f));

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

		if (cloth != null)
		{
			cloth.setSpheres(spheres, 0, cloth.numSpheres);
			cloth.setTransform(position, rotation);
		}
	}

	public unsafe override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		if (cloth != null)
		{
			Renderer.DrawCloth(cloth, armor.cloth.getMaterialData(0), position, rotation);

			/*
			foreach (Vector4 sphere in spheres)
				Renderer.DrawDebugSphere(sphere.w, Matrix.CreateTranslation(rotation * (sphere.xyz) + position), 0xFFFF0000);
			*/
		}
	}
}
