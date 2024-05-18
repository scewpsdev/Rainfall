using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


internal class Player : Entity
{
	float speed = 3;

	Model capeMesh;
	Cloth cape;

	Simplex simplex = new Simplex(12345, 3);


	public unsafe Player()
	{
		model = Resource.GetModel("res/entity/player/player.gltf");

		capeMesh = Resource.GetModel("res/entity/player/player_cape.gltf");
		float[] clothInvMasses = new float[capeMesh.getMeshData(0)->vertexCount];
		for (int i = 0; i < clothInvMasses.Length; i++)
		{
			uint color = capeMesh.getMeshData(0)->getVertexColor(i);
			float invMass = ((color & 0x0000FF00) >> 8) / 255.0f * 0.0001f;
			clothInvMasses[i] = invMass;
		}
		cape = new Cloth(capeMesh, clothInvMasses, new Vector3(0, 2, 0), Quaternion.Identity);
	}

	public override void update()
	{
		base.update();

		Vector3 delta = Vector3.Zero;
		if (Input.IsKeyDown(KeyCode.A))
			delta.x--;
		if (Input.IsKeyDown(KeyCode.D))
			delta.x++;
		if (Input.IsKeyDown(KeyCode.W))
			delta.z--;
		if (Input.IsKeyDown(KeyCode.S))
			delta.z++;

		if (delta.lengthSquared > 0)
		{
			delta = delta.normalized * speed;
			Vector3 displacement = delta * Time.deltaTime;
			//position += displacement;
		}


		cape.setTransform(position, rotation);

		Span<Vector4> spheres = [
			new Vector4(new Vector3(0, 0.5f, 0) - cape.position, 0.5f),
			new Vector4(new Vector3(0, 1.5f, 0) - cape.position, 0.5f)
		];
		//cape.setSpheres(spheres, 0, cape.numSpheres);

		Span<Vector2i> capsules = [new Vector2i(0, 1)];
		//cape.setCapsules(capsules, 0, cape.numCapsules);

		float time = Time.currentTime / 1e9f;
		Cloth.SetWind(Vector3.Zero);
		//Cloth.SetWind(new Vector3(1, 0, 1) * (simplex.sample1f(time) * 1 + 0.5f));
	}

	public override unsafe void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		Renderer.DrawCloth(cape, capeMesh.getMaterialData(0));
	}
}
